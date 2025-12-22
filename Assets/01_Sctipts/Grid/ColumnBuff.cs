
public class Column : IBuffApply
{
    int columnX = 2;

    public void ApplyBuffs(FieldGrid field, float buffMultiplier)
    {
        var chess = field.GetColumnUnits(columnX);
        if (chess.Count <= 0) return;

        foreach (var piece in chess)
        {
            piece.GlobalBuffApply(buffMultiplier);
        }
    }
}