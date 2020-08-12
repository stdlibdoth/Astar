using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AStar;

namespace AStar
{
    public class AStarManager : MonoBehaviour
    {

        [SerializeField] private AStarSettings m_settings = null;

        //public static AStarManager ManagerSingleton { get { return m_singleton; } }

        public static AStarSettings Settings { get { return m_settingsCopy; } }
        public static Dictionary<string,AStarGrid> Grids { get { return m_grids; } }
        private static AStarManager m_singleton;
        private static Dictionary<string,AStarGrid> m_grids;
        private static AStarSettings m_settingsCopy;

        private Transform gridsHolder;

        private void Awake()
        {
            if (!m_singleton)
            {
                m_singleton = this;
                m_grids = new Dictionary<string, AStarGrid>();
                m_settingsCopy = ScriptableObject.CreateInstance("AStarSettings") as AStarSettings;
                for (int i = 0; i < m_settings.layers.Count; i++)
                {
                    m_settings.layers[i] = new AStarLayer(m_settings.layers[i].layerID, i);
                }
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

        //if no layer found, return null
        public static int GetLayer(string id)
        {
            for (int i = 0; i < m_singleton.m_settings.layers.Count; i++)
            {
                if (m_singleton.m_settings.layers[i].layerID == id)
                {
                    return i;
                }
            }
            return -1;
        }


        //The layer will be created if it doesn't exist
        public static AStarLayer SetAStarLayer(string layerID)
        {
            int index = AStarManager.GetLayer(layerID);
            if (index != -1)
            {
                return new AStarLayer(layerID,index);
            }
            AStarLayer layer = new AStarLayer(layerID, m_settingsCopy.layers.Count);
            m_settingsCopy.layers.Add(layer);
            return layer;
        }
    }
}
