using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemCombineManager : MonoBehaviour
{
    [SerializeField] ItemData[] arr_CombinedItemDatas;
    private Dictionary<ItemData, Dictionary<ItemData, ItemData>> combinedItems = new();

    private void Awake()
    {
        combinedItems.Clear();
        foreach(var combined in arr_CombinedItemDatas)
        {
            AddItemPair(combined.combineA, combined.combineB, combined);
            AddItemPair(combined.combineB, combined.combineA, combined);
        }
    }

    void AddItemPair(ItemData a, ItemData b, ItemData res)
    {
        if(!combinedItems.TryGetValue(a, out var item))
        {
            item = new Dictionary<ItemData, ItemData>();
            combinedItems[a] = item;
        }
        item[b] = res;
    }

    public bool TryCombine(ItemData a, ItemData b, out ItemData res)
    {
        res = null;
        if (!a || !b) return false;
        if (a.itemType == ItemType.Combined || b.itemType == ItemType.Combined) return false;
        return combinedItems.TryGetValue(a, out var item) && item.TryGetValue(b, out res);
    }
}
