using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;


[RequireComponent(typeof(Camera))]
public class CamControlScript : MonoBehaviour
{
    [SerializeField] private Camera m_cam = null;
    [SerializeField] private UITag m_uiTag = null;
    [SerializeField] [Range(0,0.2f)]private float m_panSensibility = 0.1f;
    [SerializeField] [Range(0,1)]private float m_zoomSensibility = 1f;
    [SerializeField] private int m_zoomMin = 5;

    private int m_camPanState = 0;
    private Vector3 m_mouseLastFramePos;

    private void Update()
    {

        if (Input.GetMouseButton(2))
        {
            if (m_camPanState == 0)
            {
                m_camPanState = 1;
                m_mouseLastFramePos = Input.mousePosition;
            }
        }
        else
        {
            m_camPanState = 0;
        }

        if(m_camPanState == 1 && m_uiTag.CurrentPage != null)
        {
            AStarGrid grid = AStarManager.Grids[m_uiTag.CurrentPage];
            Vector3 delta = Input.mousePosition - m_mouseLastFramePos;
            float boundRef = grid.GridHBound.x > grid.GridHBound.x ? grid.GridHBound.x : grid.GridHBound.y;
            float index = m_panSensibility * m_cam.orthographicSize * 0.01f;

            Vector3 destination = Vector3.MoveTowards(transform.position, transform.position - new Vector3(delta.x, 0, delta.y) * index, float.MaxValue);
            float x = Mathf.Clamp(destination.x, -grid.GridHBound.x, grid.GridHBound.x);
            float z = Mathf.Clamp(destination.z, -grid.GridHBound.y, grid.GridHBound.y);
            transform.position = new Vector3(x, transform.position.y, z);
            m_mouseLastFramePos = Input.mousePosition;
        }

        if (m_camPanState == 0 && Input.mouseScrollDelta.y != 0)
        {
            if (m_uiTag.CurrentPage == null) return;

            float camsize = m_cam.orthographicSize - Input.mouseScrollDelta.y * m_zoomSensibility;
            camsize = Mathf.Clamp(camsize, m_zoomMin, 1.2f * AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.y);
            m_cam.orthographicSize = camsize;
        }
    }
}
