using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using AStar;

namespace AStar
{
    public enum AgentState
    {
        IDLE,       //Initial state
        PAUSED,     //Step by step movement, waiting for resume command
        TARGETING,  //Finding destination tile
        TARGETED,   //Destination tile set and fund
        ROUTING,    //Finding path, waiting for A* algorithm to finish
        MOVING,     //Agent is moving
        ROUTED,     //A* finished
        ARRIVING,   //Arrived at destination. Sending arrived message at next frame
        ARRIVED,    //Arrived. Message sent
    }


    public abstract class MoveAgent :MonoBehaviour
    {
        protected UnityEvent m_onArrival;
        protected int m_id;

        [Header("Optional")]
        [SerializeField] protected PathOverlay m_pathOverlay = null;
        [Header("Transform to move")]
        [SerializeField] protected Transform m_moveTrans = null;
        [Header("Step by step?")]
        [SerializeField] protected bool m_descreteMovement = false;
        [Header("Path Generator")]
        [SerializeField] protected PathGenerator m_pathGenerator = null;

        private AStarPath m_tempPath;
        private AStarTile m_targetTile;
        private AStarTile m_currentTile;
        private Thread m_astarThread;
        private bool m_overlayUpdated;

        public UnityEvent OnArrival { get { return m_onArrival; } }
        public AgentState AgentState { get; private set; }
        public bool IsDiscrete
        {
            get
            {
                return m_descreteMovement;
            }
            set
            {
                m_descreteMovement = value;
            }
        }
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
        public List<AStarTile> PathTiles { get { return m_tempPath.PathTiles; } }
        public AStarTile CurrentTile
        {
            get
            {
                if (AgentState == AgentState.IDLE)
                {
                    RaycastHit r_hit;
                    if (Physics.Raycast(new Vector3(m_moveTrans.position.x, 1f, m_moveTrans.position.z), Vector3.down, out r_hit, 2f, LayerMask.GetMask("AstarTile")))
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
                if (m_tempPath != null && m_tempPath.PathTiles.Count > 1)
                    return m_tempPath.PathTiles[1];
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
            agent.CurrentTile.TileType = TileType.BLANK;
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
                    agent.CurrentTile.TileType = TileType.BLANK;
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
            }

        }

        //Callded in every frame if Agent's state is 'Moving'. Overwrite to customize the movement behavior
        protected abstract void Move();

        public void ActiveOverlay(bool active)
        {
            m_pathOverlay.ActiveOverlay(active);
            m_pathOverlay.UpdateOverlay(m_tempPath);
        }

        //for descrete movement
        public void ResumeMovement()
        {
            if (AgentState == AgentState.PAUSED)
                AgentState = AgentState.ROUTING;
            else if (AgentState == AgentState.TARGETED)
            {
                AgentState = AgentState.ROUTED;
            }
        }

        private void Awake()
        {
            AgentState = AgentState.IDLE;
            m_id = m_agentCount;
            m_agents[m_agentCount] = this;
            m_agentCount++;
            m_onArrival = new UnityEvent();
        }

        //movement state machine
        private void Update()
        {
            if (AgentState == AgentState.TARGETING)
            {
                if (m_astarThread != null)
                    m_astarThread.Abort();
                m_astarThread = new Thread(() => { });
                m_astarThread.Start();

                AStarTile s_tile = CurrentTile as AStarTile;
                s_tile.TileType = TileType.AGENT;
                s_tile.Agent = this;
                if (NextTile != null && NextTile.Agent == this && !m_descreteMovement)
                    NextTile.TileType = TileType.BLANK;
                m_tempPath = m_pathGenerator.GeneratePath(s_tile.Grid, s_tile, m_targetTile);
                if (!m_descreteMovement)
                {
                    AgentState = AgentState.ROUTED;
                }
                else
                    AgentState = AgentState.TARGETED;

                if (m_pathOverlay)
                    m_pathOverlay.UpdateOverlay(m_tempPath);
            }
            else if (AgentState == AgentState.ARRIVING)
            {
                AgentState = AgentState.ARRIVED;
                m_onArrival.Invoke();
                m_onArrival.RemoveAllListeners();
            }
            else if (AgentState != AgentState.ARRIVED && AgentState != AgentState.TARGETING && AgentState != AgentState.TARGETED)
            {
                float dis = Vector3.Distance(NextTile.transform.position, m_moveTrans.position);

                if (m_pathOverlay && AgentState == AgentState.ROUTED && m_tempPath.Successful && !m_overlayUpdated)
                {
                    m_pathOverlay.UpdateOverlay(m_tempPath);
                    m_overlayUpdated = true;
                }

                if (m_tempPath.Successful && AgentState == AgentState.ROUTED && dis >= 0.001f)
                {
                    NextTile.TileType = TileType.AGENT;
                    NextTile.Agent = this;
                    AgentState = AgentState.MOVING;
                }
                else if (m_tempPath.Successful && dis < 0.001f && AgentState == AgentState.MOVING)
                {
                    //clear last tile
                    CurrentTile.TileType = TileType.BLANK;

                    //update current tile
                    m_currentTile = m_tempPath.PathTiles[1];
                    AgentState = AgentState.ROUTING;
                    if (m_descreteMovement)
                        AgentState = AgentState.PAUSED;
                }

                if (AgentState == AgentState.MOVING)
                {
                    Move();
                }


                if (m_astarThread != null && !m_astarThread.IsAlive)
                {
                    if (m_tempPath.Successful)
                    {
                        if (CurrentTile == m_targetTile)
                            AgentState = AgentState.ARRIVING;
                        else if (dis < 0.001f && AgentState == AgentState.ROUTING && NextTile != m_targetTile)
                        {
                            CurrentTile.TileType = TileType.AGENT;
                            CurrentTile.Agent = this;

                            m_astarThread = new Thread(AstarUpdateThread1);
                            m_astarThread.Start();
                        }
                    }

                    //if stuck, keep finding path
                    else if (!m_tempPath.Successful && AgentState == AgentState.ROUTED)
                    {
                        m_astarThread = new Thread(AstarUpdateThread2);
                        m_astarThread.Start();
                    }
                }
            }
        }

        private void AstarUpdateThread1()
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();

            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile);
            m_overlayUpdated = false;
            if (m_tempPath.Successful)
            {
                NextTile.TileType = TileType.AGENT;
                NextTile.Agent = this;
            }
            else
            {
                CurrentTile.TileType = TileType.AGENT;
                CurrentTile.Agent = this;
            }

            AgentState = AgentState.ROUTED;
            //print(stopWatch.ElapsedMilliseconds);
        }

        private void AstarUpdateThread2()
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            m_tempPath = m_pathGenerator.GeneratePath(Grid, CurrentTile, m_targetTile);
            if (m_tempPath.Successful)
            {
                NextTile.TileType = TileType.AGENT;
                NextTile.Agent = this;
            }
            else
            {
                CurrentTile.TileType = TileType.AGENT;
                CurrentTile.Agent = this;
            }
            AgentState = AgentState.ROUTED;
            //print(stopWatch.ElapsedMilliseconds);
        }

        private void OnDestroy()
        {

            CurrentTile.TileType = TileType.BLANK;
            if (AgentState != AgentState.IDLE && NextTile.Agent == this)
                NextTile.TileType = TileType.BLANK;
        }
    }
}
