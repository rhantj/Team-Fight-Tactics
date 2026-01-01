using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시너지 UI 전체를 관리하는 컨트롤러
/// - SynergyManager의 계산 결과를 받아
/// - 시너지 UI 프리팹 생성 / 갱신 / 제거를 담당
/// - 시너지 툴팁 데이터 주입 책임을 가짐
/// </summary>
public class SynergyUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform synergyUIParent;
    [SerializeField] private SynergyUI synergyUIPrefab;

    [Header("Databases")]
    [SerializeField] private TraitSynergyIconDatabase synergyIconDB;
    [SerializeField] private TraitIconDataBase traitIconDB; // 이름 표시용

    [Header("Tooltip Data")]
    [SerializeField] private TraitTooltipData[] tooltipDatas;

    // TraitType 당 하나의 UI만 유지
    private Dictionary<TraitType, SynergyUI> uiMap
        = new Dictionary<TraitType, SynergyUI>();

    /// <summary>
    /// 시너지 UI 전체 갱신
    /// </summary>
    public void RefreshUI()
    {
        if (SynergyManager.Instance == null)
            return;

        var states = SynergyManager.Instance.GetSynergyUIStates();

        // 시너지 정렬 (금 > 은 > 동 > 비활성)
        states.Sort((a, b) =>
        {
            // 1. 활성 여부 (활성이 위)
            bool aActive = a.active != null;
            bool bActive = b.active != null;

            if (aActive != bActive)
                return bActive.CompareTo(aActive);

            // 2. 둘 다 활성일 경우: requiredCount 큰 쪽이 위 (금 > 은 > 동)
            if (aActive && bActive)
            {
                int tierCompare = b.active.requiredCount.CompareTo(a.active.requiredCount);
                if (tierCompare != 0)
                    return tierCompare;
            }

            // 3. 같은 단계면 count 큰 쪽이 위
            return b.count.CompareTo(a.count);
        });
        // 이번 프레임에 실제로 사용된 Trait 기록
        HashSet<TraitType> usedTraits = new();
        int siblingIndex = 0;

        foreach (var state in states)
        {
            if (state.count <= 0)
                continue;

            usedTraits.Add(state.trait);

            // UI 없으면 생성
            if (!uiMap.TryGetValue(state.trait, out var ui))
            {
                ui = Instantiate(synergyUIPrefab, synergyUIParent);
                uiMap.Add(state.trait, ui);
            }

            // 정렬된 순서대로 UI 위치 재배치
            ui.transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;


            // 아이콘 선택 (Trait + count 기준)
            Sprite icon = synergyIconDB != null
                ? synergyIconDB.GetIcon(state.trait, state.count)
                : null;

            // 이름 선택
            string displayName = traitIconDB != null
                ? traitIconDB.GetDisplayName(state.trait)
                : state.trait.ToString();

            // 기본 UI 갱신
            ui.SetUI(
                icon,
                displayName,
                state.count
            );

            // ─────────────────────────────
            // 툴팁 데이터 주입 (핵심)
            // ─────────────────────────────
            TraitTooltipData tooltipData = GetTooltipData(state.trait);

            ui.SetTooltipData(
                icon,          // 시너지 UI에서 쓰는 동일 아이콘 (회색/동/은/금 중 현재 상태)
                displayName,   // 시너지 UI에 표시되는 이름 그대로
                tooltipData
            );
        }

        // 더 이상 존재하지 않는 Trait UI 제거
        List<TraitType> toRemove = new();
        foreach (var kv in uiMap)
        {
            if (!usedTraits.Contains(kv.Key))
                toRemove.Add(kv.Key);
        }

        foreach (var trait in toRemove)
        {
            if (uiMap.TryGetValue(trait, out var ui))
            {
                Destroy(ui.gameObject);
            }
            uiMap.Remove(trait);
        }
    }

    /// <summary>
    /// 모든 시너지 UI 제거
    /// </summary>
    public void ClearAll()
    {
        foreach (var kv in uiMap)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }
        uiMap.Clear();
    }

    /// <summary>
    /// TraitType에 해당하는 TooltipData 검색
    /// </summary>
    private TraitTooltipData GetTooltipData(TraitType trait)
    {
        if (tooltipDatas == null) return null;

        foreach (var data in tooltipDatas)
        {
            if (data != null && data.trait == trait)
                return data;
        }
        return null;
    }
}
