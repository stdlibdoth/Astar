using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AStar;

namespace AStar
{
    public abstract class AStarGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        public Vector2Int hSize;
        public abstract Vector2 TileSize { get; }
        public abstract Vector2 GridHBound { get; }

        [Header("References")]
        [SerializeField] protected AStarTile m_tilePrefab = null;

        public UnityEvent OnInit
        {
            get
            {
                if (m_onInit == null)
                    m_onInit = new UnityEvent();
                return m_onInit;
            }
        }

        protected AStarTile[,] m_tiles;
        protected Transform m_tilesHolder;
        protected Transform m_tilesParent;
        protected Transform m_routesHolder;
        protected UnityEvent m_onInit;


        private void Awake()
        {
            m_tilesHolder = new GameObject("Tiles Holder").transform;
            m_tilesHolder.SetParent(transform);
            m_onInit = new UnityEvent();
        }

        // override to implement the grid generation
        protected abstract IEnumerator Generate();

        public void InitGrid()
        {
            StartCoroutine(Generate());
        }

        public void InitGrid(Vector2Int halfsize)
        {
            hSize = halfsize;
            StartCoroutine(Generate());
        }

        public AStarTile GetTile(int x, int y)
        {
            return m_tiles[x + hSize.x, y + hSize.y];
        }

        public bool CheckBoundary(int x, int y)
        {
            return x >= -hSize.x && x <= hSize.x - 1 && y <= hSize.y - 1 && y >= -hSize.y;
        }

        public abstract List<AStarTile> GetAdjacentTiles(AStarTile tile);

        //public int NodeDistance(AStarNode a, AStarNode b)
        //{
        //    //return Mathf.FloorToInt(Mathf.Sqrt((a.Coord_x - b.Coord_x) * (a.Coord_x - b.Coord_x) + (a.Coord_z - b.Coord_z) * (a.Coord_z - b.Coord_z)));
        //    int diff_x = tileSize.x * Mathf.Abs(a.Tile.X - b.Tile.X);
        //    int diff_z = tileSize.y * Mathf.Abs(a.Tile.Y - b.Tile.Y);
        //    if (diff_x - diff_z == 0)
        //        return Mathf.FloorToInt(1.414f * diff_x);
        //    else if (diff_x == 0 || diff_z == 0)
        //        return diff_x + diff_z;
        //    else
        //    {
        //        int diagnal = diff_x < diff_z ? diff_x : diff_z;
        //        return Mathf.FloorToInt(diagnal * 1.414f + Mathf.Abs(diff_x - diff_z));
        //    }
        //}

        public virtual float NodeDistance(AStarNode a, AStarNode b)
        {
            return Mathf.Sqrt((a.tile.X - b.tile.X) * (a.tile.X - b.tile.X) + (a.tile.Y - b.tile.Y) * (a.tile.Y - b.tile.Y));
        }

        private void OnDestroy()
        {
            m_onInit.RemoveAllListeners();
        }
    }
}