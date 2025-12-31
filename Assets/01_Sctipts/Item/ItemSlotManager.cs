using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlotManager : MonoBehaviour
{
    public static ItemSlotManager Instance { get; private set; }
    [SerializeField] private ItemSlot[] slots;

    private void Awake()
    {
        Instance = this;
    }

    //==============================
    //빈 슬롯 갯수 체크
    //==============================
    public int EmptySlotCount
    {
        get
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                    count++;
            }
            return count;
        }
    }
    //==============================
    //아이템 추가(기물 판매 시 반환)
    //==============================
    public bool AddItem(ItemData data)
    {
        foreach(var slot in slots)
        {
            if(slot.IsEmpty)
            {
                slot.SetItem(data);
                SettingsUI.PlaySFX("DragChess", Vector3.zero, 1f, 1f);
                return true;
            }
        }
        return false;
    }

    public void SortSlots()
    {
        List<ItemBase> items = new();
        foreach(var slot in slots)
        {
            if (!slot.IsEmpty)
                items.Add(slot.CurrentItem);
        }

        foreach (var slot in slots) slot.ClearSlot();

        for (int i = 0; i < items.Count; ++i)
        {
            slots[i].SetItem(items[i]);
        }
    }

    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }
}
