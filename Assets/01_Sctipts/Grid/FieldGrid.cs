using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGrid : GridDivideBase
{
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
