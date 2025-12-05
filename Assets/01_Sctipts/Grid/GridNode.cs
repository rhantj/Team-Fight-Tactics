using System;
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;

    public Chess ChessPiece { get; set; }

    public GridNode(Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
    }
}