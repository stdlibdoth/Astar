using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AStar;

namespace AStar
{
    public class AStarManager : MonoBehaviour
    {

        //public static AStarManager ManagerSingleton { get { return m_singleton; } }
        public static Dictionary<string,AStarGrid> Grids { get { return m_grids; } }
        private static AStarManager m_singleton;
        private static Dictionary<string,AStarGrid> m_grids;

        private Transform gridsHolder;

        private void Awake()
        {
            if (!m_singleton)
            {
                m_singleton = this;
                m_grids = new Dictionary<string, AStarGrid>();
                DontDestroyOnLoad(gameObject);
            }
            else if (m_singleton != this)
            {
                Destroy(m_singleton.gameObject);
            }
        }

        private void Start()
        {
            gridsHolder = new GameObject("Grids").transform;
        }

        public static T AddGrid<T> (string id, T grid_prefab, Vector2Int halfsize) where T: AStarGrid
        {
            T g = Instantiate(grid_prefab, m_singleton.gridsHolder);
            m_grids[id] = g;
            g.InitGrid(halfsize);
            return g;
        }

        public static T AddGrid<T>(string id, T grid_prefab) where T : AStarGrid
        {
            T g = Instantiate(grid_prefab, m_singleton.gridsHolder);
            m_grids[id] = g;
            g.InitGrid();
            return g;
        }

        public static void RemoveGrid(string id)
        {
            if(m_grids.ContainsKey(id))
            {
                Destroy(m_grids[id].gameObject);
                m_grids.Remove(id);
            }
        }

    }
}
