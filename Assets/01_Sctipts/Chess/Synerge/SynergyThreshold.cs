using UnityEngine;

[System.Serializable]
public class SynergyThreshold
{
    [Tooltip("몇마리 이상부터 작동하는가")]
    public int requiredCount;

    [Header("보너스")]
    public int bonusAttack;
    public int bonusArmor;
    public float attackSpeedMultiplier = 1f;
    public int bonusHP;
}
