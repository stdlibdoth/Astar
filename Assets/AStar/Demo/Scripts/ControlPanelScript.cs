using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;


public enum InputState
{
    COMPLETE,
    WAITING_START,
    WAITING_TARGET,
    ADDING_BLOCK,
    SETTING_TARGET,
}


public class ControlPanelScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer m_linePrefab = null;
    [SerializeField] private AgentToggleScript m_agentTogglePrefab = null;
    [SerializeField] private UITag m_uiTag = null;
    [SerializeField] private Transform m_toggleParent = null;
    [SerializeField] private InputField m_newPathInput = null;
    [SerializeField] private Transform m_uiBlocker = null;
    [SerializeField] private Transform m_CommonTarget = null;
    [SerializeField] private InputField m_gridXInput = null;
    [SerializeField] private InputField m_gridYInput = null;

    [SerializeField] private Button m_allBtn = null;
    [SerializeField] private Button m_inverseBtn = null;
    [SerializeField] private Button m_visibilityBtn = null;
    [SerializeField] private Button m_pauseBtn = null;
    [SerializeField] private Button m_runBtn = null;
    [SerializeField] private Button m_nextBtn = null;
    [SerializeField] private Button m_overlayBtn = null;
    [SerializeField] private Button m_removeBtn = null;

    [SerializeField] private Button m_addAgentBtn = null;
    [SerializeField] private Button m_resetGridBtn = null;
    [SerializeField] private Button m_editBlockBtn = null;
    [SerializeField] private Button m_setCommonTargetBtn = null;

    [SerializeField] private GameObject m_charPrefab = null;
    [SerializeField] private GameObject m_blockPrefab = null;



    public static InputState InputState { get; set; }


    private LineRenderer m_line;
    private AStarTile m_tempStartTile;
    private HashSet<AgentToggleScript> m_agentToggles;
    private Transform m_blockHolder;
    private int m_mouseDownFrameInterval;

    private void Awake()
    {
        m_agentToggles = new HashSet<AgentToggleScript>();
    }

    private void OnEnable()
    {
        m_gridXInput.text = AStarManager.Grids[m_uiTag.CurrentPage].hSize.x.ToString();
        m_gridYInput.text = AStarManager.Grids[m_uiTag.CurrentPage].hSize.y.ToString();

        float x_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.x;
        float y_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.y;

        Camera.main.orthographicSize = x_ref > y_ref ? x_ref : y_ref;
        //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -AStarManager.Grids[m_uiTag.CurrentPage].TileSize.y * 0.5f);

        if (m_blockHolder == null)
        {
            m_blockHolder = new GameObject("block holder").transform;
            m_blockHolder.SetParent(AStarManager.Grids[m_uiTag.CurrentPage].transform);
        }

        foreach (AgentToggleScript t in m_agentToggles)
        {
            t.Agent.gameObject.SetActive(t.IsVisibilityToggleOn);
            t.Agent.ActiveOverlay(t.IsOverlayToggleOn);
        }
    }

    private void OnDisable()
    {
        foreach (AgentToggleScript t in m_agentToggles)
        {
            t.Agent.gameObject.SetActive(false);
            t.Agent.ActiveOverlay(false);
        }
    }

    private void Start()
    {
        m_newPathInput.onValueChanged.AddListener((string input) =>
        {
            m_addAgentBtn.interactable = input != "";
        });

        m_addAgentBtn.onClick.AddListener(() =>
        {
            m_newPathInput.DeactivateInputField();
            m_uiBlocker.gameObject.SetActive(true);
            InputState = InputState.WAITING_START;
        });

        m_resetGridBtn.onClick.AddListener(() =>
        {
            Destroy(m_blockHolder.gameObject);
            m_blockHolder = new GameObject("block holder").transform;
            m_blockHolder.SetParent(AStarManager.Grids[m_uiTag.CurrentPage].transform);
            AStarManager.Grids[m_uiTag.CurrentPage].OnInit.AddListener(() =>
            {
                Camera.main.orthographicSize = AStarManager.Grids[m_uiTag.CurrentPage].hSize.x * AStarManager.Grids[m_uiTag.CurrentPage].TileSize.x;
                //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -AStarManager.Grids[m_uiTag.CurrentPage].TileSize.y * 0.5f);
            });
            MoveAgent.RemoveAgents(AStarManager.Grids[m_uiTag.CurrentPage]);
            foreach (AgentToggleScript t in m_agentToggles)
            {
                Destroy(t.Agent.gameObject);
                Destroy(t.gameObject);
            }
            m_agentToggles.Clear();
            int x = Mathf.Clamp(int.Parse(m_gridXInput.text), 1, int.MaxValue);
            int y = Mathf.Clamp(int.Parse(m_gridYInput.text), 1, int.MaxValue);
            m_gridXInput.text = x.ToString();
            m_gridYInput.text = y.ToString();
            AStarGrid grid = AStarManager.Grids[m_uiTag.CurrentPage];
            grid.InitGrid(new Vector2Int(x, y));
            Transform floor = grid.transform.Find("Floor");
            floor.localScale = new Vector3(2 * x, 2 * y, 1);

            float x_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.x;
            float y_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.y;

            Camera.main.orthographicSize = x_ref > y_ref ? x_ref : y_ref;
            //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -AStarManager.Grids[m_uiTag.CurrentPage].TileSize.y * 0.5f);

        });

        m_editBlockBtn.onClick.AddListener(() =>
        {
            m_uiBlocker.GetComponentInChildren<Text>().text = "\"ESC\"=Cancel" + System.Environment.NewLine + "Left Click=Add" + System.Environment.NewLine + "Right Click=Delete";
            m_uiBlocker.gameObject.SetActive(true);
            InputState = InputState.ADDING_BLOCK;
        });

        m_allBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                Toggle to = t.GetComponent<Toggle>();
                if (to != null)
                {
                    to.isOn = true;
                }
            }
        });

        m_inverseBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                Toggle to = t.GetComponent<Toggle>();
                if (to != null)
                {
                    to.isOn = !to.isOn;
                }          
            }
        });

        m_runBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn && t.Agent.gameObject.activeSelf)
                {
                    t.GetComponent<AgentToggleScript>().Agent.descreteMovement = false;
                    t.GetComponent<AgentToggleScript>().Agent.ResumeMovement();
                }
            }
        });

        m_pauseBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn && t.Agent.gameObject.activeSelf)
                    t.GetComponent<AgentToggleScript>().Agent.Halt();
            }
        });

        m_nextBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn && t.Agent.gameObject.activeSelf)
                {
                    t.GetComponent<AgentToggleScript>().Agent.descreteMovement = true;
                    t.GetComponent<AgentToggleScript>().Agent.ResumeMovement();
                }
            }
        });

        m_removeBtn.onClick.AddListener(() =>
        {
            List<AgentToggleScript> ts = new List<AgentToggleScript>();
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn)
                {
                    MoveAgent.RemoveAgent(t.Agent);
                    Destroy(t.Agent.gameObject);
                    ts.Add(t);
                }
            }

            while (ts.Count>0)
            {
                Destroy(ts[0].gameObject);
                m_agentToggles.Remove(ts[0]);
                ts.RemoveAt(0);
            }

        });

        m_overlayBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn)
                {
                    MoveAgent agent = t.GetComponent<AgentToggleScript>().Agent;
                    t.GetComponent<AgentToggleScript>().SetOverlayToggle(!agent.IsOverlayActive);
                }
            }
        });

        m_visibilityBtn.onClick.AddListener(() =>
        {
            foreach (AgentToggleScript t in m_agentToggles)
            {
                if (t.GetComponent<Toggle>() && t.GetComponent<Toggle>().isOn)
                {
                    MoveAgent agent = t.GetComponent<AgentToggleScript>().Agent;
                    t.GetComponent<AgentToggleScript>().SetVisibilityToggle(!agent.gameObject.activeSelf);
                }
            }
        });

        m_setCommonTargetBtn.onClick.AddListener(() =>
        {
            //m_CommonTarget.gameObject.SetActive(true);
            m_uiBlocker.GetComponentInChildren<Text>().text = "\"ESC\"=Cancel" + System.Environment.NewLine + "Left Click=Set";
            m_uiBlocker.gameObject.SetActive(true);
            InputState = InputState.SETTING_TARGET;
        });

    }

    public void AddAgentToggle(AgentToggleScript toggle_script)
    {
        if (m_agentToggles == null)
            m_agentToggles = new HashSet<AgentToggleScript>();
        m_agentToggles.Add(toggle_script);
    }

    public void RemoveAgentToggle(AgentToggleScript toggle_script)
    {
        if (!m_agentToggles.Contains(toggle_script))
            return;
        m_agentToggles.Remove(toggle_script);
    }

    private void Update()
    {
        AStarTile tempTargetTile = null;

        switch (InputState)
        {
            case InputState.COMPLETE:
                break;
            case InputState.WAITING_START:
                m_uiBlocker.GetComponentInChildren<Text>().text = "\"ESC\"=Cancel" + System.Environment.NewLine + "Left Click=Start Tile";
                if (Input.GetMouseButtonDown(0))
                {
                    Ray r1 = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit1;
                    if (Physics.Raycast(r1, out hit1, LayerMask.GetMask("AstarTile")))
                    {
                        AStarTile tile = hit1.transform.GetComponent<AStarTile>();
                        if (tile && tile.Layer.layerID != "BLOCK")
                        {
                            m_tempStartTile = tile;
                            m_line = Instantiate(m_linePrefab);
                            m_line.transform.position = m_tempStartTile.transform.position;
                            InputState = InputState.WAITING_TARGET;
                        }
                    }
                }
                break;
            case InputState.WAITING_TARGET:
                Ray r2 = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit2;
                m_uiBlocker.GetComponentInChildren<Text>().text = "\"ESC\"=Cancel" + System.Environment.NewLine + "Left Click=Destination";
                if (Physics.Raycast(r2, out hit2, LayerMask.GetMask("AstarTile")))
                {
                    AStarTile tile = hit2.transform.GetComponent<AStarTile>();
                    if (tile && tile.Layer.layerID != "BLOCK" && tile != tempTargetTile)
                    {
                        m_line.SetPosition(1, m_line.transform.InverseTransformPoint(tile.transform.position) + new Vector3(0,0.03f,0));
                        if (Input.GetMouseButtonDown(0))
                        {
                            tempTargetTile = tile;
                            GameObject g = Instantiate(m_charPrefab, new Vector3(m_tempStartTile.transform.position.x, 0, m_tempStartTile.transform.position.z),Quaternion.identity);
                            g.name = m_newPathInput.text;
                            g.GetComponent<MoveAgent>().SetTarget(new Vector2Int(tempTargetTile.X, tempTargetTile.Y));
                            AgentToggleScript agentToggle = Instantiate(m_agentTogglePrefab, m_toggleParent).Init(this, g.GetComponent<MoveAgent>());
                            g.GetComponent<MoveAgent>().OnArrival.AddListener(() => 
                            {
                                MoveAgent.RemoveAgent(g.GetComponent<MoveAgent>());
                                Destroy(g);
                                Destroy(agentToggle.gameObject);
                                m_agentToggles.Remove(agentToggle);
                            });
                            m_agentToggles.Add(agentToggle);
                            m_newPathInput.text = "";
                            m_newPathInput.ActivateInputField();
                            m_uiBlocker.gameObject.SetActive(false);
                            Destroy(m_line.gameObject);
                            InputState = InputState.COMPLETE;
                        }
                    }
                }             
                break;
            case InputState.ADDING_BLOCK:
                if (Input.GetMouseButton(0))
                {
                    Ray r3 = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit3;
                    if (Physics.Raycast(r3, out hit3, 500, LayerMask.GetMask("AstarTile")))
                    {
                        AStarTile tile = hit3.transform.GetComponent<AStarTile>();
                        if (tile && tile.Layer.layerID == "BLANK")
                        {
                            if (m_mouseDownFrameInterval >= 5)
                            {
                                Transform t = Instantiate(m_blockPrefab, tile.transform.position, Quaternion.identity).transform;
                                t.localScale = new Vector3(tile.Grid.TileSize.x * 0.98f, tile.Grid.TileSize.x, tile.Grid.TileSize.y * 0.98f);
                                t.SetParent(m_blockHolder, true);
                                m_mouseDownFrameInterval = 0;
                            }
                            else
                            {
                                m_mouseDownFrameInterval++;
                            }
                        }
                    }
                }
                else if (Input.GetMouseButton(1))
                {
                    Ray r4 = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit4;
                    if (Physics.Raycast(r4, out hit4, 500, LayerMask.GetMask("AstarBlock")))
                    {
                        AstarBlock block = hit4.transform.GetComponent<AstarBlock>();
                        if (block)
                            Destroy(block.gameObject);
                    }
                }
                break;

            case InputState.SETTING_TARGET:
                Ray r5 = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit5;
                AStarTile target = null;
                if (Physics.Raycast(r5, out hit5, 500, LayerMask.GetMask("AstarTile")))
                {
                    target = hit5.transform.GetComponent<AStarTile>();
                    if (target != null)
                    {
                        m_CommonTarget.position = target.transform.position;
                        m_CommonTarget.localScale = new Vector3(target.Grid.TileSize.x, target.Grid.TileSize.x, target.Grid.TileSize.x);
                        m_CommonTarget.gameObject.SetActive(true);

                        if (Input.GetMouseButtonDown(0))
                        {
                            foreach (AgentToggleScript t in m_agentToggles)
                            {
                                if (t.GetComponent<Toggle>() != null && t.GetComponent<Toggle>().isOn)
                                {
                                    t.Agent.SetTarget(new Vector2Int(target.X, target.Y));
                                }
                            }
                            m_CommonTarget.gameObject.SetActive(false);
                            m_uiBlocker.gameObject.SetActive(false);
                            InputState = InputState.COMPLETE;
                        }
                    }
                }

                break;
            default:
                break;
        }


        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (InputState == InputState.ADDING_BLOCK || 
                InputState == InputState.WAITING_START || 
                InputState == InputState.WAITING_TARGET ||
                InputState == InputState.SETTING_TARGET)
            {
                InputState = InputState.COMPLETE;
                if(m_line!= null)
                    Destroy(m_line.gameObject);
                m_uiBlocker.gameObject.SetActive(false);
                m_CommonTarget.gameObject.SetActive(false);
            }
        }
    }

}
