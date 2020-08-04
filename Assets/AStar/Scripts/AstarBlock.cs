using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    [RequireComponent(typeof(Rigidbody))]
    public class AstarBlock : MonoBehaviour
    {
        [SerializeField] private string m_tileTag;
        private HashSet<AStarTile> m_tiles;


        private void Awake()
        {
            m_tiles = new HashSet<AStarTile>();
            Collider[] cs = GetComponentsInChildren<Collider>();
            foreach (Collider c in cs)
            {
                c.isTrigger = true;
            }
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;
        }

        private void OnDisable()
        {
            foreach (var tile in m_tiles)
            {
                tile.TileType = TileType.BLANK;
            }
        }

        private void OnDestroy()
        {
            foreach (var tile in m_tiles)
            {
                tile.TileType = TileType.BLANK;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(m_tileTag))
            {
                AStarTile tile = other.GetComponent<AStarTile>();
                tile.TileType = TileType.BLOCK;
                m_tiles.Add(tile);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(m_tileTag))
            {
                AStarTile t = other.GetComponent<AStarTile>();
                if (t)
                {
                    m_tiles.Remove(t);
                    t.TileType = TileType.BLANK;
                }
            }
        }
    }
}
