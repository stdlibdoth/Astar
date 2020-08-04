using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class NodeOverlayPool : MonoBehaviour
{

    private NodeOverlay m_overlayPrefabRef;
    private List<NodeOverlay> m_pooledOverlays;

    public NodeOverlayPool InitPool(NodeOverlay overlay_prefab_ref)
    {
        m_pooledOverlays = new List<NodeOverlay>();
        m_overlayPrefabRef = overlay_prefab_ref;
        return this;
    }


    public bool CheckPoolType(NodeOverlay overlay_ref)
    {
        return overlay_ref == m_overlayPrefabRef;
    }

    public NodeOverlay CreateOverlay(bool active)
    {
        NodeOverlay overlay;
        if (m_pooledOverlays.Count > 0)
        {
            overlay = m_pooledOverlays[0];
            m_pooledOverlays.RemoveAt(0);
        }
        else
        {
            overlay = Instantiate(m_overlayPrefabRef);
            overlay.pool = this;
        }
        //if (parent != null)
        //    overlay.transform.SetParent(parent);
        overlay.gameObject.SetActive(true);
        overlay.pooled = false;
        if (active)
            overlay.gameObject.SetActive(active);
        return overlay;
    }


    public void PoolOverlay(NodeOverlay overlay)
    {
        overlay.gameObject.SetActive(false);
        //overlay.transform.SetParent(transform);
        overlay.transform.localPosition = Vector3.zero;
        m_pooledOverlays.Add(overlay);
        overlay.pooled = true;
    }
}
