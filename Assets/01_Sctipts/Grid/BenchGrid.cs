using UnityEngine;

public class BenchGrid : GridDivideBase
{
    // 벤치 필드 위에 기물 세팅
    public void SetChessOnBenchNode(Chess piece)
    {
        if (piece == null) return;
        GridNode pos = FindEmptyNode();

        if (pos == null) return;
        pos.ChessPiece = piece;
        piece.SetPosition(pos.worldPosition);
    }
}