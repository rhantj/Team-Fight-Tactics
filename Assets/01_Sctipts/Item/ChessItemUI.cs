using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기물의 아이템 UI를 담당하는 클래스.
/// 
/// - 아이템 슬롯 UI 표시 관리
/// - 아이템 추가 시 조합 여부 판단
/// - 조합 또는 장착 결과를 ChessItemHandler와 동기화
/// - 기물 정보 UI(ChessInfoUI) 갱신 트리거 역할
/// 
/// 실제 스탯 계산은 하지 않으며,
/// 아이템의 시각적 상태와 장착 흐름만 관리한다.
/// </summary>
public class ChessItemUI : MonoBehaviour
{
    // 아이템 슬롯 UI 이미지 배열
    [SerializeField] private Image[] itemSlots;

    // 아이템 조합 로직을 담당하는 매니저
    private ItemCombineManager combineManager;

    // 현재 UI 기준으로 장착된 아이템 목록
    private List<ItemData> equippedItems = new();

    // 현재 장착된 아이템 개수 (외부 접근용)
    public int EquippedItemCount => equippedItems.Count;   //외부 접근용

    private void Awake()
    {
        // 씬 내에서 ItemCombineManager 탐색
        combineManager = FindObjectOfType<ItemCombineManager>();
    }

    /// <summary>
    /// 아이템을 기물에 장착한다.
    /// 
    /// - 기존 아이템과 조합 가능한지 우선 검사
    /// - 조합 성공 시 기존 아이템 제거 후 합성 아이템으로 교체
    /// - 조합 실패 시 빈 슬롯이 있으면 일반 장착
    /// - 처리 결과를 ChessItemHandler와 UI에 동기화
    /// </summary>
    public bool AddItem(ItemData newItem)
    {
        // 유효하지 않은 아이템 방어 처리
        if (newItem == null) return false;

        // 상위 오브젝트에서 ChessItemHandler 탐색
        var handler = GetComponentInParent<ChessItemHandler>();
        if (handler == null)
        {
            return false;
        }

        // 1. 조합 체크
        for (int i = 0; i < equippedItems.Count; i++)
        {
            ItemData exist = equippedItems[i];

            // 기존 아이템과 신규 아이템의 조합 가능 여부 확인
            if (combineManager.TryCombine(exist, newItem, out ItemData combined))
            {
                // 기존 아이템 제거 후 조합 결과 아이템 추가
                equippedItems.RemoveAt(i);
                equippedItems.Add(combined);

                // Handler는 UI 기준 상태로 재계산
                handler.ClearItems();
                foreach (var item in equippedItems)
                {
                    handler.EquipItem(item);
                }

                // UI 갱신
                RefreshUI();
                ChessInfoUI.Instance?.RefreshItemUIOnly();
                SettingsUI.PlaySFX("ItemEquip(Comb)", transform.position, 1f, 1f);
                return true;
            }
        }

        // 2. 슬롯 초과
        if (equippedItems.Count >= itemSlots.Length)
            return false;

        // 3. 일반 장착
        equippedItems.Add(newItem);
        SettingsUI.PlaySFX("ItemEquip(Mate)", transform.position, 1f, 1f);
        // Handler에 현재 UI 기준 아이템 목록 재적용
        handler.ClearItems();
        foreach (var item in equippedItems)
        {
            handler.EquipItem(item);
        }

        // UI 갱신
        RefreshUI();
        ChessInfoUI.Instance?.RefreshItemUIOnly();
        return true;
    }

    /// <summary>
    /// 현재 장착된 모든 아이템을 반환하고 UI에서는 제거한다.
    /// 주로 기물 판매, 조합 재료 처리 시 사용된다.
    /// </summary>
    public List<ItemData> PopAllItems()
    {
        List<ItemData> items = new List<ItemData>(equippedItems);
        equippedItems.Clear();
        RefreshUI();
        return items;
    }

    // 아이템 슬롯 UI를 현재 장착 상태에 맞게 갱신
    private void RefreshUI()
    {
        // 모든 슬롯 초기화
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].sprite = null;
            itemSlots[i].color = Color.clear;
            itemSlots[i].gameObject.SetActive(false);
        }

        // 장착된 아이템 수만큼 슬롯 활성화
        for (int i = 0; i < equippedItems.Count; i++)
        {
            itemSlots[i].sprite = equippedItems[i].icon;
            itemSlots[i].color = Color.white;
            itemSlots[i].gameObject.SetActive(true);
        }
    }

    // 모든 아이템을 제거하고 UI를 초기화
    public void ClearAll()
    {
        equippedItems.Clear();
        RefreshUI();
    }

    public void SyncFromHandler()
    {
        var handler = GetComponentInParent<ChessItemHandler>();

        if (handler == null) return;

        equippedItems.Clear();
        equippedItems.AddRange(handler.EquippedItems);

        RefreshUI();
    }
}
