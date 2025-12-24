using System.Collections.Generic;
using UnityEditor.Search;

public class FieldGrid : GridDivideBase, IPrepare
{
    public List<ChessStateBase> allFieldUnits = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.OnRoundEnded += PrepareChessPieces;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoundEnded -= PrepareChessPieces;
    }

    public void PrepareChessPieces(int arg1, bool arg2)
    {
        allFieldUnits.Clear();
        allFieldUnits = GetAllFieldUnits();

        foreach (var node in FieldGrid)
        {
            var piece = node.ChessPiece;
            if (piece)
            {
                piece.InitOnPrepare();
                piece.SetPosition(node.worldPosition);
                piece.gameObject.SetActive(true);
            }
        }
    }

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
                //var pooled = unit.GetComponentInParent<PooledObject>();
                //if (pooled != null)
                //    pooled.ReturnToPool();
                //else
                unit.gameObject.SetActive(false);
            }
        }
    }

    // 행에 있는 기물들 반환
    public List<ChessStateBase> GetRowUnits(int y)
    {
        var tmp  = new List<ChessStateBase>();

        for (int x = 0; x < gridXCnt; ++x)
        {
            var node = fieldGrid[y, x];
            if (node.ChessPiece != null)
                tmp.Add(node.ChessPiece);
        }

        return tmp;
    }

    // 열에 있는 기물들 반환
    public List<ChessStateBase> GetColumnUnits(int x)
    {
        var tmp = new List<ChessStateBase>();

        for (int y = 0; y < gridYCnt; ++y)
        {
            var node = fieldGrid[y, x];
            if (node.ChessPiece != null)
                tmp.Add(node.ChessPiece);
        }

        return tmp;
    }
}
