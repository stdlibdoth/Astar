using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;


public class UIPanelButton : MonoBehaviour
{
    [SerializeField] private RectTransform m_UIPanel = null;
    [SerializeField] private Text m_text = null;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            m_UIPanel.pivot = m_UIPanel.pivot.x == 0 ? new Vector2(1, 0.5f):new Vector2(0,0.5f);
            m_text.text = m_text.text == "+" ? "-" : "+";
        });
    }

}
