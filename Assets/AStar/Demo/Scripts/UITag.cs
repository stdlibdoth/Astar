using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

public class UITag : MonoBehaviour
{

    [SerializeField] private RectTransform m_headerContainer = null;
    [SerializeField] private RectTransform m_pageContainer = null;
    [SerializeField] private UINewGridPanelScript m_newGridPanel = null;

    [SerializeField] private Button m_newGridBtn = null;
    [SerializeField] private UITagHeaderButton m_tagHeaderBtnPrefab = null;

    [SerializeField] private GameObject m_premadeLevel = null;

    private List<UITagHeaderButton> m_tagHeaders = null;
    private Dictionary<string, RectTransform> m_tagPages = null;

    public string CurrentPage
    {
        get
        {
            foreach (var item in m_tagHeaders)
            {
                if (item.isOn)
                {
                    return item.TagName;
                }
            }
            return null;
        }
    }

    private void Awake()
    {
        m_tagHeaders = new List<UITagHeaderButton>();
        m_tagPages = new Dictionary<string, RectTransform>();
    }

    private void Start()
    {
        m_newGridBtn.onClick.AddListener(() => m_newGridPanel.gameObject.SetActive(true));
    }

    public UITagHeaderButton AddPage(string header, RectTransform content_panel_template)
    {
        UITagHeaderButton btn = Instantiate(m_tagHeaderBtnPrefab, m_headerContainer);
        btn.SetText(header);
        RectTransform content = Instantiate(content_panel_template, m_pageContainer);
        btn.OnValueChanged.AddListener(OnAnyHeaderPressed);
        btn.OnDeleteClicked.AddListener(() => { RemovePage(btn.TagName); MoveAgent.RemoveAgents(AStarManager.Grids[btn.TagName], true); AStarManager.RemoveGrid(btn.TagName); });

        m_tagHeaders.Add(btn);
        m_tagPages[header] = content;
        btn.isOn = true;

        if (header == "Premade")
            m_premadeLevel.SetActive(true);
        return btn;
    }

    public void RemovePage(string header)
    {
        if (!m_tagPages.ContainsKey(header))
            return;
        RectTransform rt = m_tagPages[header];
        UITagHeaderButton h = m_tagHeaders.Find(x => x.TagName == header);
        m_tagHeaders.Remove(h);
        m_tagPages.Remove(header);
        Destroy(h.gameObject);
        Destroy(rt.gameObject);

        if (header == "Premade")
            m_premadeLevel.SetActive(false);
    }

    public RectTransform GetPage(string header)
    {
        if(m_tagPages.ContainsKey(header))
            return m_tagPages[header];
        else
            return null;
    }

    private void OnAnyHeaderPressed(UITagHeaderButton btn)
    {
        if(btn.isOn)
        {
            foreach (var header in m_tagHeaders)
            {
                if (header != btn)
                    header.isOn = false;
            }
        }

        foreach (UITagHeaderButton item in m_tagHeaders)
            m_tagPages[item.TagName].gameObject.SetActive(item.isOn);

        foreach (var gridpair in AStarManager.Grids)
            gridpair.Value.gameObject.SetActive(m_tagPages[gridpair.Key].gameObject.activeSelf);

        m_premadeLevel.gameObject.SetActive(btn.TagName == "Premade" && btn.isOn);
    }

}
