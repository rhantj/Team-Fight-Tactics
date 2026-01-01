using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class GridDivideBase : MonoBehaviour
{
    [SerializeField] protected Vector2 gridWorldSize;   // 필드 크기
    [SerializeField] protected int gridXCnt;            // 필드 위 가로 노드 개수
    [SerializeField] protected int gridYCnt;            // 필드 뒤 새로 노드 개수
    [SerializeField] protected float nodeRadius;        // 노드 반지름
    [SerializeField] protected float nodeDiameter;      // 노드 지름
    [SerializeField] protected Vector3 worldBottomLeft; // 필드 좌하단 위치
    [SerializeField] protected LineRenderer linePF;     // 노드 별 위치 표시
    [HideInInspector] public Transform lineParent;      // 관리용 라인 부모 오브젝트
    protected GridNode[,] fieldGrid;                    // 실제 필드
    protected Dictionary<int, GridNode> nodePerInt = new(); // 숫자별로 노드 접근
    public List<int> unitPerLevel = new();           // 레벨 당 유닛 제한 수

    public int CountOfPiece { get; private set; }
    public event Action<GridDivideBase, GridNode, ChessStateBase, ChessStateBase> OnGridChessPieceChanged;

    private void Awake()
    {
        Init();
    }

    protected virtual void OnEnable()
    {
        InitUnitLimits();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
    }

    private void OnDrawGizmos()
    {
        if (fieldGrid == null) return;
        foreach (var n in fieldGrid)
        {
            Gizmos.color = n.ChessPiece ? Color.red : Color.green;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeRadius);

            Handles.Label(n.worldPosition + Vector3.up * nodeDiameter, $"{n.NodeNumber}");

        }

    }
#endif

    public void Init()
    {
        if (gridWorldSize == Vector2.zero)
            gridWorldSize = new Vector2(transform.localScale.x, transform.localScale.z);
        nodeDiameter = nodeRadius * 2;
        gridXCnt = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridYCnt = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    // 필드 생성
    void CreateGrid()
    {
        // 필드 크기
        fieldGrid = new GridNode[gridYCnt, gridXCnt];
        worldBottomLeft = transform.position
            - Vector3.right * gridWorldSize.x / 2
            - Vector3.forward * gridWorldSize.y / 2;

        // 필드 생성
        for (int y = 0; y < gridYCnt; ++y)
        {
            for (int x = 0; x < gridXCnt; ++x)
            {
                // 좌하단부터 시작
                Vector3 worldPoint = worldBottomLeft
                    + (x * nodeDiameter + nodeRadius) * Vector3.right
                    + (y * nodeDiameter + nodeRadius) * Vector3.forward;

                var num = y * gridXCnt + x;
                var node = new GridNode(this,worldPoint,x,y,num);
                node.OnChessPieceChanged += NodeChessPieceChanged;

                fieldGrid[y, x] = node;
                nodePerInt.Add(num, node);
            }
        }

        if (!linePF) return;
        DrawLine();
    }

    private void NodeChessPieceChanged(GridNode node, ChessStateBase before, ChessStateBase after)
    {
        OnGridChessPieceChanged?.Invoke(this, node, before, after);
    }

    // 노드별 라인 생성
    void DrawLine()
    {
        lineParent = new GameObject("Grid Line").transform;

        float width = gridWorldSize.x;
        float height = gridWorldSize.y;

        // 원점
        Vector3 origin = worldBottomLeft;

        // 가로 라인 생성
        for (int x = 0; x < gridXCnt; ++x)
        {
            var start = origin + Vector3.right * (x * nodeDiameter);
            var end = start + Vector3.forward * height;

            MakeLine(start, end);
        }

        // 새로 라인 생성
        for (int y = 0; y < gridYCnt; ++y)
        {
            var start = origin + Vector3.forward * (y * nodeDiameter);
            var end = start + Vector3.right * width;

            MakeLine(start, end);
        }

        lineParent.gameObject.SetActive(false);
    }

    // 라인 긋기
    void MakeLine(Vector3 start, Vector3 end)
    {
        var lr = Instantiate(linePF, lineParent);
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        var offset = Vector3.up * 2f;
        lr.SetPosition(0, start + offset);
        lr.SetPosition(1, end + offset);
    }

    public GridNode GetGridNode(Vector3 pos)
    {
        float x = pos.x - worldBottomLeft.x;
        float z = pos.z - worldBottomLeft.z;

        int nx = Mathf.FloorToInt(x / nodeDiameter);
        int ny = Mathf.FloorToInt(z / nodeDiameter);

        if (nx < 0 || ny < 0 || nx >= gridXCnt || ny >= gridYCnt)
            return null;

        return fieldGrid[ny, nx];
    }

    // 정수로 노드에 접근
    public GridNode GetNodeByNumber(int num)
    {
        return nodePerInt.TryGetValue(num, out var node) ? node : null;
    }

    // 기물이 움직인 칸의 노드 정보 초기화
    public void ClearChessPiece(ChessStateBase piece)
    {
        foreach(var node in fieldGrid)
        {
            if (node.ChessPiece == piece)
                node.ChessPiece = null;
        }
    }

    // 빈 노드 찾기
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

    void InitUnitLimits()
    {
        FieldInfo baseDataField = typeof(ShopManager).GetField
            ("levelDataTable", BindingFlags.Instance | BindingFlags.NonPublic);
        LevelDataTable data = baseDataField.GetValue(ShopManager.Instance) as LevelDataTable;

        foreach(var d in data.levels)
        {
            unitPerLevel.Add(d.boardUnitLimit);
        }
    }

    public void IncreasePieceCount() => CountOfPiece++;
    public void DecreasePieceCount() => CountOfPiece = Mathf.Max(0, CountOfPiece - 1);

    public bool IsFull(int level)
    {
        return CountOfPiece >= unitPerLevel[level - 1];
    }

    public GridNode[,] FieldGrid
    {
        get { return fieldGrid; }
    }

    public void ClearNode<T>(T piece) where T : ChessStateBase
    {
        foreach(var node in fieldGrid)
        {
            if(node.ChessPiece == piece)
            {
                node.ChessPiece = null;
            }
        }
    }
}