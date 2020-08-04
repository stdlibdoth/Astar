using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GenState
{
    BLANK,
    GRID_DONE,
    MAP_DONE,
    ASTAR,
    PATH_DONE,
}

public class PathController : MonoBehaviour
{
    public int hsize_x;
    public int hsize_z;

    public Node NodePrefab;
    public static GenState genState = GenState.BLANK;

    public static Node startNode;
    public static Node targetNode;
    private Node[,] m_nodes;
    private Transform m_nodeHolder;

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            genState = GenState.BLANK;

        if(genState == GenState.BLANK)
            StartCoroutine(GenerateGrid());

        if (genState == GenState.GRID_DONE)
            CollectMapData();

        if (Input.GetKeyDown(KeyCode.P)&& genState == GenState.MAP_DONE)
        {
            print("A*");
            StartCoroutine(Astar(startNode, targetNode));
        }
    }

    private Node NodeFromGrid(Vector2Int grid)
    {
        return m_nodes[grid.x + hsize_x, grid.y + hsize_z];
    }

    private Vector2Int GridClamp (int x, int z)
    {
        return new Vector2Int(Mathf.Clamp(x, -hsize_x, hsize_x - 1), Mathf.Clamp(z, -hsize_z, hsize_z - 1));
    }
    

    private IEnumerator GenerateGrid()
    {
        if (m_nodes != null)
            Destroy(m_nodeHolder.gameObject);

        m_nodeHolder = new GameObject("node holder").transform;
        m_nodeHolder.SetParent(transform);
        startNode = null;
        targetNode = null;
        m_nodes = new Node[2 * hsize_x, 2 * hsize_z];
        for (int i = -hsize_z; i < hsize_z; i++)
        {
            for (int j = -hsize_x; j < hsize_x; j++)
            {
                m_nodes[j + hsize_x, i + hsize_z] = Instantiate<Node>(NodePrefab, m_nodeHolder).InitNode(j, i);
            }
        }
        genState = GenState.GRID_DONE;
        yield return null;
    }

    private void CollectMapData()
    {
        if (genState == GenState.GRID_DONE && Input.GetKeyDown(KeyCode.G))
        {
            int l = m_nodes.GetLength(0);
            int w = m_nodes.GetLength(1);
            for (int i = 0; i <l; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (m_nodes[i, j].nType == NodeT.START)
                        startNode = m_nodes[i, j];
                    if (m_nodes[i, j].nType == NodeT.TARGET)
                        targetNode = m_nodes[i, j];
                }
            }

            if (startNode && targetNode)
            {
                genState = GenState.MAP_DONE;
                print("map collected");
            }
        }
    }

    private IEnumerator Astar(Node startnode, Node targetnode)
    {
        if (!startnode && !targetnode)
            yield return null;
        List<Node> open_nodes = new List<Node>();
        startnode.EvaluateNode(startnode, targetnode);
        open_nodes.Add(startnode);
        Node current = startnode;
        while (current != targetnode && open_nodes.Count > 0)
        {
            current = open_nodes[0];
            foreach (var node in open_nodes)
            {
                if (node.Dist_f < current.Dist_f)
                {
                    current = node;
                }
                else if(node.Dist_f == current.Dist_f && node.Dist_g<current.Dist_g)
                {
                    current = node;
                }
            }
            open_nodes.Remove(current);
            if (current != startnode)
                current.SetNodeType(NodeT.CLOSED);

            List<Node> eval_nodes = new List<Node>();
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X, current.Z + 1)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X + 1, current.Z + 1)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X + 1, current.Z)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X + 1, current.Z - 1)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X, current.Z - 1)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X - 1, current.Z - 1)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X - 1, current.Z)));
            eval_nodes.Add(NodeFromGrid(GridClamp(current.X - 1, current.Z + 1)));
            if (eval_nodes.Contains(targetnode))
            {
                targetnode.EvaluateNode(current, targetnode);
                current = targetnode;
            }
            else
            {
                foreach (var n in eval_nodes)
                {
                    if(n.nType == NodeT.OPEN)
                        n.EvaluateNode(current, targetnode);
                    else if (n.nType == NodeT.BLANK)
                    {
                        open_nodes.Add(n);
                        n.EvaluateNode(current, targetnode);
                        n.SetNodeType(NodeT.OPEN);
                    }
                }
            }
            yield return null;
        }

        if (targetNode.parent)
        {
            Node backtrack = targetnode;
            while (backtrack != startnode)
            {
                if (backtrack != targetnode)
                    backtrack.SetNodeType(NodeT.PATH);
                backtrack = backtrack.parent;
            }
        }
        yield return null;
    }


}
