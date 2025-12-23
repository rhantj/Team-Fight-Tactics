using System.Collections.Generic;
using UnityEngine;

public class GlobalLineBuffSystem : MonoBehaviour
{
    [SerializeField] FieldGrid field;
    [SerializeField] float defaultMultiplier = 1.2f;

    List<BuffRequest> selectedBuffs = new();
    bool dirty;

    public BuffLine CurrentType { get; private set; } = BuffLine.Row;

    private void OnEnable()
    {
        if(field)
            field.OnGridChessPieceChanged += GridPieceChanged;
        dirty = true;
    }

    private void OnDisable()
    {
        if(field)
            field.OnGridChessPieceChanged -= GridPieceChanged;
    }

    private void LateUpdate()
    {
        if (!dirty) return;
        dirty = false;
        CalculateBuff();
    }

    private void GridPieceChanged(GridDivideBase _, GridNode __, ChessStateBase ___, ChessStateBase ____)
    {
        dirty = true;
    }

    void CalculateBuff()
    {
        if (field == null) return;

        foreach(var node in field.FieldGrid)
        {
            if(node.ChessPiece is Chess c)
            {
                c.ClearAllBuffs();
            }
        }

        for (int i = 0; i < selectedBuffs.Count; ++i)
        {
            var request = selectedBuffs[i];
            var piecies = request.type == BuffLine.Row ?
                field.GetRowUnits(request.idx) :
                field.GetColumnUnits(request.idx);

            foreach(var chess in piecies)
            {
                chess.GlobalBuffApply(request.multiplier);
            }
        }
    }

    public void SetTypeToRow() => CurrentType = BuffLine.Row;
    public void SetTypeToColumn() => CurrentType = BuffLine.Column;

    public void AddBuffByNode(GridNode node)
    {
        if (node == null) return;

        int index = (CurrentType == BuffLine.Row) ? node.Y : node.X;
        AddOrToggle(CurrentType, index, defaultMultiplier);
        dirty = true;
    }

    public void AddOrToggle(BuffLine type, int index, float multiplier)
    {
        // 같은 라인 다시 누르면 토글(제거)되게
        for (int i = 0; i < selectedBuffs.Count; i++)
        {
            if (selectedBuffs[i].type == type && selectedBuffs[i].idx == index)
            {
                selectedBuffs.RemoveAt(i);
                return;
            }
        }
        selectedBuffs.Add(new BuffRequest(type, index, multiplier));
    }
}