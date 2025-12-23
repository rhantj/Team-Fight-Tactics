using UnityEngine;

public class Column : MonoBehaviour, IBuffApply
{
    public void ApplyBuffs(FieldGrid field, int column, float buffMultiplier)
    {
        var chess = field.GetColumnUnits(column);
        if (chess.Count <= 0) return;

        foreach (var piece in chess)
        {
            piece.GlobalBuffApply(buffMultiplier);
        }
    }
}