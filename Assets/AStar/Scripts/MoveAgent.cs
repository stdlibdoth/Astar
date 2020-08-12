using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using AStar;
using Unity.Jobs;

namespace AStar
{
    [System.Serializable]
    public enum AgentState
    {
        IDLE,       //Initial state
        TARGETING,  //Finding destination tile
        TARGETED,   //Destination tile set and fund
        ROUTING,    //Finding path, waiting for A* algorithm to finish
        ROUTED,     //A* finished
        ARRIVING,   //Arrived at destination. Sending arrived message at next frame
        ARRIVED,    //Arrived. Message sent
    }


    public abstract class MoveAgent :MonoBehaviour
    {
        protected UnityEvent m_onArrival;
        protected int m_id;
        protected List<AStarTile> m_waypointTiles;

        [Header("Astar Tile Layer Mask")]
        [SerializeField] protected LayerMask m_tileLayerMask;
        [Header("Optional")]
        [SerializeField] protected PathOverlay m_pathOverlay = null;
        [Header("Transform to move")]
        [SerializeField] protected Transform m_moveTrans = null;
        [Header("Step by step?")]
        [SerializeField] public bool descreteMovement = false;
        [Header("Path Generator")]
        [SerializeField] protected PathGenerator m_pathGenerator = null;
        [Header("Obstacle Avoidance Mode")]
        [SerializeField] public OAMode oaMode = OAMode.ACTIVE;
        [Header("Obstacle Settings")]
        [SerializeField] protected AgentObstacleSetting m_obstacleSetting = null;

        private AStarPath m_tempPath;
        private AStarTile m_targetTile;
        private AStarTile m_currentTile;
        private Thread m_astarThread;
        private bool m_overlayUpdated;
        private float m_astarTime;
        private float m_deltaTime;
        private float m_instantSpeed;
        private Vector3 m_lastLocation;
        private int m_routeCount;
        private bool m_astarFlag;
        [SerializeField]private AStarLayer m_astarLayer;
        private List<AStarLayer> m_obstacleLayers;


        public UnityEvent OnArrival { get { return m_onArrival; } }
        public AgentState AgentState { get; private set; }
        public bool IsOverlayActive
        {
            get
            {
                if (m_pathOverlay != null)
                    return m_pathOverlay.isOverlayActive;
                else
                    return false;
            }
        }
        public AStarGrid Grid { get; private set; }
        public AStarTile CurrentTile
        {
            get
            {
                if (AgentState == AgentState.IDLE)
                {
                    RaycastHit r_hit;
                    if (Physics.Raycast(new Vector3(m_moveTrans.position.x, 1f, m_moveTrans.position.z), Vector3.down, out r_hit, 2f, m_tileLayerMask))
                    {
                        m_currentTile = r_hit.transform.GetComponent<AStarTile>();
                    }
                }
                return m_currentTile;
            }
        }
        public AStarTile NextTile
        {
            get
            {
                if (m_waypointTiles != null && m_waypointTiles.Count > 1)
                    return m_waypointTiles[1];
                else
                    return null;
            }
        }
        public AStarTile TargetTile
        {
            get
            {
                return m_targetTile;
            }
        }

        private static int m_agentCount = 0;
        private static Dictionary<int, MoveAgent> m_agents = new Dictionary<int, MoveAgent>();

        [System.Serializable]
        public enum OAMode
        {
            ACTIVE,
            PASSIVE,
        }

        public static T1 AddAgent<T1,T2>(GameObject g_obj) where T1:MoveAgent where T2:PathGenerator
        {
            if (g_obj.GetComponent<MoveAgent>())
                return null;
            T1 agent = g_obj.AddComponent<T1>();
            agent.m_moveTrans = g_obj.transform;
            T2 pathGenerator = g_obj.GetComponent<PathGenerator>() as T2;
            if (pathGenerator == null)
                pathGenerator = g_obj.AddComponent<T2>();
            agent.m_pathGenerator = pathGenerator;
            return agent as T1;
        }

        public static void RemoveAgent(MoveAgent agent, bool remove_gameObject = false)
        {
            m_agents.Remove(agent.m_id);
            agent.m_pathOverlay.ActiveOverlay(false);
            agent.CurrentTile.Layer = agent.CurrentTile.InitialLayer;
            if (remove_gameObject)
                Destroy(agent.gameObject);
            else
                Destroy(agent);
        }

        public static void RemoveAgents(AStarGrid grid = null, bool remove_gameObject = false)
        {
            HashSet<int> keys = new HashSet<int>();
            foreach (MoveAgent agent in m_agents.Values)
            {
                if (agent.Grid == grid)
                {
                    MoveAgent ma = agent;
                    keys.Add(agent.m_id);
                    agent.m_pathOverlay.ActiveOverlay(false);
                    agent.CurrentTile.Layer = agent.CurrentTile.InitialLayer;
                    if (remove_gameObject)
                        Destroy(ma.gameObject);
                    else
                        Destroy(ma);
                }
            }

            foreach (var key in keys)
            {
                m_agents.Remove(key);
            }
        }

        public static List<MoveAgent> GetAgents(AStarGrid grid = null)
        {
            if (grid == null)
                return new List<MoveAgent>(m_agents.Values);
            else
            {
                List<MoveAgent> agents = new List<MoveAgent>();
                foreach (MoveAgent ag in m_agents.Values)
                {
                    if (ag.Grid == grid)
                        agents.Add(ag);
                }
                return agents;
            }
        }

        public void SetTarget(Vector2Int target, UnityAction arrival_action = null)
        {
            AStarTile s_tile = CurrentTile;
            if (s_tile != null)
            {
                Grid = s_tile.Grid;
                m_targetTile = s_tile.Grid.GetTile(target.x, target.y);
                AgentState = AgentState.TARGETING;
                if (arrival_action != null)
                    m_onArrival.AddListener(arrival_action);


                if (m_astarThread != null)
                    m_astarThread.Abort();

                s_tile.Layer = m_astarLayer;
                s_tile.Agent = this;
                m_astarTime = 0;
                m_astarThread = new Thread(TargetRoute);
                m_astarThread.Start();
            }

        }

        //Callded in every frame if at least two way point exist. Overwrite to customize the move behavior
        protected abstract void Move(List<AStarTile> way_points);

        public void ActiveOverlay(bool active)
        {
            m_pathOverlay.ActiveOverlay(active);
            m_pathOverlay.UpdateOverlay(m_tempPath);
        }

        //for descrete movement
        public void ResumeMovement()
        {
            m_astarFlag = true;
        }


        public void Halt()
        {
            if (AgentState == AgentState.TARGETING || AgentState == AgentState.IDLE)
                return;
            if (Mathf.Abs(m_instantSpeed) <= float.Epsilon && m_tempPath.Successful)
                return;

            descreteMovement = true;
            if (oaMode == OAMode.PASSIVE)
            {
                m_waypointTiles = new List<AStarTile>(m_tempPath.PathTiles);

                //remove excessive tiles in the path caused by delay
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);

                m_waypointTiles.RemoveRange(0, index);
                if (m_waypointTiles.Count > 2)
                    m_waypointTiles.RemoveRange(2, m_waypointTiles.Count - 2);
            }
            else if (oaMode == OAMode.ACTIVE)
            {
                if (m_waypointTiles != null && m_waypointTiles.Count >= 2)
                    m_astarFlag = true;
            }
        }

        //return true if the tile is within an obstacle astar layer
        public bool CheckObstacle(AStarTile tile)
        {
            for (int i = 0; i < m_obstacleLayers.Count; i++)
            {
                if (tile.Layer == m_obstacleLayers[i] && tile.Agent != this)
                    return true;
            }
            return false;
        }

        private void Awake()
        {
            AgentState = AgentState.IDLE;
            m_id = m_agentCount;
            m_agents[m_agentCount] = this;
            m_agentCount++;
            m_onArrival = new UnityEvent();
            m_astarLayer = AStarManager.SetAStarLayer(m_obstacleSetting.astarLayer.layerID);
            int count = m_obstacleSetting.obstacleAstarLayers.Count;
            m_obstacleLayers = new List<AStarLayer>();
            for (int i = 0; i < count; i++)
            {
                m_obstacleLayers.Add(AStarManager.SetAStarLayer(m_obstacleSetting.obstacleAstarLayers[i].layerID));
            }
        }

        //movement state machine
        private void Update()
        {
//            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
//            stopWatch.Start();
            m_deltaTime = Time.unscaledDeltaTime;
            m_instantSpeed = Vector3.Distance(m_moveTrans.position, m_lastLocation) / m_deltaTime;
            m_lastLocation = m_moveTrans.position;

            if (AgentState == AgentState.ROUTING || AgentState == AgentState.TARGETING)
            {
                m_astarTime += Time.unscaledDeltaTime;
            }
            if (AgentState == AgentState.ARRIVING)
            {
                AgentState = AgentState.ARRIVED;
                m_onArrival.Invoke();
                m_onArrival.RemoveAllListeners();
            }

            else if (AgentState != AgentState.ARRIVED && AgentState != AgentState.TARGETING && AgentState != AgentState.IDLE)
            {
                if (m_pathOverlay && AgentState == AgentState.ROUTED && m_tempPath.Successful && !m_overlayUpdated)
                {
                    m_pathOverlay.UpdateOverlay(m_tempPath);
                    m_overlayUpdated = true;
                }


                if(m_waypointTiles.Count>=2)
                {
                    float dist = Vector3.Distance(m_waypointTiles[1].transform.position, m_moveTrans.position);
                    if (dist < 0.01f)
                    {
                        m_waypointTiles.RemoveAt(0);
                        m_currentTile = m_waypointTiles[0];
                    }
                    
                    //Passive mode. Start finding path only if the original path is blocked
                    if(oaMode == OAMode.PASSIVE)
                    {
                        m_astarFlag = false;
                        int count = m_waypointTiles.Count;
                        int i = 1;
                        while (i < count)
                        {
                            if (CheckObstacle(m_waypointTiles[i]))
                            {
                                m_astarFlag = true;
                                break;
                            }
                            i++;
                        }
                    }

                    //Active mode. Start path finding when the distance to the next tile is small enough
                    if (!descreteMovement && oaMode == OAMode.ACTIVE)
                    {
                        if (dist <= m_instantSpeed * m_astarTime + 0.01f && m_routeCount == 0)
                        {
                            m_astarFlag = true;
                            m_routeCount++;
                        }
                        else if(dist > m_instantSpeed * m_astarTime + 0.01f)
                        {
                            m_astarFlag = false;
                            m_routeCount = 0;
                        }
                    }

                    List<AStarTile> adjacent = Grid.GetAdjacentTiles(m_currentTile);
                    foreach (AStarTile tile in adjacent)
                    {
                        if (tile.Agent == this)
                        {
                            tile.Agent = null;
                            tile.Layer = tile.InitialLayer;
                        }
                    }
                    if (m_waypointTiles.Count > 1 && m_waypointTiles[1].Agent == null)
                    {
                        m_waypointTiles[1].Agent = this;
                        m_waypointTiles[1].Layer = m_astarLayer;
                    }
                }

                //move agent
                if (m_waypointTiles.Count >= 2)
                {
                    Move(m_waypointTiles);
                }


                if (CurrentTile == m_targetTile)
                    AgentState = AgentState.ARRIVING;
                else
                {
                    //start pathfinding thread
                    if (m_astarThread != null && !m_astarThread.IsAlive && m_astarFlag)
                    {
                        if (m_tempPath.Successful)
                        {
                            m_astarTime = 0;
                            AgentState = AgentState.ROUTING;
                            m_astarThread = new Thread(AstarWorker1);
                            m_astarThread.Start();
                        }
                        else
                        {
                            m_astarThread = new Thread(AstarWorker2);
                            m_astarThread.Start();
                        }
                    }
                }
            }
           // print(stopWatch.ElapsedMilliseconds);
        }



        private void TargetRoute()
        {
            //float startTime = Time.unscaledTime;
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile, this);
            m_overlayUpdated = false;

            if (!descreteMovement && m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>(m_tempPath.PathTiles);
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);

                m_waypointTiles.RemoveRange(0, index);
            }
            else if (descreteMovement && m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>();
            }
            else if (!m_tempPath.Successful)
            {
                if (m_waypointTiles != null)
                {
                    m_waypointTiles[0].Layer = m_astarLayer;
                    m_waypointTiles[0].Agent = this;
                    m_waypointTiles[1].Layer = m_waypointTiles[1].InitialLayer;
                }
                m_waypointTiles = new List<AStarTile>();
            }
            m_astarFlag = !descreteMovement;
            //m_astarTime = Time.unscaledTime - startTime;
            if (m_astarTime < m_deltaTime && m_tempPath.Successful)
                m_astarFlag = false;
            AgentState = AgentState.TARGETED;
        }

        private void AstarWorker1()
        {
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile,this);
            m_overlayUpdated = false;
            if(m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>(m_tempPath.PathTiles);

                //remove excessive tiles in the path caused by delay
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);

                m_waypointTiles.RemoveRange(0, index);

                if (m_waypointTiles.Count > 2 && descreteMovement)
                    m_waypointTiles.RemoveRange(2, m_waypointTiles.Count - 2);
            }
            else if (!m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>();
            }
            m_astarFlag = !descreteMovement;
            if (m_astarTime < m_deltaTime && m_tempPath.Successful)
                m_astarFlag = false;
            AgentState = AgentState.ROUTED;
            //print(stopWatch.ElapsedMilliseconds);
        }

        private void AstarWorker2()
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            AgentState = AgentState.ROUTING;
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile,this);

            if (!descreteMovement && m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>(m_tempPath.PathTiles);
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);
                m_waypointTiles.RemoveRange(0, index);
            }
            else if (descreteMovement && m_tempPath.Successful)
            {
                m_waypointTiles = new List<AStarTile>();
                m_waypointTiles.Add(m_tempPath.PathTiles[0]);
                m_waypointTiles.Add(m_tempPath.PathTiles[1]);
            }

            //print(stopWatch.ElapsedMilliseconds);
        }

        private void OnDestroy()
        {
            CurrentTile.Layer = CurrentTile.InitialLayer;
            if (AgentState != AgentState.IDLE && NextTile && NextTile.Agent == this)
                NextTile.Layer = NextTile.InitialLayer;
        }
    }
}
