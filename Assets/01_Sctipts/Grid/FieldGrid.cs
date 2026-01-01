using System.Collections.Generic;

public class FieldGrid : GridDivideBase
{
    public List<ChessStateBase> allFieldUnits = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        StaticRegistry<FieldGrid>.Add(this);
    }

    private void OnDisable()
    {
        StaticRegistry<FieldGrid>.Remove(this);
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
