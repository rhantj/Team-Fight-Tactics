using UnityEngine;

public class GridDivideBase : MonoBehaviour
{
    [SerializeField] protected Vector2 gridWorldSize;
    protected int gridXCnt;
    protected int gridYCnt;
    [SerializeField] protected float nodeRadius;
    [SerializeField] protected float nodeDiameter;
    protected Vector3 worldBottomLeft;
    protected GridNode[,] fieldGrid;

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
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (fieldGrid == null) return;
        foreach (var n in fieldGrid)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 1f));
        }
    }
}