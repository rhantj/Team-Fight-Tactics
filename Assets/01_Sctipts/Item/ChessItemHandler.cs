using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기물에 장착된 아이템을 관리하는 클래스.
/// 
/// - 아이템 장착/해제 관리
/// - 장착된 아이템들의 스탯을 합산
/// - 합산된 아이템 스탯을 ChessStateBase에 전달
/// 
/// 이 클래스는 실제 스탯 계산을 직접 하지 않고,
/// 아이템 수치만 취합하여 ChessStateBase에 반영하는 역할만 담당한다.
/// </summary>
public class ChessItemHandler : MonoBehaviour
{
    // 기물 1개당 장착 가능한 최대 아이템 개수
    public const int MAX_ITEM_COUNT = 3;

    // 현재 장착된 아이템 목록
    private List<ItemData> equippedItems = new();

    // 외부에서 읽기 전용으로 접근하기 위한 프로퍼티
    public IReadOnlyList<ItemData> EquippedItems => equippedItems;

    // 아이템 스탯을 적용할 대상 기물
    private ChessStateBase chess;

    private List<ItemBase> runtimeItems = new List<ItemBase>();

    private ItemCombineManager combineManager;

    private void Awake()
    {
        // 같은 오브젝트에 있는 ChessStateBase 참조
        chess = GetComponent<ChessStateBase>();

        combineManager = FindObjectOfType<ItemCombineManager>();
    }
    //합성용 : 현재 아이템 전부 추출
    public List<ItemData> PopAllItemDatas()
    {
        var result = new List<ItemData>(equippedItems);
        ClearItems(); // OnUnequip + 리스트 초기화 + 스탯 재계산
        return result;
    }

    //합성용 : 데이터 리스트로 재 장착
    public bool SetItemFromDatas(List<ItemData> items, out List<ItemData> overflow)
    { 
        overflow = new List<ItemData>();
        if (items == null) return true;

        ClearItems(); 

        for(int i = 0; i<items.Count; i++)
        {
            var item = items[i];
            if (item == null) continue;

            if(!TryAddItemWithCombine(item))
            {
                overflow.Add(item);
            }
        }

        return overflow.Count == 0;
    }

    //추가 + 자동으로 조합 메서드
    private bool TryAddItemWithCombine(ItemData newItem)
    {
        if (newItem == null) return false;

        //기존 아이템과 조합 시도
        if(combineManager != null)
        {
            for(int i =0; i<equippedItems.Count; i++)
            {
                ItemData exist = equippedItems[i];

                if(combineManager.TryCombine(exist, newItem, out ItemData combined))
                {
                    RemoveRuntimeItemAt(i);
                    equippedItems.RemoveAt(i);

                    return AddRuntimeItem(combined);
                }
            }
        }

        if(equippedItems.Count >= MAX_ITEM_COUNT) return false;

        return AddRuntimeItem(newItem);
    }

    // 아이템을 더 장착할 수 있는지 여부
    public bool CanEquip => equippedItems.Count < MAX_ITEM_COUNT;

    // 아이템 장착 처리
    public bool EquipItem(ItemData itemData)
    {
        if (itemData == null) return false;
        if (equippedItems.Count >= MAX_ITEM_COUNT) return false;
        return AddRuntimeItem(itemData);
    }

    private bool AddRuntimeItem(ItemData itemData)
    {
        if(itemData == null || chess == null) return false;

        ItemBase itemInstance = ItemFactory.Create(itemData);
        itemInstance.OnEquip(chess);

        runtimeItems.Add(itemInstance);
        equippedItems.Add(itemData);

        RecalculateItemStats();
        return true;
    }

    private void RemoveRuntimeItemAt(int index)
    {
        if (index < 0 || index >= runtimeItems.Count) return;

        var inst = runtimeItems[index];
        if(inst != null)
        {
            inst.OnUnequip();
        }

        runtimeItems.RemoveAt(index);
    }

    // 모든 아이템 제거
    public void ClearItems()
    {
        foreach(var item in runtimeItems)
        {
            item.OnUnequip();
        }

        // 장착 목록 초기화
        runtimeItems.Clear();
        equippedItems.Clear();

        // 아이템 제거 후 스탯 재계산
        RecalculateItemStats();
    }

    // 장착된 아이템들의 스탯을 다시 계산
    private void RecalculateItemStats()
    {
        if (chess == null) return;

        int bonusHp = 0;
        int bonusAtk = 0;
        int bonusArmor = 0;
        float attackSpeedMultiplier = 1f;

        foreach (var item in equippedItems)
        {
            bonusHp += item.addHp;
            bonusAtk += item.addAttack;
            bonusArmor += item.addDefense;


            // 공속 누적 (퍼센트 → 배수)
            if (item.addAttackSpeed != 0)
            {
                attackSpeedMultiplier *= 1f + item.addAttackSpeed * 0.01f;
            }
        }

        // 공속까지 함께 전달
        chess.SetItemBonusStats(
            bonusAtk,
            bonusArmor,
            bonusHp,
            attackSpeedMultiplier
        );

    }



}
