using System;
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;

    private GridDivideBase owner;
    private ChessStateBase chessPiece;
    public ChessStateBase ChessPiece
    {
        get => chessPiece;
        set
        {
            if (chessPiece == value) return;
            bool before = chessPiece;
            bool after = value;

            chessPiece = value;

            if(before && !after)
            {
                owner.DecreasePieceCount();
            }
            else if(!before && after)
            {
                owner.IncreasePieceCount();
            }
        }
    }

    public int NodeNumber;
    public int X;
    public int Y;

    public GridNode(GridDivideBase owner, Vector3 worldPosition, int x, int y, int nodeNum)
    {
        this.owner = owner;
        this.worldPosition = worldPosition;
        X = x;
        Y = y;
        NodeNumber = nodeNum;
        chessPiece = null; 
    }
}