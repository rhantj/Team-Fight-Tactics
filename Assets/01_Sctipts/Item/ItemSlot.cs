using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour,IBeginDragHandler, IDragHandler,IEndDragHandler
{
    [SerializeField] private Image itemIcon;
   
    public ItemBase CurrentItem { get; private set; }
    public bool IsEmpty => CurrentItem == null;

    public void SetItem(ItemData data) //랜덤 아이템 넣기(임시임)
    {
        CurrentItem = new ItemBase(data);
        itemIcon.sprite = data.icon;
        itemIcon.enabled = true;
    }

    public void SetItem(ItemBase item) //이건 슬롯 간 교환
    {
        CurrentItem = item;
        itemIcon.sprite = item.Data.icon;
        itemIcon.enabled = true;
    }

    public void ClearSlot()
    {
        CurrentItem = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(IsEmpty)
        {
            return;
        }

        ItemDrag.Instance.BeginDrag(this, itemIcon.sprite);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(IsEmpty)
        {
            return;
        }
        ItemDrag.Instance.Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsEmpty)
        {
            return;
        }
        ItemDrag.Instance.EndDrag(eventData);
    }
}
