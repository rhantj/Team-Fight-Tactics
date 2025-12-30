using System.Collections.Generic;
using UnityEngine;

public class GlobalLineBuffSystem : MonoBehaviour
{
    [SerializeField] FieldGrid field;
    [SerializeField] float defaultMultiplier = 1.2f;

    List<BuffRequest> selectedBuffs = new();
    bool dirty;

    private void Start()
    {
        AddRequest(BuffLine.Row, 0, 1.3f);
        AddRequest(BuffLine.Row, 3, 1.3f);
        ApplyRequestNow();
    }

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

    private void GridPieceChanged(GridDivideBase grid, GridNode node, ChessStateBase prev, ChessStateBase curr)
    {
        if(grid == field && prev is Chess prevChess && curr==null)
        {
            prevChess.ClearAllBuffs();
            prevChess.SetOnField(false);
        }

        dirty = true;
    }

    void CalculateBuff()
    {
        if (!field) return;
        ClearBuff();

        for (int i = 0; i < selectedBuffs.Count; ++i)
        {
            var request = selectedBuffs[i];
            var piecies = request.type == BuffLine.Row ?
                field.GetRowUnits(request.idx) :
                field.GetColumnUnits(request.idx);

            foreach(var chess in piecies)
                chess.GlobalBuffApply(request.multiplier);
        }
    }

    void ClearBuff()
    {
        foreach (var node in field.FieldGrid)
        {
            if (node.ChessPiece is Chess c)
            {
                c.ClearAllBuffs();
            }
        }
    }

    public void AddRequest(BuffLine type, int idx, float multiplier = -1f)
    {
        if (multiplier <= 0f)
            multiplier = defaultMultiplier;
        selectedBuffs.Add(new BuffRequest(type, idx, multiplier));
    }

    public void ClearRequests()
    {
        selectedBuffs.Clear();
        dirty = true;
    }

    public void ApplyRequestNow()
    {
        dirty = true;
    }
}