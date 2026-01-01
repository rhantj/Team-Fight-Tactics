using UnityEngine;

public class RowBuff : MonoBehaviour, IBuffApply
{
    public void ApplyBuffs(FieldGrid field, int row, float buffMultiplier)
    {
        var chess = field.GetRowUnits(row);
        if(chess.Count <= 0) return;

        foreach (var piece in chess)
        {
            piece.GlobalBuffApply(buffMultiplier);
        }
    }
}