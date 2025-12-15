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
}
