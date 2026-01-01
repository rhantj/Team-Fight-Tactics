using UnityEngine;

public class BenchGrid : GridDivideBase
{
    protected override void OnEnable()
    {
        base.OnEnable();
        StaticRegistry<BenchGrid>.Add(this);
    }

    private void OnDisable()
    {
        StaticRegistry<BenchGrid>.Remove(this);
    }

    // 벤치 필드 위에 기물 세팅
    public void SetChessOnBenchNode(Chess piece)
    {
        if (piece == null) return;
        GridNode pos = FindEmptyNode();
        if (pos == null) return;
        pos.ChessPiece = piece;
        piece.SetPosition(pos.worldPosition);
        piece.SetOnField(false); // 벤치에 있음을 표시
    }

    public void ResetAllNode()
    {
        if (fieldGrid != null)
        {
            foreach (var node in fieldGrid)
            {
                if (node.ChessPiece == null) continue;

                // 노드 참조 제거 (CountOfPiece 자동 감소)
                ClearChessPiece(node.ChessPiece);

                // 풀 반환
                var pooled = node.ChessPiece.GetComponentInParent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    node.ChessPiece.gameObject.SetActive(false);
            }
        }
    }
}