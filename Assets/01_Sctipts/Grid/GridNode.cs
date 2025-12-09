using System;
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;

    public ChessStateBase ChessPiece;
    public int NodeNumber;
    public int X;
    public int Y;

    public GridNode(Vector3 worldPosition, int x, int y, int nodeNum)
    {
        this.worldPosition = worldPosition;
        X = x;
        Y = y;
        NodeNumber = nodeNum;
    }
}