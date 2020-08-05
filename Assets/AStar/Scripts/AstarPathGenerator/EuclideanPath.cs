using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{

    public class EuclideanPath : AStarPath
    {
        public EuclideanPath(EuclideanGrid grid, AStarTile start, AStarTile target) : base(grid, start, target)
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
                HashSet<AStarTile> eval_tiles = new HashSet<AStarTile>();
                AStarTile tile_n = null;
                AStarTile tile_s = null;
                AStarTile tile_e = null;
                AStarTile tile_w = null;
                AStarTile tile_ne = null;
                AStarTile tile_nw = null;
                AStarTile tile_se = null;
                AStarTile tile_sw = null;
                if (grid.CheckBoundary(current.tile.X, current.tile.Y + 1))
                {
                    tile_n = grid.GetTile(current.tile.X, current.tile.Y + 1);
                    eval_tiles.Add(tile_n);
                    if (tile_nodes.ContainsKey(tile_n) && tile_nodes[tile_n].nodeType == NodeType.CORNER)
                    {
                        tile_nodes[tile_n].nodeType = NodeType.OPEN;
                        open_nodes.Add(tile_nodes[tile_n]);
                    }
                }
                if (grid.CheckBoundary(current.tile.X, current.tile.Y - 1))
                {
                    tile_s = grid.GetTile(current.tile.X, current.tile.Y - 1);
                    eval_tiles.Add(tile_s);
                    if (tile_nodes.ContainsKey(tile_s) && tile_nodes[tile_s].nodeType == NodeType.CORNER)
                    {
                        tile_nodes[tile_s].nodeType = NodeType.OPEN;
                        open_nodes.Add(tile_nodes[tile_s]);
                    }
                }
                if (grid.CheckBoundary(current.tile.X + 1, current.tile.Y))
                {
                    tile_e = grid.GetTile(current.tile.X + 1, current.tile.Y);
                    eval_tiles.Add(tile_e);
                    if (tile_nodes.ContainsKey(tile_e) && tile_nodes[tile_e].nodeType == NodeType.CORNER)
                    {
                        tile_nodes[tile_e].nodeType = NodeType.OPEN;
                        open_nodes.Add(tile_nodes[tile_e]);
                    }
                }
                if (grid.CheckBoundary(current.tile.X - 1, current.tile.Y))
                {
                    tile_w = grid.GetTile(current.tile.X - 1, current.tile.Y);
                    eval_tiles.Add(tile_w);
                    if (tile_nodes.ContainsKey(tile_w) && tile_nodes[tile_w].nodeType == NodeType.CORNER)
                    {
                        tile_nodes[tile_w].nodeType = NodeType.OPEN;
                        open_nodes.Add(tile_nodes[tile_w]);
                    }
                }
                if (grid.CheckBoundary(current.tile.X + 1, current.tile.Y + 1))
                {
                    tile_ne = grid.GetTile(current.tile.X + 1, current.tile.Y + 1);
                    eval_tiles.Add(tile_ne);
                }
                if (grid.CheckBoundary(current.tile.X - 1, current.tile.Y + 1))
                {
                    tile_nw = grid.GetTile(current.tile.X - 1, current.tile.Y + 1);
                    eval_tiles.Add(tile_nw);
                }
                if (grid.CheckBoundary(current.tile.X - 1, current.tile.Y - 1))
                {
                    tile_sw = grid.GetTile(current.tile.X - 1, current.tile.Y - 1);
                    eval_tiles.Add(tile_sw);
                }
                if (grid.CheckBoundary(current.tile.X + 1, current.tile.Y - 1))
                {
                    tile_se = grid.GetTile(current.tile.X + 1, current.tile.Y - 1);
                    eval_tiles.Add(tile_se);
                }

                // Evaluate nodes
                if (current.g > 0.001f)
                {
                    foreach (var e_t in eval_tiles)
                    {
                        if (e_t.TileType != TileType.BLOCK && e_t.TileType != TileType.AGENT && e_t != startnode.tile)
                        {
                            if (!tile_nodes.ContainsKey(e_t) || tile_nodes[e_t] == targetnode)
                            {
                                if (!tile_nodes.ContainsKey(e_t))
                                    tile_nodes[e_t] = new AStarNode(this, e_t, NodeType.OPEN, current);
                                if ((e_t == tile_ne || e_t == tile_nw) && (tile_n.TileType == TileType.BLOCK || tile_n.TileType == TileType.AGENT))
                                    tile_nodes[e_t].nodeType = NodeType.CORNER;
                                if ((e_t == tile_ne || e_t == tile_se) && (tile_e.TileType == TileType.BLOCK || tile_e.TileType == TileType.AGENT))
                                    tile_nodes[e_t].nodeType = NodeType.CORNER;
                                if ((e_t == tile_se || e_t == tile_sw) && (tile_s.TileType == TileType.BLOCK || tile_s.TileType == TileType.AGENT))
                                    tile_nodes[e_t].nodeType = NodeType.CORNER;
                                if ((e_t == tile_nw || e_t == tile_sw) && (tile_w.TileType == TileType.BLOCK || tile_w.TileType == TileType.AGENT))
                                    tile_nodes[e_t].nodeType = NodeType.CORNER;

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
                                bool check = true;
                                if (tile_n && (tile_n.TileType == TileType.BLOCK || tile_n.TileType == TileType.AGENT))
                                    check = (e_t != tile_ne && e_t != tile_nw);
                                if (tile_e && (tile_e.TileType == TileType.BLOCK || tile_e.TileType == TileType.AGENT))
                                    check = check && (e_t != tile_ne && e_t != tile_se);
                                if (tile_s && (tile_s.TileType == TileType.BLOCK || tile_s.TileType == TileType.AGENT))
                                    check = check && (e_t != tile_se && e_t != tile_sw);
                                if (tile_w && (tile_w.TileType == TileType.BLOCK || tile_w.TileType == TileType.AGENT))
                                    check = check && (e_t != tile_nw && e_t != tile_sw);
                                if (check)
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
                m_onPathGen.Invoke();
                m_successful = true;
            }
        }
    }
}
