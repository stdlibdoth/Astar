using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using AStar;
using Battlehub.Dispatcher;

namespace AStar
{

    [RequireComponent(typeof(Collider))]
    public class AStarTile : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Material m_blankMat = null;
        [SerializeField] protected Material m_blockMat = null;

        [SerializeField] protected AStarLayer m_astarLayer;

        [SerializeField] MoveAgent m_agent;
        public MoveAgent Agent { get { return m_agent; } set { m_agent = value; } }

        private AStarLayer m_initialLayer;

        public AStarLayer InitialLayer { get { return m_initialLayer; } }

        public AStarLayer Layer
        {
            get { return m_astarLayer; }
            set
            {
                m_astarLayer = value;
                //Dispatcher.Current.BeginInvoke(() =>
                //{
                //    MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
                //    if (!mr)
                //        return;
                //    if (m_astarLayer.layerID == "BLANK")
                //        mr.material = m_blankMat;
                //    else
                //    {
                //        mr.material = m_blockMat;
                //    }
                //});
            }
        }
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public AStarGrid Grid { get { return m_grid; } }

//        protected TileType m_tileType;
        protected AStarGrid m_grid;

        public AStarTile InitTile(AStarGrid grid, int x, int y)
        {
            if(m_astarLayer.layerID != "")
                m_astarLayer = AStarManager.SetAStarLayer(m_astarLayer.layerID);
            else
                m_astarLayer = AStarManager.SetAStarLayer("BLANK");
            m_initialLayer = m_astarLayer;
            GetComponent<Collider>().isTrigger = true;
            transform.localScale = new Vector3(grid.TileSize.x, transform.localScale.y, grid.TileSize.y);
            m_grid = grid;
            X = x;
            Y = y;
            name = "(" + X + "," + Y + ")";
            return this;
        }
    }
}