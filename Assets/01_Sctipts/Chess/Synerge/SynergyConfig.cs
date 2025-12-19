using UnityEngine;

[System.Serializable]
public class SynergyConfig
{
    [Tooltip("시너지 타입")]
    public TraitType trait;

    [Tooltip("시너지 설정")]
    public SynergyThreshold[] thresholds;
}
