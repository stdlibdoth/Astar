using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;

namespace AStar
{
    [System.Serializable]
    public enum AgentState
    {
        IDLE,       //Initial state
        TARGETING,  //Finding destination tile
        TARGETED,   //Destination tile set and initial path found
        ROUTING,    //Finding path, waiting for A* algorithm to finish
        ROUTED,     //A* finished
        WAYPOINT,   //Arrived at waypoint.
        ARRIVED,    //Completed all waypoints. Message sent
    }

    [RequireComponent(typeof(PathGenerator))]
    public abstract class MoveAgent : MonoBehaviour
    {
        protected UnityEvent m_onArrival;
        protected int m_id;
        protected List<AStarTile> m_pathTiles;
        protected List<Vector2Int> m_waypoints;

        [Header("Astar Tile Layer Mask")]
        [SerializeField] protected LayerMask m_tileLayerMask;
        [Header("Optional")]
        [SerializeField] protected PathOverlay m_pathOverlay = null;
        [Header("Transform to move")]
        [SerializeField] protected Transform m_moveTrans = null;
        [Header("Path Generator")]
        [SerializeField] protected PathGenerator m_pathGenerator = null;
        [Header("Obstacle Avoidance Mode")]
        [SerializeField] public OAMode oaMode = OAMode.ACTIVE;
        [Header("Obstacle Settings")]
        [SerializeField] protected AgentObstacleSetting m_obstacleSetting = null;
        [Header("Pause at way point")]
        [SerializeField] public bool pauseAtWaypoint = false;
        [Header("Time interval between retrys")]
        [SerializeField] public float retryInterval = 1.5f;

        private AStarPath m_tempPath;
        private AStarTile m_targetTile;
        private AStarTile m_currentTile;
        private Thread m_astarThread;
        private bool m_overlayUpdated;
        private float m_astarTime;
        private float m_deltaTime;
        private float m_instantSpeed;
        private Vector3 m_lastLocation;
        private bool m_astarFlag;
        [SerializeField]private AStarLayer m_astarLayer;
        [SerializeField]private List<AStarLayer> m_obstacleLayers;
        private AStarTile m_activeModePrevNextTile;
        private float m_prevUnsuccessfulTime;


        public UnityEvent OnArrival { get { return m_onArrival; } }
        public AgentState AgentState { get; private set; }
        public bool IsOverlayActive
        {
            get
            {
                if (m_pathOverlay != null)
                    return m_pathOverlay.isOverlayActive;
                return false;
            }
        }
        public AStarGrid Grid { get; private set; }
        public AStarLayer Layer { get { return m_astarLayer; } set { m_astarLayer = value; } }
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
                if (m_pathTiles != null && m_pathTiles.Count > 1)
                    return m_pathTiles[1];
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

        public static T1 AddAgent<T1, T2>(GameObject g_obj) where T1 : MoveAgent where T2 : PathGenerator
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

        //remove a given agent from the system
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


        //remove all agents in a given grid
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

            List<MoveAgent> agents = new List<MoveAgent>();
            foreach (MoveAgent ag in m_agents.Values)
            {
                if (ag.Grid == grid)
                    agents.Add(ag);
            }
            return agents;
        }
        public void SetWayPoints(Vector2Int[] way_points)
        {
            if (way_points == null) return;
            if (way_points.Length == 0) return;
            m_waypoints = new List<Vector2Int>();
            for (int i = 0; i < way_points.Length; i++)
                m_waypoints.Add(way_points[i]);
            SetTarget(m_waypoints[0]);
            m_waypoints.RemoveAt(0);
        }

        public void ApendWayPoints(Vector2Int[] way_points)
        {
            if (way_points == null) return;
            for (int i = 0; i < way_points.Length; i++)
                m_waypoints.Add(way_points[i]);
        }

        protected virtual void SetTarget(Vector2Int target)
        {
            AStarTile s_tile = CurrentTile;
            if (s_tile == null) return;

            Grid = s_tile.Grid;
            m_targetTile = s_tile.Grid.GetTile(target.x, target.y);

            m_astarThread?.Abort();
            s_tile.Layer = m_astarLayer;
            s_tile.Agent = this;
            m_astarTime = 0;
            m_astarFlag = false;
            m_astarThread = new Thread(TargetRoute);
            m_astarThread.Start();
            AgentState = AgentState.TARGETING;
        }


        //Callded in every frame if at least two path tiles exist. Override to customize the move behavior
        protected abstract void MoveToNextWayPoint();

        public void ActiveOverlay(bool active)
        {
            m_pathOverlay.ActiveOverlay(active);
            m_pathOverlay.UpdateOverlay(m_tempPath);
        }

        //for descrete movement
        public void ResumeMovement()
        {
            if (AgentState == AgentState.WAYPOINT && pauseAtWaypoint)
            {
                SetTarget(m_waypoints[0]);
                m_waypoints.RemoveAt(0);
            }
            else
                m_astarFlag = true;
        }

        //pause the movement in continious mode
        public void Pause()
        {
            if (AgentState == AgentState.TARGETING || AgentState == AgentState.IDLE)
                return;
            if (Mathf.Abs(m_instantSpeed) <= float.Epsilon && m_tempPath.Successful)
                return;

            m_pathTiles?.Clear();
            m_astarFlag = false;
        }

        public void Stop()
        {
            m_pathTiles?.Clear();
            m_waypoints?.Clear();
            m_astarFlag = false;
            AgentState = AgentState.ARRIVED;
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
            if (m_pathGenerator == null)
                m_pathGenerator = GetComponent<PathGenerator>();
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

        private void FixedUpdate()
        {
            m_deltaTime = Time.fixedUnscaledDeltaTime;
            m_instantSpeed = Vector3.Distance(m_moveTrans.position, m_lastLocation) / m_deltaTime;
            m_lastLocation = m_moveTrans.position;
            if (AgentState == AgentState.ROUTING || AgentState == AgentState.TARGETING)
            {
                m_astarTime += m_deltaTime;
            }
        }

        //movement state machine
        private void Update()
        {
            if (AgentState == AgentState.WAYPOINT)
            {
                if (m_waypoints.Count == 0)
                {
                    AgentState = AgentState.ARRIVED;
                    m_onArrival.Invoke();
                    m_onArrival.RemoveAllListeners();
                }
                else if(!pauseAtWaypoint)
                {
                    SetTarget(m_waypoints[0]);
                    m_waypoints.RemoveAt(0);
                }
            }

            else if (AgentState != AgentState.ARRIVED && AgentState != AgentState.TARGETING && AgentState != AgentState.IDLE)
            {
                if (m_pathOverlay && (AgentState == AgentState.ROUTED || AgentState == AgentState.TARGETED) && m_tempPath.Successful && !m_overlayUpdated)
                {
                    m_pathOverlay.UpdateOverlay(m_tempPath);
                    m_overlayUpdated = true;
                }

                if (m_pathTiles != null)
                {
                    if (m_pathTiles.Count >= 2)
                    {
                        float dist = Vector3.Distance(m_pathTiles[1].transform.position, m_moveTrans.position);
                        if (dist < 0.01f)
                        {
                            m_pathTiles.RemoveAt(0);
                            m_currentTile = m_pathTiles[0];
                            //Passive mode. Start finding path only if the original path is blocked
                            if (oaMode == OAMode.PASSIVE)
                            {
                                m_astarFlag = false;
                                int count = m_pathTiles.Count;
                                int i = 1;
                                while (i < count)
                                {
                                    if (CheckObstacle(m_pathTiles[i]))
                                    {
                                        m_astarFlag = true;
                                        break;
                                    }
                                    i++;
                                }
                            }
                        }


                        //Active mode. Start path finding when the distance to the next tile is small enough
                        else if (oaMode == OAMode.ACTIVE)
                        {
                            if (m_pathTiles.Count >= 2 && dist <= m_instantSpeed * m_astarTime + 0.05f &&
                                m_activeModePrevNextTile != m_pathTiles[1])
                            {
                                m_astarFlag = true;
                                m_activeModePrevNextTile = m_pathTiles[1];
                            }
                            else if (m_pathTiles.Count >= 2 && m_activeModePrevNextTile == m_pathTiles[1])
                            {
                                m_astarFlag = false;
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
                        if (m_pathTiles.Count > 1 && m_pathTiles[1].Agent == null && !CheckObstacle(m_pathTiles[1]))
                        {
                            m_pathTiles[1].Agent = this;
                            m_pathTiles[1].Layer = m_astarLayer;
                        }
                    }

                    //move agent
                    if (m_pathTiles.Count >= 2)
                    {
                        MoveToNextWayPoint();
                    }
                }

                if (CurrentTile == m_targetTile)
                    AgentState = AgentState.WAYPOINT;
                else
                {
                    //start pathfinding thread
                    if (m_astarThread != null && !m_astarThread.IsAlive && m_astarFlag)
                    {
                        if (m_tempPath.Successful)
                        {
                            m_astarTime = 0;
                            AgentState = AgentState.ROUTING;
                            m_astarThread.Abort();
                            m_astarThread = new Thread(AstarWorker1);
                            m_astarThread.Start();
                        }
                        else if (!m_tempPath.Successful && Time.unscaledTime - m_prevUnsuccessfulTime > retryInterval)
                        {
                            m_astarThread.Abort();
                            m_astarThread = new Thread(AstarWorker2);
                            m_astarThread.Start();
                            m_prevUnsuccessfulTime = Time.unscaledTime;
                        }

                    }
                }
            }
        }

        private void TargetRoute()
        {
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile, this);
            m_overlayUpdated = false;

            if (m_tempPath.Successful)
            {
                m_pathTiles = new List<AStarTile>(m_tempPath.PathTiles);
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);

                m_pathTiles.RemoveRange(0, index);
            }
            else if (!m_tempPath.Successful)
            {
                //if (m_pathTiles != null)
                //{
                //    for (int i = 0; i < m_pathTiles.Count; i++)
                //    {
                //        if (m_pathTiles[i] == m_currentTile)
                //        {
                //            m_pathTiles[i].Agent = this;
                //            m_pathTiles[i].Layer = m_astarLayer;
                //            if (m_pathTiles[i + 1].Agent == this)
                //            {
                //                m_pathTiles[i + 1].Layer = m_pathTiles[i].InitialLayer;
                //                m_pathTiles[i + 1].Agent = null;
                //            }
                //            break;
                //        }
                //    }
                //}
                if (m_pathTiles != null && m_pathTiles.Count > 1)
                {
                    m_pathTiles[0].Layer = m_astarLayer;
                    m_pathTiles[0].Agent = this;
                    m_pathTiles[1].Layer = m_pathTiles[1].InitialLayer;
                }

                m_pathTiles = new List<AStarTile>();
            }
            m_astarFlag = m_tempPath.Successful ? false : true;
            AgentState = AgentState.TARGETED;
        }

        private void AstarWorker1()
        {
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile, this);
            m_overlayUpdated = false;
            if (m_tempPath.Successful)
            {
                m_pathTiles = new List<AStarTile>(m_tempPath.PathTiles);

                //remove excessive tiles in the path due to delay caused by "GeneratePath"
                int index = -1;
                do
                {
                    index++;
                } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);

                m_pathTiles.RemoveRange(0, index);
                if (m_astarTime < m_deltaTime)
                    m_astarFlag = false;
            }
            else if (!m_tempPath.Successful)
            {
                for (int i = 0; i <m_pathTiles.Count; i++)
                {
                    if(m_pathTiles[i] != m_currentTile)
                    {
                        m_pathTiles[i].Layer = m_pathTiles[i].InitialLayer;
                        m_pathTiles[i].Agent = null;
                        break;
                    }
                }
                m_pathTiles.Clear();
            }
            AgentState = AgentState.ROUTED;
        }

        private void AstarWorker2()
        {
            AgentState = AgentState.ROUTING;
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile, this);

            if (!m_tempPath.Successful) return;

            m_pathTiles = new List<AStarTile>(m_tempPath.PathTiles);
            int index = -1;
            do
            {
                index++;
            } while (index < m_tempPath.PathTiles.Count && m_tempPath.PathTiles[index] != m_currentTile);
            m_pathTiles.RemoveRange(0, index);
            if (m_pathTiles.Count > 1)
            {
                m_pathTiles[1].Agent = this;
                m_pathTiles[1].Layer = m_astarLayer;
            }

            if (m_astarTime < m_deltaTime)
                m_astarFlag = false;
        }

        private void OnDestroy()
        {
            if (AgentState == AgentState.IDLE) return;
            m_astarThread.Abort();
            CurrentTile.Layer = CurrentTile.InitialLayer;
            if (NextTile && NextTile.Agent == this)
            {
                NextTile.Layer = NextTile.InitialLayer;
                NextTile.Agent = null;
            }
        }
    }
}
