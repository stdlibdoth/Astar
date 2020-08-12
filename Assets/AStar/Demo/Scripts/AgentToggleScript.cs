using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

[RequireComponent(typeof(Toggle))]
public class AgentToggleScript : MonoBehaviour
{
    [SerializeField] private Toggle m_agentToggle = null;
    [SerializeField] private Toggle m_overlayToggle = null;
    [SerializeField] private Toggle m_visibilityToggle = null;
    [SerializeField] private Toggle m_modeToggle = null;
    [SerializeField] private Slider m_speedSlider = null;
    [SerializeField] private Text m_agentText = null;

    public bool IsVisibilityToggleOn { get { return m_visibilityToggle.isOn; } }
    public bool IsOverlayToggleOn { get { return m_overlayToggle.isOn; } }
    public ConstantSpeedAgent Agent { get { return m_agent; } }

    private ConstantSpeedAgent m_agent;

    public AgentToggleScript Init(ControlPanelScript panel_script, MoveAgent agent)
    {
        if (!m_agentToggle)
            m_agentToggle = GetComponent<Toggle>();



        m_agent = (ConstantSpeedAgent)agent;
        m_agentText.text = agent.name;

        m_agentToggle.onValueChanged.AddListener((bool check) =>
        {
            if (check)
                panel_script.AddAgentToggle(this);
            Agent.ActiveNameTag(check);
        });

        m_visibilityToggle.onValueChanged.AddListener((bool visible) =>
        {
            m_agent.gameObject.SetActive(visible);
            m_overlayToggle.interactable = visible;
            if (!visible && m_overlayToggle.isOn)
                m_overlayToggle.isOn = false;
        });


        m_modeToggle.SetIsOnWithoutNotify(m_agent.oaMode == MoveAgent.OAMode.PASSIVE);

        m_modeToggle.onValueChanged.AddListener((bool passive) =>
        {
            m_agent.oaMode = passive ? MoveAgent.OAMode.PASSIVE : MoveAgent.OAMode.ACTIVE;
        });

        m_overlayToggle.onValueChanged.AddListener((bool active) =>
        {
            m_agent.ActiveOverlay(active);
        });

        m_speedSlider.value = Agent.constSpeed;

        m_speedSlider.onValueChanged.AddListener((float speed) =>
        {
            Agent.constSpeed = speed;
        });

        return this;
    }

    public void SetVisibilityToggle(bool set)
    {
        m_visibilityToggle.isOn = set;
    }

    public void SetOverlayToggle(bool set)
    {
        m_overlayToggle.isOn = set;
    }

    private void OnDisable()
    {
        m_agentToggle.group = null;
    }
}
