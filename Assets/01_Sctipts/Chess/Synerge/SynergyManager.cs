using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }
    [Header("시너지 설정")]
    [SerializeField]
    private SynergyConfig[] synergyConfigs;
    [SerializeField] private SynergyUIController synergyUIController;

    //현재활성화된 효과
    private Dictionary<TraitType, SynergyThreshold> activeSynergies = new Dictionary<TraitType, SynergyThreshold>();

    //현재 특성카운트
    private Dictionary<TraitType, int> currentCounts = new Dictionary<TraitType, int>();

    private void Awake() //싱글톤 , DDOL
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //유닛카운트 ->시너지 갱신 ->효과적용
    public void RecalculateSynergies(IEnumerable<ChessStateBase> fieldUnits)
    {
        if (fieldUnits == null)
        {
            ClearSynergies(); //유닛 없으면 시너지 초기화.
            return;
        }
        Dictionary<TraitType, HashSet<ChessStatData>> uniqueByTrait = new(); //중복 방지.

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
                set.Add(unit.BaseData); //동일 기물 SO는 1번만카운트,.
            }
        }

        currentCounts.Clear();
        foreach (var kv in uniqueByTrait)
            currentCounts[kv.Key] = kv.Value.Count;

        UpdateActiveSynergies(); //카운트기반으로 
        ApplySynergyEffects(fieldUnits); //시너지를 기물스텟 반영
        synergyUIController?.RefreshUI();
    }


    public bool TryGetSynergyEffect(
        TraitType trait,
        out SynergyThreshold effect)
    {
        return activeSynergies.TryGetValue(trait, out effect);
    }
    //currentCounts 기준으로 각 특성의 가장 높은 단계 시너지 1개만 활성화됩니다.
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
                activeSynergies[trait] = best; //최종적으로 활성화되는 시너지 등록
            }
        }
    }
    private void ClearSynergies() //시너지 카운터 데이터 초기화
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

            float attackSpeedMul = 1f; //공속은 배수누적입니다. 그이외엔 합산.
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
            unit.SetSynergyBonusStats(bonusAttack, bonusArmor, bonusHP);

        }
    }
    public List<SynergyUIState> GetSynergyUIStates()
    {
        List<SynergyUIState> result = new();

        foreach (var kv in currentCounts)
        {
            TraitType trait = kv.Key;
            int count = kv.Value;

            if (count <= 0)
                continue;

            activeSynergies.TryGetValue(trait, out var active);

            result.Add(new SynergyUIState
            {
                trait = trait,
                count = count,
                active = active
            });
        }

        return result;
    }

    public void ResetAll()
    {
        // 내부 시너지 데이터 초기화
        activeSynergies.Clear();
        currentCounts.Clear();

        // 모든 기물의 시너지 보너스 제거
        foreach (var unit in FindObjectsOfType<ChessStateBase>())
        {
            unit.ResetSynergyStats();
        }

        // 시너지 UI 전부 제거
        synergyUIController?.ClearAll();
    }



}
