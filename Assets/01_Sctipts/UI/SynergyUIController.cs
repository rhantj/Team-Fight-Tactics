using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시너지 UI 전체를 관리하는 컨트롤러
/// - SynergyManager의 계산 결과를 받아
/// - 시너지 UI 프리팹 생성 / 갱신 / 제거를 담당
/// </summary>
public class SynergyUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform synergyUIParent;
    [SerializeField] private SynergyUI synergyUIPrefab;

    [Header("Databases")]
    [SerializeField] private TraitSynergyIconDatabase synergyIconDB;
    [SerializeField] private TraitIconDatabase traitIconDB; // 표시 이름용

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

        // 이번 프레임에 실제로 사용된 Trait 기록
        HashSet<TraitType> usedTraits = new();

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

            // 아이콘 선택 (Trait + count 기준)
            Sprite icon = synergyIconDB != null
                ? synergyIconDB.GetIcon(state.trait, state.count)
                : null;

            // 이름 선택
            string displayName = traitIconDB != null
                ? traitIconDB.GetDisplayName(state.trait)
                : state.trait.ToString();

            // UI 갱신
            ui.SetUI(
                icon,
                displayName,
                state.count
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
}
