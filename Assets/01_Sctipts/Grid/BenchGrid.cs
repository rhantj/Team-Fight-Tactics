using UnityEngine;

public class BenchGrid : GridDivideBase
{
    public void SetChessOnBenchNode(Chess piece)
    {
        if (piece == null) return;
        GridNode pos = FindEmptyNode();

        if (pos == null) return;
        pos.ChessPiece = piece;
        piece.SetPosition(pos.worldPosition);
    }
}