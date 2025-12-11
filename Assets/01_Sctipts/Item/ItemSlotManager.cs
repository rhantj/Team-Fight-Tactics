using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlotManager : MonoBehaviour
{
    [SerializeField] private ItemSlot[] slots;
    [SerializeField] private ItemData[] testItems; //¿”Ω√

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
