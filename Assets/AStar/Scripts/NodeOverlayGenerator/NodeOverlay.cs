using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

namespace AStar
{
    public class NodeOverlay : MonoBehaviour
    {
        [SerializeField] private Transform m_arrow = null;
        [SerializeField] private TextMesh m_hText = null;
        [SerializeField] private TextMesh m_gText = null;
        [SerializeField] private TextMesh m_fText = null;

        //[SerializeField] private Canvas m_canvas = null;
        [SerializeField] private SpriteRenderer m_sRenderer = null;
        [SerializeField] private Color m_terminalColor = new Color(0, 0, 0, 0);
        [SerializeField] private Color m_openColor = new Color(0, 0, 0, 0);
        [SerializeField] private Color m_closeColor = new Color(0, 0, 0, 0);
        [SerializeField] private Color m_pathColor = new Color(0, 0, 0, 0);

        public bool pooled;
        public NodeOverlayPool pool = null;

        private void Awake()
        {
            //m_canvas.worldCamera = Camera.main;
        }

        public NodeOverlay UpdateOverlay(AStarNode node)
        {
            //((RectTransform)m_canvas.transform).sizeDelta = new Vector2(node.Tile.Grid.TileSize.x, node.Tile.Grid.TileSize.y);
            switch (node.nodeType)
            {
                case NodeType.START:
                case NodeType.TARGET:
                    m_sRenderer.color = m_terminalColor;
                    break;
                case NodeType.OPEN:
                    m_sRenderer.color = m_openColor;
                    break;
                case NodeType.CLOSED:
                    m_sRenderer.color = m_closeColor;
                    break;
                case NodeType.PATH:
                    m_sRenderer.color = m_pathColor;
                    break;
                default:
                    break;
            }

            m_hText.text = node.h.ToString("F1");
            m_gText.text = node.g.ToString("F1");
            m_fText.text = node.F.ToString("F1");
            if (node.ParentNode != null)
                m_arrow.LookAt(node.ParentNode.tile.transform);
            m_arrow.gameObject.SetActive(true);
            return this;
        }

        public void ReturnToPool()
        {
            pool.PoolOverlay(this);
        }
    }
}
