using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridDivideBase : MonoBehaviour
{
    [SerializeField] protected Vector2 gridWorldSize;
    [SerializeField] protected int gridXCnt;
    [SerializeField] protected int gridYCnt;
    [SerializeField] protected float nodeRadius;
    [SerializeField] protected float nodeDiameter;
    [SerializeField] protected Vector3 worldBottomLeft;
    protected GridNode[,] fieldGrid;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        gridWorldSize = new Vector2(transform.localScale.x, transform.localScale.z);
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
    
    public bool IsPositionInGrid(Vector3 pos)
    {
        var center = transform.position;
        float halfx = gridWorldSize.x * 0.5f;
        float halfy = gridWorldSize.y * 0.5f;

        return pos.x >= center.x - halfx && pos.x <= center.x + halfx
            && pos.z >= center.z - halfy && pos.z <= center.z + halfy;
    }

    public GridNode GetNearGrid(Vector3 pos)
    {
        GridNode res = null;
        float closest = float.PositiveInfinity;

        foreach(var node in fieldGrid)
        {
            var nodePos = new Vector2(node.worldPosition.x, node.worldPosition.z);
            var newPos = new Vector2(pos.x, pos.z);
            float dist = (nodePos - newPos).sqrMagnitude;
            
            if(dist < closest)
            {
                closest = dist;
                res = node;
            }
        }

        return res;
    }

    public void ClearChessPiece(Chess piece)
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
            Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeRadius);
        }
    }

    protected GridNode FindEmptyNode()
    {
        GridNode res = null;

        foreach(var node in fieldGrid)
        {
            if (node.ChessPiece == null)
            {
                res = node;
                break;
            }
        }

        return res;
    }
}