using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

namespace AStar
{
    public enum NodeType
    {
        START,      //The start node
        TARGET,     //The target node
        OPEN,       //Evaluated at least once, but not used as root node
        CLOSED,     //Already Used as root node
        PATH,       //Node that composes the path
        CORNER,     //Used as a temp marker for diagonal movement in euclidean grid
    }


    public class AStarNode
    {

        public float H { get { return h; } }
        public float G { get { return g; } }
        public float F { get { return h + g; } }
        public AStarTile Tile
        {
            get
            {
                if (m_tile != null)
                    return m_tile;
                return null;
            }
        }


        public NodeType nodeType;
        public AStarNode ParentNode { get { return m_parentNode; } }

        private float h;
        private float g;
        private AStarPath m_path;
        private AStarTile m_tile;
        private AStarNode m_parentNode;


        public AStarNode(AStarPath path, AStarTile tile, NodeType type, AStarNode parent = null)
        {
            m_path = path;
            m_parentNode = parent;
            m_tile = tile;
            nodeType = type;
            h = 0;
            g = 0;
        }

        public bool EvaluateNode(AStarNode parent, AStarNode target)
        {
            float h = m_path.Grid.NodeDistance(this, parent) + parent.H;
            float g = m_path.Grid.NodeDistance(this, target);
            float f = this.h + this.g;
            if (g + h - f > 0.0001f && f > 0.0001f)
            {
                return false;
            }
            else
            {
                m_parentNode = parent;
                this.h = h;
                this.g = g;
                return true;
            }
        }
    }
}