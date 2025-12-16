using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessItemHandler : MonoBehaviour
{
    public const int MAX_ITEM_COUNT = 3;

    private List<ItemData> equippedItems = new List<ItemData>();
    public IReadOnlyList<ItemData> EquippedItems => equippedItems;

    public bool CanEquip => equippedItems.Count < MAX_ITEM_COUNT;

    public bool EquipItem(ItemData item)
    {
        if(!CanEquip) return false;

        equippedItems.Add(item);
        ApplyItemStat(item);

        OnItemEquipped?.Invoke(equippedItems);
        return true;
    }

    private void ApplyItemStat(ItemData item)
    {
        ChessStatData chess = GetComponent<ChessStatData>();

        chess.maxHP += item.addHp;
    }

    public System.Action<IReadOnlyList<ItemData>> OnItemEquipped;
}
