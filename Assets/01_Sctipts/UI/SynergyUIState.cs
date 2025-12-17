public class SynergyUIState
{
    public TraitType trait;
    public int count;                 // 중복 제거된 개수
    public SynergyThreshold active;   // 없으면 null (1개일 때)
}
