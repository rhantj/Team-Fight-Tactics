public enum BuffLine
{
    Row,
    Column
}

public struct BuffRequest
{
    public BuffLine type;
    public int idx;
    public float multiplier;

    public BuffRequest(BuffLine type, int idx, float multiplier)
    {
        this.type = type;
        this.idx = idx;
        this.multiplier = multiplier;
    }
}