using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlotManager : MonoBehaviour
{
    public static ItemSlotManager Instance { get; private set; }
    [SerializeField] private ItemSlot[] slots;
    [SerializeField] private ItemData[] testItems; //¿”Ω√

    private void Awake()
    {
        Instance = this;
    }

    //==============================
    //∫Û ΩΩ∑‘ ∞πºˆ √º≈©
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
    //æ∆¿Ã≈€ √ﬂ∞°(±‚π∞ ∆«∏≈ Ω√ π›»Ø)
    //==============================
    public bool AddItem(ItemData data)
    {
        foreach(var slot in slots)
        {
            if(slot.IsEmpty)
            {
                slot.SetItem(data);
                return true;
            }
        }
        return false;
    }

    public void AddRandomItem()
    {
        ItemData item = testItems[Random.Range(0, testItems.Length)];

        //∫ÛΩΩ∑‘ √£±‚
        foreach (var slot in slots)
        {
            if(slot.IsEmpty)
            {
                slot.SetItem(item);
                return;
            }
        }
    }
}
