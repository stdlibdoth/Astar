using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class UITagHeaderButton : MonoBehaviour,IPointerClickHandler
{
    public class HeaderEvent : UnityEvent<UITagHeaderButton>
    {

    }

    [SerializeField] private Text m_text = null;
    [SerializeField] private Image m_image = null;
    [SerializeField] private Button m_deleteBtn = null;

    [SerializeField] private Color m_onColor = new Color(0,0,0,0);
    [SerializeField] private Color m_offColor = new Color(0,0,0,0);

    private bool m_isOn;
    private HeaderEvent m_onValueChanged;
    private UnityEvent m_onDeleteClicked;

    public string TagName { get { return m_text.text; } }
    public UnityEvent OnDeleteClicked { get { return m_onDeleteClicked; } }
    public HeaderEvent OnValueChanged { get { return m_onValueChanged; } }

    private void Awake()
    {
        m_image.color = m_isOn ? m_onColor : m_offColor;
        m_onValueChanged = new HeaderEvent();
        m_onDeleteClicked = new UnityEvent();
        m_deleteBtn.onClick.AddListener(() => m_onDeleteClicked.Invoke());
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }

    public bool isOn
    {
        get
        {
            return m_isOn;
        }
        set
        {
            if (m_isOn != value)
            {
                m_isOn = value;
                m_image.color = m_isOn ? m_onColor : m_offColor;
                m_onValueChanged.Invoke(this);
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
    }
}
