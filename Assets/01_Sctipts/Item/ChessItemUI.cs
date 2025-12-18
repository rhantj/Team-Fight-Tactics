using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessItemUI : MonoBehaviour
{
    [SerializeField] private Image[] itemSlots;
    private ItemCombineManager combineManager;

    private List<ItemData> equippedItems = new();

    public int EquippedItemCount => equippedItems.Count;   //외부 접근용

    private void Awake()
    {
        combineManager = FindObjectOfType<ItemCombineManager>();

        if (combineManager == null)
        {
            Debug.LogError(
                "[ChessItemUI] ItemCombineManager not found in scene. " +
                "씬에 ItemCombineManager가 존재해야 합니다."
            );
        }
    }

    public bool AddItem(ItemData newItem)
    {
        if (newItem == null) return false;

        var handler = GetComponentInParent<ChessItemHandler>();
        if (handler == null)
        {
            Debug.LogError("[ChessItemUI] ChessItemHandler not found");
            return false;
        }

        // 1. 조합 체크
        for (int i = 0; i < equippedItems.Count; i++)
        {
            ItemData exist = equippedItems[i];

            if (combineManager.TryCombine(exist, newItem, out ItemData combined))
            {
                equippedItems.RemoveAt(i);
                equippedItems.Add(combined);

                // Handler는 UI 기준 상태로 재계산
                handler.ClearItems();
                foreach (var item in equippedItems)
                {
                    handler.EquipItem(item);
                }

                RefreshUI();
                ChessInfoUI.Instance?.RefreshItemUIOnly();
                return true;
            }
        }

        // 2. 슬롯 초과
        if (equippedItems.Count >= itemSlots.Length)
            return false;

        // 3. 일반 장착
        equippedItems.Add(newItem);

        handler.ClearItems();
        foreach (var item in equippedItems)
        {
            handler.EquipItem(item);
        }

        RefreshUI();
        ChessInfoUI.Instance?.RefreshItemUIOnly();
        return true;
    }


    public List<ItemData> PopAllItems()
    {
        List<ItemData> items = new List<ItemData>(equippedItems);
        equippedItems.Clear();
        RefreshUI();
        return items;
    }

    private void RefreshUI()
    {
        for(int i = 0; i< itemSlots.Length; i++)
        {
            itemSlots[i].sprite = null;
            itemSlots[i].color = Color.clear;
            itemSlots[i].gameObject.SetActive(false);
        }

        for(int i= 0; i<equippedItems.Count; i++)
        {
            itemSlots[i].sprite = equippedItems[i].icon;
            itemSlots[i].color = Color.white;
            itemSlots[i].gameObject.SetActive(true);
        }
    }

    public void ClearAll()
    {
        equippedItems.Clear();
        RefreshUI();
    }
}
