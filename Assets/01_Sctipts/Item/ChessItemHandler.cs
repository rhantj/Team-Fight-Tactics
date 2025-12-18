using System.Collections.Generic;
using UnityEngine;

public class ChessItemHandler : MonoBehaviour
{
    public const int MAX_ITEM_COUNT = 3;

    private List<ItemData> equippedItems = new();
    public IReadOnlyList<ItemData> EquippedItems => equippedItems;

    private ChessStateBase chess;

    private void Awake()
    {
        chess = GetComponent<ChessStateBase>();
    }

    public bool CanEquip => equippedItems.Count < MAX_ITEM_COUNT;

    public bool EquipItem(ItemData item)
    {
        if (!CanEquip || item == null || chess == null)
            return false;

        equippedItems.Add(item);
        RecalculateItemStats();
        return true;
    }

    public void ClearItems()
    {
        equippedItems.Clear();
        RecalculateItemStats();
    }

    private void RecalculateItemStats()
    {
        if (chess == null) return;

        int bonusHp = 0;
        int bonusAtk = 0;
        int bonusArmor = 0;

        foreach (var item in equippedItems)
        {
            bonusHp += item.addHp;
            bonusAtk += item.addAttack;
            bonusArmor += item.addDefense;
        }

        chess.SetItemBonusStats(bonusAtk, bonusArmor, bonusHp);
    }
}
