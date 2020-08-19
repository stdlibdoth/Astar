using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    public class RightAngleEuclideanPath : AStarPath
    {
        public RightAngleEuclideanPath(AStarGrid grid, AStarTile start, AStarTile target, MoveAgent agent) : base(grid, start, target, agent)
        {

        }

        protected override void AStar(AStarGrid grid, AStarNode startnode, AStarNode targetnode)
        {
            m_successful = false;
            if (startnode == null && targetnode == null)
                return;
            List<AStarNode> open_nodes = new List<AStarNode>();
            Dictionary<AStarTile, AStarNode> tile_nodes = new Dictionary<AStarTile, AStarNode>();
            startnode.EvaluateNode(startnode, targetnode);
            open_nodes.Add(startnode);
            tile_nodes.Add(startnode.tile, startnode);
            tile_nodes.Add(targetnode.tile, targetnode);
            AStarNode current = startnode;
            while (current != targetnode && open_nodes.Count > 0)
            {
                current = open_nodes[0];
                foreach (AStarNode node in open_nodes)
                {
                    if (node.F - current.F < -0.0001f)
                    {
                        current = node;
                    }
                    else if (node.F == current.F && node.g < current.g)
                    {
                        current = node;
                    }
                }
                open_nodes.Remove(current);
                if (current != startnode)
                {
                    current.nodeType = NodeType.CLOSED;
                }
                if (current == targetnode)
                    continue;


                //Testity surrounding tiles, take care of diagonal movements
                List<AStarTile> eval_tiles = ((EuclideanGrid)grid).GetAdjacentTilesWithoutDiagonal(current.tile);

                // Evaluate nodes
                if (current.g > 0.001f)
                {
                    foreach (var e_t in eval_tiles)
                    {
                        if (!agent.CheckObstacle(e_t) && e_t != startnode.tile)
                        {
                            if (!tile_nodes.ContainsKey(e_t) || tile_nodes[e_t] == targetnode)
                            {
                                if (!tile_nodes.ContainsKey(e_t))
                                    tile_nodes[e_t] = new AStarNode(this, e_t, NodeType.OPEN, current);

                                if (tile_nodes[e_t].nodeType == NodeType.OPEN)
                                {
                                    open_nodes.Add(tile_nodes[e_t]);
                                    tile_nodes[e_t].EvaluateNode(current, targetnode);
                                }
                                if (!m_nodes.Contains(tile_nodes[e_t]))
                                    m_nodes.Add(tile_nodes[e_t]);
                            }
                            else if (tile_nodes[e_t].nodeType == NodeType.OPEN)
                            {
                                tile_nodes[e_t].EvaluateNode(current, targetnode);
                            }
                        }
                    }
                }
            }

            targetnode.nodeType = NodeType.TARGET;
            //Back track to find the path
            if (targetnode.ParentNode != null)
            {
                AStarNode backtrack = targetnode;
                while (backtrack != startnode)
                {
                    if (backtrack != targetnode)
                    {
                        backtrack.nodeType = NodeType.PATH;
                        m_pathTiles.Insert(1, backtrack.tile);
                    }
                    backtrack = backtrack.ParentNode;
                }
                m_successful = true;
            }

        }
    }
}
