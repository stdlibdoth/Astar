using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public enum NodeT
{
    BLANK,
    START,
    TARGET,
    BLOCK,
    OPEN,
    CLOSED,
    PATH,
}

public class Node:MonoBehaviour, IPointerDownHandler
{
    public Transform arrow;
    public static int size = 10;
    public Text h_text;
    public Text g_text;
    public Text f_text;

    public Material blank_mat;
    public Material open_mat;
    public Material close_mat;
    public Material path_mat;
    public Material terminal_mat;
    public Material block_mat;


    public NodeT nType;
    public Node parent;
    public int X { get; private set; }
    public int Z { get; private set; }
    public int Dist_h { get; private set; }
    public int Dist_g { get; private set; }
    public int Dist_f { get { return Dist_h + Dist_g; } }


    private MeshRenderer m_meshRenderer;

    public static int Distance(Node a, Node b)
    {
        //return Mathf.FloorToInt(Mathf.Sqrt((a.Coord_x - b.Coord_x) * (a.Coord_x - b.Coord_x) + (a.Coord_z - b.Coord_z) * (a.Coord_z - b.Coord_z)));
        int diff_x = size * Mathf.Abs(a.X - b.X);
        int diff_z = size * Mathf.Abs(a.Z - b.Z);
        if (diff_x - diff_z == 0)
            return Mathf.FloorToInt(1.414f * diff_x);
        else if (diff_x == 0 || diff_z == 0)
            return diff_x + diff_z;
        else
        {
            int diagnal = diff_x < diff_z ? diff_x : diff_z;
            return Mathf.FloorToInt(diagnal * 1.414f + Mathf.Abs(diff_x - diff_z));
        }
    }

    public Node InitNode(int x, int z)
    {
        nType = NodeT.BLANK;
        X = x;
        Z = z;
        h_text.text = "";
        g_text.text = "";
        f_text.text = "";
        transform.localPosition = new Vector3(x * size, 0, z * size);
        return this;
    }

    public void SetNodeType(NodeT type)
    {
        nType = type;
        switch (type)
        {
            case NodeT.BLANK:
                GetComponentInChildren<MeshRenderer>().material = blank_mat;
                if (PathController.startNode == this)
                    PathController.startNode = null;
                if(PathController.targetNode == this)
                    PathController.targetNode = null;
                break;
            case NodeT.START:
                GetComponentInChildren<MeshRenderer>().material = terminal_mat;
                PathController.startNode = this;
                break;
            case NodeT.TARGET:
                GetComponentInChildren<MeshRenderer>().material = terminal_mat;
                PathController.targetNode = this;
                break;
            case NodeT.BLOCK:
                GetComponentInChildren<MeshRenderer>().material = block_mat;
                break;
            case NodeT.OPEN:
                GetComponentInChildren<MeshRenderer>().material = open_mat;
                break;
            case NodeT.CLOSED:
                GetComponentInChildren<MeshRenderer>().material = close_mat;
                break;
            case NodeT.PATH:
                GetComponentInChildren<MeshRenderer>().material = path_mat;
                break;
            default:
                break;
        }
    }

    public bool EvaluateNode(Node parent, Node target)
    {
        int h = Distance(this,parent) + parent.Dist_h;
        int g = Distance(this, target);
        if (g + h >= Dist_f && Dist_f != 0)
            return false;
        else
        {
            this.parent = parent;
            Dist_h = h;
            Dist_g = g;
            h_text.text = Dist_h.ToString();
            g_text.text = Dist_g.ToString();
            f_text.text = Dist_f.ToString();
            arrow.LookAt(parent.transform);
            arrow.gameObject.SetActive(true);
            return true;
        }
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    print(eventData.pointerEnter.name + "X: " + X + "   Z: " + Z);
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PathController.genState == GenState.GRID_DONE)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(!PathController.startNode)
                    SetNodeType(NodeT.START);
                else if (!PathController.targetNode)
                    SetNodeType(NodeT.TARGET);
            }
            else if (Input.GetMouseButton(1))
            {
                SetNodeType(NodeT.BLANK);
            }
            else if (Input.GetMouseButton(2))
            {
                SetNodeType(NodeT.BLOCK);
            }
        }
    }
}
