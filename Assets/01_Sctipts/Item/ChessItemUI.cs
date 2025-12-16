using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessItemUI : MonoBehaviour
{
    [SerializeField] private Image[] itemSlots;
    private ItemCombineManager combineManager;

    private List<ItemData> equippedItems = new();

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
        for(int i =0; i<equippedItems.Count; i++)
        {
            ItemData exist = equippedItems[i];

            if(combineManager.TryCombine(exist, newItem, out ItemData combined))
            {
                equippedItems.RemoveAt(i);

                equippedItems.Add(combined);

                RefreshUI();
                return true;
            }
        }

        if(equippedItems.Count >= itemSlots.Length)
        {
            return false;
        }

        equippedItems.Add(newItem);
        RefreshUI();
        return true;
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
