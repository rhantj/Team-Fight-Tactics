using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GridDivideBase[] grids;
    [SerializeField] ChessStateBase chess;
    [SerializeField] Vector3 chessFirstPos;
    GridNode targetNode;
    [SerializeField] GridDivideBase targetGrid;
    GridNode prevNode;
    [SerializeField] GridDivideBase prevGrid;
    [SerializeField] Vector3 _worldPos;
    [SerializeField] Ray camRay;
    public bool IsPointerOverSellArea = false;

    private void Update()
    {
        camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        CalculateWorldPosition(camRay);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverSellArea = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverSellArea = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CalculateWorldChess(camRay);
        if (!chess) return;
        chessFirstPos = _worldPos;
        prevGrid = FindGrid(chessFirstPos);
        
        if(prevGrid)
            prevNode = prevGrid.GetNearGrid(chessFirstPos);

        if (prevNode != null && !prevNode.ChessPiece)
        {
            prevNode.ChessPiece = chess;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!chess) return;
        chess.SetPosition(_worldPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!chess) return;

        // 필드 밖
        if (OutofGrid())
        {
            chess = null;
            return;
        }

        ClearAllNodeChess(chess);

        // 원래자리 그대로
        if (OnFirstNode())
        {
            chess = null;
            return;
        }

        // 노드 위에 기물이 있는 경우
        SwapPiece();

        SellPiece();

        UpdateGridAndNode();
        chess = null;
    }

    private bool OutofGrid()
    {
        if ((targetGrid == null || targetNode == null) && !IsPointerOverSellArea)
        {
            if (prevNode != null)
            {
                chess.SetPosition(prevNode.worldPosition);
                prevNode.ChessPiece = chess;
            }
            else
            {
                chess.SetPosition(chessFirstPos);
                prevNode.ChessPiece = chess;
            }

            return true;
        }
        return false;
    }

    private bool OnFirstNode()
    {
        if (prevNode != null && targetNode == prevNode && targetGrid == prevGrid && !IsPointerOverSellArea)
        {
            chess.SetPosition(targetNode.worldPosition);
            targetNode.ChessPiece = chess;

            UpdateGridAndNode();
            return true;
        }
        return false;
    }

    private void SwapPiece()
    {
        if (!targetGrid) return;
        ChessStateBase other = targetNode.ChessPiece;
        if (other != null && other != chess && !IsPointerOverSellArea)
        {
            var to = targetNode.worldPosition;
            var from = prevNode.worldPosition;

            chess.SetPosition(to);
            other.SetPosition(from);

            targetNode.ChessPiece = chess;
            prevNode.ChessPiece = other;
        }
        else
        {
            chess.SetPosition(targetNode.worldPosition);
            targetNode.ChessPiece = chess;
        }
    }

    private void UpdateGridAndNode()
    {
        prevGrid = targetGrid;
        prevNode = targetNode;
    }

    private void SellPiece()
    {
        if (!IsPointerOverSellArea || !chess) return;

        ShopManager shop = FindObjectOfType<ShopManager>();
        if (!shop) return;

        FieldInfo baseDataField = typeof(ChessStateBase).GetField
            ("baseData", BindingFlags.Instance | BindingFlags.NonPublic);
        ChessStatData chessData = baseDataField.GetValue(chess) as ChessStatData;

        Debug.LogWarning($"Chess Pool ID : {chessData.poolID}");

        shop.SellUnit(chessData, chess.gameObject);
        ClearAllNodeChess(chess);
    }

    void CalculateWorldPosition(Ray ray)
    {
        var ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out var hit))
        {
            var pos = ray.GetPoint(hit);

            targetGrid = FindGrid(pos);
            if (targetGrid)
            {
                targetNode = targetGrid.GetNearGrid(pos);
                _worldPos = targetNode.worldPosition;
            }
            else
            {
                targetNode = null;
                _worldPos = pos;
            }
        }
    }

    void CalculateWorldChess(Ray ray)
    {
        if(Physics.Raycast(ray, out var hit, 1000f))
        {
            Chess = hit.transform.GetComponentInChildren<Chess>();
            return;
        }

        Chess = null;
    }

    GridDivideBase FindGrid(Vector3 pos)
    {
        GridDivideBase grid = null;
        float dist = float.PositiveInfinity;

        if (grids.Length == 0) return null;

        foreach(var g in grids)
        {
            if (!g) continue;
            if (g.IsPositionInGrid(pos))
            {
                float closest = (pos - g.transform.position).sqrMagnitude;
                if(closest < dist)
                {
                    dist = closest;
                    grid = g;
                }
            }
        }

        return grid;
    }

    void ClearAllNodeChess(ChessStateBase piece)
    {
        foreach(var g in grids)
        {
            g.ClearChessPiece(piece);
        }
    }

    public ChessStateBase Chess
    {
        get { return chess; }
        set { chess = value; }
    }
}
