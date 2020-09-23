using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

public class UINewGridPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject m_euclideanFloorPrefab = null;
    [SerializeField] private AStarGrid m_euclideanGridPrefab = null;
    [SerializeField] private AStarGrid m_hexagonalGridPrefab = null;
    [SerializeField] private AStarGrid m_premadeGridPrefab = null;
    [SerializeField] private RectTransform m_euclideanPanelTemplate = null;
    [SerializeField] private RectTransform m_hexagonalPanelTemplate = null;
    [SerializeField] private RectTransform m_premadePanelTemplate = null;
    [SerializeField] private RectTransform m_pressureTestPanelTemplate = null;

    [SerializeField] private UITag m_uiTag = null;
    [SerializeField] private InputField m_idInput = null;
    [SerializeField] private InputField m_xInput = null;
    [SerializeField] private InputField m_yInput = null;
    [SerializeField] private Dropdown m_gridTypeDropdown = null;
    [SerializeField] private Button m_generateBtn = null;
    [SerializeField] private Button m_cancelBtn = null;

    private AStarGrid m_premadeGrid;
    private bool m_xInputChecked;
    private bool m_yInputChecked;
    private bool m_idInputChecked;

    private void OnEnable()
    {
        m_generateBtn.interactable = false;
        m_xInputChecked = false;
        m_yInputChecked = false;
        m_idInputChecked = false;
    }

    private void Start()
    {
        m_xInput.onValueChanged.AddListener((string input) =>
        {
            m_xInputChecked = input != "";
            m_generateBtn.interactable = m_xInputChecked && m_yInputChecked && m_idInputChecked;
        });

        m_yInput.onValueChanged.AddListener((string input) =>
        {
            m_yInputChecked = input != "";
            m_generateBtn.interactable = m_xInputChecked && m_yInputChecked && m_idInputChecked;
        });

        m_idInput.onValueChanged.AddListener((string input) =>
        {
            m_idInputChecked = input != "" && !AStarManager.Grids.ContainsKey(input) && input != "Premade";
            m_generateBtn.interactable = m_xInputChecked && m_yInputChecked && m_idInputChecked;
        });


        m_gridTypeDropdown.onValueChanged.AddListener((int index) =>
        {
            if(index == 2 || index == 3)
            {
                m_idInput.text = "";
                m_xInput.text = "";
                m_yInput.text = "";
                m_idInput.interactable = false;
                m_xInput.interactable = false;
                m_yInput.interactable = false;
                m_generateBtn.interactable = true;
                if (index == 2)
                    m_generateBtn.interactable = m_premadeGrid == null;
            }
            else
            {
                m_idInput.interactable = true;
                m_xInput.interactable = true;
                m_yInput.interactable = true;
                m_generateBtn.interactable = m_xInputChecked && m_yInputChecked && m_idInputChecked;
            }
        });

        m_generateBtn.onClick.AddListener(() =>
        {
            string text_ref = "";
            if (m_gridTypeDropdown.captionText.text == "Pre-made Example")
            {
                text_ref = "Premade";
                m_premadeGrid = AStarManager.AddGrid("Premade", m_premadeGridPrefab);
                Transform tilesHolder = m_premadeGrid.transform.Find("Tiles Holder");
                tilesHolder.localPosition = new Vector3(0.5f * m_premadeGrid.TileSize.x, 0, 0.5f * m_premadeGrid.TileSize.y);
                m_uiTag.AddPage("Premade", m_premadePanelTemplate);
            }
            else if (m_gridTypeDropdown.captionText.text == "OA Pressure Test")
            {
                AStarGrid grid = AStarManager.AddGrid(m_idInput.text, m_euclideanGridPrefab, new Vector2Int(16, 16));
                Transform tilesHolder = grid.transform.Find("Tiles Holder");
                tilesHolder.localPosition = new Vector3(0.5f * grid.TileSize.x, 0, 0.5f * grid.TileSize.y);
                Transform floor = Instantiate(m_euclideanFloorPrefab).transform;
                floor.name = "Floor";
                floor.localScale = new Vector3(2 * 16, 2 * 16, 1);
                floor.SetParent(grid.transform);

                m_uiTag.AddPage(m_idInput.text, m_pressureTestPanelTemplate);
            }
            else
            {
                int x = Mathf.Clamp(int.Parse(m_xInput.text), 1, 9999);
                int y = Mathf.Clamp(int.Parse(m_yInput.text), 1, 9999);
                text_ref = m_idInput.text;
                if (m_gridTypeDropdown.captionText.text == "Euclidean")
                {
                    AStarGrid grid = AStarManager.AddGrid(m_idInput.text, m_euclideanGridPrefab, new Vector2Int(x, y));
                    Transform tilesHolder = grid.transform.Find("Tiles Holder");
                    tilesHolder.localPosition = new Vector3(0.5f * grid.TileSize.x, 0, 0.5f * grid.TileSize.y);
                    Transform floor = Instantiate(m_euclideanFloorPrefab).transform;
                    floor.name = "Floor";
                    floor.localScale = new Vector3(2 * x, 2 * y, 1);
                    floor.SetParent(grid.transform);

                    m_uiTag.AddPage(m_idInput.text, m_euclideanPanelTemplate);
                }

                else if (m_gridTypeDropdown.captionText.text == "Hexagonal")
                {
                    AStarGrid grid = AStarManager.AddGrid(m_idInput.text, m_hexagonalGridPrefab, new Vector2Int(x, y));
                    //Transform tilesHolder = grid.transform.Find("Tiles Holder");
                    Transform floor = Instantiate(m_euclideanFloorPrefab).transform;
                    floor.name = "Floor";
                    floor.localScale = new Vector3(2*grid.GridHBound.x, 2*grid.GridHBound.y, 1);
                    floor.SetParent(grid.transform);
                    floor.localPosition = new Vector3(-0.5f * Mathf.Cos(30*Mathf.Deg2Rad)* grid.TileSize.x * 0.5f, 0, -0.75f * grid.TileSize.y*0.5f);
                    m_uiTag.AddPage(m_idInput.text, m_hexagonalPanelTemplate);
                }
            }
            foreach (var item in AStarManager.Grids)
            {
                if (item.Key != text_ref)
                    item.Value.gameObject.SetActive(false);
            }

            float x_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.x;
            float y_ref = AStarManager.Grids[m_uiTag.CurrentPage].GridHBound.y;

            Camera.main.orthographicSize = x_ref > y_ref ? x_ref : y_ref;
            //
            //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, AStarManager.Grids[m_uiTag.CurrentPage].TileSize.y * 0.5f);


            gameObject.SetActive(false);

        });

        m_cancelBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        m_xInput.text = "";
        m_yInput.text = "";
        m_idInput.text = "";
        m_idInput.interactable = true;
        m_xInput.interactable = true;
        m_yInput.interactable = true;
        m_gridTypeDropdown.value = 0;
    }
}
