using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using AStar;
using Battlehub.Dispatcher;

namespace AStar
{
    [System.Serializable]
    public enum TileType
    {
        BLANK,
        BLOCK,
        AGENT,
        PATH,
    }

    [RequireComponent(typeof(Collider))]
    public class AStarTile : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Material m_blankMat = null;
        [SerializeField] protected Material m_blockMat = null;


        public MoveAgent Agent { get; set; }

        public TileType TileType
        {
            get { return m_tileType; }
            set
            {
                m_tileType = value;
                Dispatcher.Current.BeginInvoke(() =>
                {
                    MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
                    if (!mr)
                        return;
                    switch (value)
                    {
                        case TileType.PATH:
                        case TileType.BLANK:
                            mr.material = m_blankMat;
                            //m_tileMesh.transform.localPosition = new Vector3(0, m_tileMesh.transform.localScale.y * 0.5f, 0);
                            break;
                        case TileType.AGENT:
                        case TileType.BLOCK:
                            mr.material = m_blockMat;
                            break;
                        default:
                            break;
                    }
                });
            }
        }
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public AStarGrid Grid { get { return m_grid; } }

        protected TileType m_tileType;
        protected AStarGrid m_grid;

        public AStarTile InitTile(AStarGrid grid, int x, int y)
        {
            GetComponent<Collider>().isTrigger = true;
            transform.localScale = new Vector3(grid.TileSize.x, transform.localScale.y, grid.TileSize.y);
            m_grid = grid;
            TileType = TileType.BLANK;
            X = x;
            Y = y;
            name = "(" + X + "," + Y + ")";
            return this;
        }

    }
}