using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/* 전체정리
 * 아이템 슬롯 1칸에서 수행하는 역할 담당.
 * 1. 슬롯 아이템 설정 및 제거
 * 2. 마우스 오버 시 ItemInfoUI
 * 3. 마우스 우클릭 ItemRecipeUI
 * 4. 드래그 & 드랍 -> ItemDrag로 전달함
*/
public class ItemSlot : MonoBehaviour,IBeginDragHandler, IDragHandler,IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image itemIcon;
    Color firstColor;
   
    //슬롯이 들고있는 아이템 인스턴스
    public ItemBase CurrentItem { get; private set; }

    //슬롯이 비었는지 여부 판단.
    public bool IsEmpty => CurrentItem == null;

    private bool isHover;
    private bool isDragging;

    private void Awake()
    {
        firstColor = itemIcon.color;
    }

    public void SetItem(ItemData data) //랜덤 아이템 넣기(임시임)
    {
        CurrentItem = new ItemBase(data);
        itemIcon.sprite = data.icon;
        itemIcon.color = Color.white;
    }

    public void SetItem(ItemBase item) //이건 슬롯 간 교환
    {
        CurrentItem = item;
        itemIcon.sprite = item.Data.icon;
        itemIcon.color = Color.white;
    }

    public void ClearSlot() //슬롯 비우기
    {
        CurrentItem = null;
        itemIcon.sprite = null;
        itemIcon.color = firstColor;
    }

    //========================= 마우스 오버 ==================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        //슬롯 빔 또는 드래그 중이면 UI 안띄움
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
