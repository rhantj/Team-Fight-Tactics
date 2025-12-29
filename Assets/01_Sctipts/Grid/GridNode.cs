using System;
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;

    public event Action<GridNode, ChessStateBase, ChessStateBase> OnChessPieceChanged;

    private GridDivideBase Owner;
    private ChessStateBase chessPiece;
    public ChessStateBase ChessPiece
    {
        get => chessPiece;
        set
        {
            if (ReferenceEquals(chessPiece, value)) return;

            var beforePiece = chessPiece;
            bool before = beforePiece != null;
            bool after = value;

            chessPiece = value;

            if(before && !after)
            {
                Owner.DecreasePieceCount();
            }
            else if(!before && after)
            {
                Owner.IncreasePieceCount();
            }

            OnChessPieceChanged?.Invoke(this, beforePiece, chessPiece);
        }
    }

    public int NodeNumber;
    public int X;
    public int Y;

    public GridNode(GridDivideBase owner, Vector3 worldPosition, int x, int y, int nodeNum)
    {
        Owner = owner;
        this.worldPosition = worldPosition;
        X = x;
        Y = y;
        NodeNumber = nodeNum;
        chessPiece = null; 
    }
}