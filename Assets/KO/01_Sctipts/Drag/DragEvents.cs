using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class DragEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] GridDivideBase[] grids;
    [SerializeField] TestingCube chess;
    [SerializeField] Vector3 chessFirstPos;
    [SerializeField] GridNode targetGridNode;
    [SerializeField] GridDivideBase targetGrid;
    [SerializeField] GridNode prevNode;
    [SerializeField] GridDivideBase prevGrid;
    [SerializeField] Vector3 _worldPos;
    [SerializeField] Ray camRay;

    //[Header("CHECK VAR")]
    //public Vector3 ChessFirstPos;
    //public Vector3 TargetGridNodePosition;
    //public Vector3 PrevNodePos;
    //public bool TargetGridNodeSelection;
    //public bool PrevNodeSelection;


    private void Update()
    {
        camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        CalculateWorldPosition(camRay);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CalculateWorldChess(camRay);
        chessFirstPos = _worldPos;
        prevGrid = FindGrid(chessFirstPos);
        
        if(prevGrid)
            prevNode = prevGrid.GetNearGrid(chessFirstPos);

        prevNode.ChessPiece = null;
        if (prevNode != null)
        {
            if (prevNode.ChessPiece == null)
                prevNode.ChessPiece = chess;
        }
        else prevNode = null;
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
        if (targetGrid == null || targetGridNode == null)
        {
            if (prevNode != null) chess.SetPosition(prevNode.worldPosition);
            else chess.SetPosition(prevNode.worldPosition);
            return;
        }

        ClearAllNodeChess(chess);

        // 원래자리 그대로
        if (prevNode != null && targetGridNode == prevNode && targetGrid == prevGrid)
        {
            chess.SetPosition(targetGridNode.worldPosition);
            targetGridNode.ChessPiece = chess;
            return;
        }

        // 노드 위에 기물이 있는 경우
        TestingCube other = targetGridNode.ChessPiece;
        if (other != null && other != chess)
        {
            var to = targetGridNode.worldPosition;
            var from = prevNode.worldPosition;

            chess.SetPosition(to);
            other.SetPosition(from);

            targetGridNode.ChessPiece = chess;
            prevNode.ChessPiece = other;
        }
        else
        {
            chess.SetPosition(targetGridNode.worldPosition);
            targetGridNode.ChessPiece = chess;
        }

        prevGrid = targetGrid;
        prevNode = targetGridNode;
        chess = null;
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
                targetGridNode = targetGrid.GetNearGrid(pos);
                _worldPos = targetGridNode.worldPosition;
            }
            else
            {
                targetGridNode = null;
                _worldPos = pos;
            }
        }
    }

    void CalculateWorldChess(Ray ray)
    {
        if(Physics.Raycast(ray, out var hit, 1000f))
        {
            if(hit.transform.TryGetComponent<TestingCube>(out var ch))
            {
                Chess = ch;
            }
        }
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

    void ClearAllNodeChess(TestingCube piece)
    {
        foreach(var g in grids)
        {
            g.ClearChessPiece(piece);
        }
    }

    public TestingCube Chess
    {
        get { return chess; }
        set { chess = value; }
    }
}
