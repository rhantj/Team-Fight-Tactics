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

    //필드 시너지 계싼
    public void RecalculateSynergies(IEnumerable<ChessStateBase> fieldUnits)
    {
        // 1) 아직 필드에 없는 경우 안전 처리
        if (fieldUnits == null)
        {
            ClearSynergies();
            return;
        }
        currentCounts.Clear();
        foreach (var unit in fieldUnits)
        {
            if (unit == null) continue;
            var traits = unit.Traits; //ChessStateBase.트레
            if (traits == null) continue;

            foreach (var trait in traits)
            {
                if (!currentCounts.ContainsKey(trait))
                    currentCounts[trait] = 0;

                currentCounts[trait]++;
            }
        }
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

            // 해당 Trait가 필드에 아예 없는 경우
            if (!currentCounts.TryGetValue(trait, out int count))
                continue;

            // 최소 2기 이상일 때만 시너지 활성
            if (count < 2)
                continue;

            // count에 맞는 가장 높은 Threshold 찾기
            SynergyThreshold best = null;

            foreach (var t in config.thresholds)
            {
                if (t == null) continue;

                if (count >= t.requiredCount)
                {
                    // 조건을 만족하는 것 중 "가장 높은 requiredCount"를 선택
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
        // 1) 먼저 기본 상태로 초기화 (공속 배수 1.0)
        foreach (var unit in fieldUnits)
        {
            if (unit == null) continue;

            // 이미 ChessStateBase에는 공속 배수 Setter가 있음 
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
