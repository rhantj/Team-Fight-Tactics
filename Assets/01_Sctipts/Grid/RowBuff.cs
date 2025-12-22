using UnityEngine;

public class RowBuff : MonoBehaviour, IBuffApply
{
    int rowY = 2;

    public void ApplyBuffs(FieldGrid field, float buffMultiplier)
    {
        var chess = field.GetRowUnits(rowY);
        if(chess.Count <= 0) return;

        foreach (var piece in chess)
        {
            piece.GlobalBuffApply(buffMultiplier);
        }
    }
}