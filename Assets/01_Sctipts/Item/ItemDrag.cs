using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDrag : MonoBehaviour
{
    public static ItemDrag Instance;

    [SerializeField] private Image dragIcon;

    private Canvas canvas;
    private ItemSlot originSlot;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        dragIcon.enabled = false;
    }

    public void BeginDrag(ItemSlot startSlot, Sprite icon)
    {
        originSlot = startSlot;
        dragIcon.sprite = icon;
        dragIcon.enabled = true;
    }

    public void Drag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var pos);
        dragIcon.rectTransform.localPosition = pos;
    }

    public void EndDrag(PointerEventData eventData)
    {
        dragIcon.enabled = false;

        GameObject dragingObj = eventData.pointerEnter;

        if(dragingObj != null)
        {
            ItemSlot targetSlot = dragingObj.GetComponent<ItemSlot>();

            if(targetSlot != null && targetSlot != originSlot)
            {
                ItemSlotSwap(originSlot, targetSlot);
                return;
            }
        }
        //드래그 후 드랍 위치 오류 -> 원래 있던 위치로 자동 드랍
    }

    private void ItemSlotSwap(ItemSlot a, ItemSlot b)
    {
        ItemBase temp = a.CurrentItem;

        if(b.IsEmpty)
        {
            a.ClearSlot();
        }
        else
        {
            a.SetItem(b.CurrentItem);
        }

        if(temp == null)
        {
            b.ClearSlot();
        }
        else
        {
            b.SetItem(temp);
        }
    }
}
