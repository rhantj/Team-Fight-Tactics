using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGrid : GridDivideBase
{
    // 필드 위의 전체 기물 리스트
    public List<ChessStateBase> GetAllFieldUnits()
    {
        List<ChessStateBase> result = new();

        foreach(var node in fieldGrid)
        {
            if(node.ChessPiece != null)
            {
                result.Add(node.ChessPiece);
            }
        }
        return result;
    }

    // 필드에 배치할 때
    /*
    public void SetChessOnFieldNode(Chess piece, GridNode node)
    {
        if (piece == null || node == null) return;
        node.ChessPiece = piece;
        piece.SetPosition(node.worldPosition);
        piece.SetOnField(true); // 필드에 있음을 표시
    }
     */

    public void ResetAllNode()
    {
        if (fieldGrid != null)
        {
            var fieldUnits = GetAllFieldUnits();

            foreach (var unit in fieldUnits)
            {
                if (unit == null) continue;

                // 노드 참조 제거 (CountOfPiece 자동 감소)
                ClearChessPiece(unit);

                // 풀 반환
                var pooled = unit.GetComponentInParent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    unit.gameObject.SetActive(false);
            }
        }
    }
}
