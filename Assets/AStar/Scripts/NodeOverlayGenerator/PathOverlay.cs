using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    public class PathOverlay : MonoBehaviour
    {
        [SerializeField] private NodeOverlay m_nodePrefab = null;
        [SerializeField] private LineRenderer m_pathLineOverlayPrefab = null;
        public bool isOverlayActive { get { return m_overlayFlag; } }
        private bool m_overlayFlag = false;
        //private Transform m_overlayHolder;

        private List<NodeOverlay> m_overlays;
        private LineRenderer m_lineOverlay;
        private void Awake()
        {
            m_overlays = new List<NodeOverlay>();
            m_lineOverlay = Instantiate(m_pathLineOverlayPrefab);
        }

        public void UpdateOverlay(AStarPath path)
        {
            if (m_overlayFlag)
            {
                //m_lineOverlay?.gameObject.SetActive(false);
                while (m_overlays.Count>0)
                {
                    m_overlays[0].ReturnToPool();
                    m_overlays.RemoveAt(0);
                }

                foreach (var node in path.Nodes)
                {
                    if (node.nodeType != NodeType.CORNER)
                    {
                            NodeOverlay n = NodeOverlayGenerator.CreateNodeOverlay(m_nodePrefab, false);
                            m_overlays.Add(n);
                            n.transform.position = node.tile.transform.position;
                            n.gameObject.SetActive(true);
                            n.UpdateOverlay(node);

                    }
                }
                if (path.Successful)
                {
                    m_lineOverlay.startWidth = path.Grid.TileSize.x*0.15f;
                    m_lineOverlay.endWidth = path.Grid.TileSize.x*0.15f;
                    m_lineOverlay.positionCount = path.PathTiles.Count;
                    Vector3[] pos = new Vector3[path.PathTiles.Count];
                    for (int i = 0; i < path.PathTiles.Count; i++)
                    {
                        pos[i] = path.PathTiles[i].transform.position;
                    }

                    m_lineOverlay.SetPositions(pos);
                    m_lineOverlay.gameObject.SetActive(true);
                }
                else
                    m_lineOverlay.gameObject.SetActive(false);

            }
        }

        public void ActiveOverlay(bool active)
        {
            if (!active)
            {
                m_lineOverlay.gameObject.SetActive(false);
                while (m_overlays.Count > 0)
                {
                    m_overlays[0].ReturnToPool();
                    m_overlays.RemoveAt(0);
                }
            }
            m_overlayFlag = active;
        }

        private void OnDestroy()
        {
            Destroy(m_lineOverlay.gameObject);
        }
    }
}
