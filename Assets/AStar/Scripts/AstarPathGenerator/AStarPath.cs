using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AStar;

namespace AStar
{

    public abstract class AStarPath
    {

        public UnityEvent OnPathGenerated
        {
            get
            {
                return m_onPathGen;
            }
        }
        public HashSet<AStarNode> Nodes { get { return m_nodes; } }
        public AStarGrid Grid { get { return m_grid; } }
        public List<AStarTile> PathTiles { get { return m_pathTiles; } }
        public bool Successful { get { return m_successful; } }

        protected AStarGrid m_grid;
        protected UnityEvent m_onPathGen;
        protected HashSet<AStarNode> m_nodes;
        protected List<AStarTile> m_pathTiles;
        protected bool m_successful;


        public AStarPath (AStarGrid grid, AStarTile start, AStarTile target)
        {
            m_onPathGen = new UnityEvent();
            m_grid = grid;
            m_nodes = new HashSet<AStarNode>();
            m_pathTiles = new List<AStarTile>();
            m_pathTiles.Add(start);
            m_pathTiles.Add(target);

            AStarNode startNode = new AStarNode(this, start, NodeType.START);
            AStarNode targetNode = new AStarNode(this, target, NodeType.OPEN);
            m_nodes.Add(startNode);
            m_nodes.Add(targetNode);

            AStar(m_grid, startNode, targetNode);
        }

        protected abstract void AStar(AStarGrid grid, AStarNode start, AStarNode target);
    }
}