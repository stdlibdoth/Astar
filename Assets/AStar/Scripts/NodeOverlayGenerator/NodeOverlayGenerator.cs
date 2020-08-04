using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class NodeOverlayGenerator : MonoBehaviour
{

    [SerializeField] private NodeOverlayPool m_poolPrefab;
    private static Dictionary<NodeOverlay, NodeOverlayPool> m_overlayPoolMap;

    private static NodeOverlayGenerator m_singleton = null;
    private void Awake()
    {
        m_overlayPoolMap = new Dictionary<NodeOverlay, NodeOverlayPool>();
        if (m_singleton == null)
            m_singleton = this;
        else
            Destroy(m_singleton);
    }

    public static NodeOverlay CreateNodeOverlay(NodeOverlay overlay_prefab, bool active)
    {
        if (m_overlayPoolMap == null)
            m_overlayPoolMap = new Dictionary<NodeOverlay, NodeOverlayPool>();
        NodeOverlayPool p;
        if (!m_overlayPoolMap.ContainsKey(overlay_prefab))
        {
            p = Instantiate(m_singleton.m_poolPrefab).InitPool(overlay_prefab);
            m_overlayPoolMap[overlay_prefab] = p;
        }
        else
            p = m_overlayPoolMap[overlay_prefab];
        return p.CreateOverlay(active);
    }
}
