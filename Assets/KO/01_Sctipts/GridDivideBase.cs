using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDivideBase : MonoBehaviour
{
    [SerializeField] protected Vector2 gridWorldSize;
    protected int gridXCnt;
    protected int gridYCnt;
    [SerializeField] protected float nodeRadius;
    [SerializeField] protected float nodeDiameter;
    protected Vector3 worldBottomLeft;
    public GridNode[,] fieldGrid;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        nodeDiameter = nodeRadius * 2;
        gridXCnt = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridYCnt = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        fieldGrid = new GridNode[gridXCnt, gridYCnt];
        worldBottomLeft = transform.position
            - Vector3.right * gridWorldSize.x / 2
            - Vector3.forward * gridWorldSize.y / 2;

        for (int i = 0; i < gridXCnt; ++i)
        {
            for (int j = 0; j < gridYCnt; ++j)
            {
                Vector3 worldPoint = worldBottomLeft
                    + (i * nodeDiameter + nodeRadius) * Vector3.right
                    + (j * nodeDiameter + nodeRadius) * Vector3.forward;

                fieldGrid[i, j] = new GridNode(worldPoint);
            }
        }
    }
    
    public bool IsContainPos(Vector3 pos)
    {
        var center = transform.position;
        float halfx = gridWorldSize.x * 0.5f;
        float halfy = gridWorldSize.y * 0.5f;

        return pos.x >= center.x - halfx && pos.x <= center.x + halfx
            && pos.z >= center.z - halfy && pos.z <= center.z + halfy;
    }

    public GridNode GetNearGrid(Vector3 pos)
    {
        float px = Mathf.InverseLerp(worldBottomLeft.x, worldBottomLeft.x + gridWorldSize.x, pos.x);
        float py = Mathf.InverseLerp(worldBottomLeft.z, worldBottomLeft.z + gridWorldSize.y, pos.z);

        int x = Mathf.RoundToInt((gridXCnt - 1) * px);
        int y = Mathf.RoundToInt((gridYCnt - 1) * py);

        x = Mathf.Clamp(x, 0, gridXCnt - 1);
        y = Mathf.Clamp(y, 0, gridYCnt - 1);

        return fieldGrid[x, y];
    }

    public void ClearChessPiece(TestingCube piece)
    {
        foreach(var node in fieldGrid)
        {
            if (node.ChessPiece == piece)
                node.ChessPiece = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (fieldGrid == null) return;
        foreach (var n in fieldGrid)
        {
            Gizmos.color = n.ChessPiece ? Color.red : Color.green;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter/2));
        }
    }
}