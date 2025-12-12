using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour,IBeginDragHandler, IDragHandler,IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image itemIcon;
   
    public ItemBase CurrentItem { get; private set; }
    public bool IsEmpty => CurrentItem == null;

    private bool isHover;
    private bool isDragging;
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

    //========================= 마우스 오버 ==================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(IsEmpty || isDragging)
        {
            return;
        }
        isHover = true;
        ItemInfoUIManager.Instance.Show(CurrentItem.Data);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;

        ItemInfoUIManager.Instance.Hide();
        ItemRecipeUIManager.Instance.Hide();
    }

    //========================= 우클릭 ==================================

    public void OnPointerDown(PointerEventData eventData)
    {
        if(IsEmpty || isDragging)
        {
            return;
        }

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            ItemInfoUIManager.Instance.Hide();
            ItemRecipeUIManager.Instance.Show(CurrentItem.Data);
        }
    }

    //========================= 아이콘 드래그 ==================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(IsEmpty)
        {
            return;
        }

        isDragging = true;

        ItemInfoUIManager.Instance.Hide();
        ItemRecipeUIManager.Instance.Hide();

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

        isDragging = false;
        ItemDrag.Instance.EndDrag(eventData);

        if(isHover)
        {
            ItemInfoUIManager.Instance.Show(CurrentItem.Data);
        }
    }
}
