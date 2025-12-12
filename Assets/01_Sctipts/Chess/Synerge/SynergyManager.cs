using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }
    [Header("시너지 설정")]
    [SerializeField]
    private SynergyConfig[] synergyConfigs;

    //현재활성화된
    private Dictionary<TraitType, SynergyThreshold> activeSynergies = new Dictionary<TraitType, SynergyThreshold>();

    private Dictionary<TraitType, int> currentCounts = new Dictionary<TraitType, int>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RecalculateSynergies(IEnumerable<ChessStateBase> fieldUnits)
    {
        if (fieldUnits == null)
        {
            ClearSynergies();
            return;
        }

        //Trait별 "서로 다른 기물" 집합
        Dictionary<TraitType, HashSet<ChessStatData>> uniqueByTrait = new();

        foreach (var unit in fieldUnits)
        {
            if (unit == null) continue;
            if (unit.BaseData == null) continue;

            var traits = unit.Traits;
            if (traits == null) continue;

            foreach (var trait in traits)
            {
                if (!uniqueByTrait.TryGetValue(trait, out var set))
                {
                    set = new HashSet<ChessStatData>();
                    uniqueByTrait.Add(trait, set);
                }
                set.Add(unit.BaseData);
            }
        }

        currentCounts.Clear();
        foreach (var kv in uniqueByTrait)
            currentCounts[kv.Key] = kv.Value.Count;

        UpdateActiveSynergies();
        ApplySynergyEffects(fieldUnits);
    }


    public bool TryGetSynergyEffect(
        TraitType trait,
        out SynergyThreshold effect)
    {
        return activeSynergies.TryGetValue(trait, out effect);
    }

    private void UpdateActiveSynergies()
    {
        activeSynergies.Clear();

        if (synergyConfigs == null) return;

        foreach (var config in synergyConfigs)
        {
            if (config == null || config.thresholds == null) continue;

            TraitType trait = config.trait;

            if (!currentCounts.TryGetValue(trait, out int count))
                continue;

            if (count < 2)
                continue;

            SynergyThreshold best = null;

            foreach (var t in config.thresholds)
            {
                if (t == null) continue;

                if (count >= t.requiredCount)
                {
                    if (best == null || t.requiredCount > best.requiredCount)
                        best = t;
                }
            }

            if (best != null)
            {
                activeSynergies[trait] = best;
            }
        }
    }
    private void ClearSynergies()
    {
        activeSynergies.Clear();
        currentCounts.Clear();
    }
    private void ApplySynergyEffects(IEnumerable<ChessStateBase> fieldUnits)
    {
        foreach (var unit in fieldUnits)
        {
            if (unit == null) continue;

            unit.SetAttackSpeedMultiplier(1f);
        }

        //특성효과 적용
        foreach (var unit in fieldUnits)
        {
            if (unit == null) continue;
            var traits = unit.Traits;
            if (traits == null) continue;

            float attackSpeedMul = 1f;
            int bonusAttack = 0;
            int bonusArmor = 0;
            int bonusHP = 0;

            foreach (var trait in traits)
            {
                if (activeSynergies.TryGetValue(trait, out var effect))
                {
                    attackSpeedMul *= effect.attackSpeedMultiplier;
                    bonusAttack += effect.bonusAttack;
                    bonusArmor += effect.bonusArmor;
                    bonusHP += effect.bonusHP;
                }
            }

            //공속 배수
            unit.SetAttackSpeedMultiplier(attackSpeedMul);
            unit.AddBonusStats(bonusAttack, bonusArmor, bonusHP);
        }
    }
}
