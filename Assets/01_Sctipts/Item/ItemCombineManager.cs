using System.Collections.Generic;
using UnityEngine;

public class ItemCombineManager : MonoBehaviour
{
    // 상위 아이템 캐싱
    [SerializeField] ItemData[] arr_CombinedItemDatas;

    // 양방향 딕셔너리로 조합식 정리
    private Dictionary<ItemData, Dictionary<ItemData, ItemData>> combinedItems = new();

    private void Awake()
    {
        foreach(var combined in arr_CombinedItemDatas)
        {
            AddItemPair(combined.combineA, combined.combineB, combined);
            AddItemPair(combined.combineB, combined.combineA, combined);
        }
    }

    // 조합식 추가
    void AddItemPair(ItemData a, ItemData b, ItemData res)
    {
        if(!combinedItems.TryGetValue(a, out var item))
        {
            item = new Dictionary<ItemData, ItemData>();
            combinedItems[a] = item;
        }
        item[b] = res;
    }

    // 합성 가능한지 판단
    public bool TryCombine(ItemData a, ItemData b, out ItemData res)
    {
        res = null;
        if (!a || !b) return false;
        if (a.itemType == ItemType.Combined || b.itemType == ItemType.Combined) return false;
        return combinedItems.TryGetValue(a, out var item) && item.TryGetValue(b, out res);
    }
}
