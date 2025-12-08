using System;
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;

    public ChessStateBase ChessPiece { get; set; }

    public GridNode(Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
    }
}