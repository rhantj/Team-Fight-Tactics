using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

/*전체 정리
 슬롯에서 아이템을 드래그 할 때 :
 - 드래그 아이콘이 마우스를 따라다니도록
 - 아이콘 드롭 위치에 따라 Swap또는 조합 처리
 - 실제 아이템 이동은 ItemSlot에서 처리
 - 드래그 중인 출발 슬롯을 기억함
*/

public class ItemDrag : MonoBehaviour
{
    public static ItemDrag Instance;

    [SerializeField] private Image dragIcon;
    [SerializeField] private ItemCombineManager combineManager;

    private Canvas canvas;
    private ItemSlot originSlot;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        dragIcon.enabled = false;
    }

    //===================== 드래그 시작 ========================
    // - 출발 슬롯 저장
    public void BeginDrag(ItemSlot startSlot, Sprite icon)
    {
        originSlot = startSlot;
        dragIcon.sprite = icon;
        dragIcon.enabled = true;
    }

    //===================== 드래그 중 ========================
    // - 마우스 위치 캔버스 로컬 좌표로 변환해 아이콘 위치 갱신
    public void Drag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var pos);
        dragIcon.rectTransform.localPosition = pos;
    }

    
    //===================== 드래그 끝 ========================
    // - 드래그 아이콘 숨김
    // - 현재 마우스가 가리키는 UI 가져옴
    // - 해당 오브젝트에 ItemSlot이 있으면 이동할 슬롯으로 판단
    // - 이동할 슬롯이 비면 Swap
    // - 이동할 슬롯이 차있으면 조합 가능 여부 검사
    //     ㄴ 조합 가능 -> 완성템 교체&출발 슬롯 비움
    //     ㄴ 조합 불가 -> 위치 교환
    public void EndDrag(PointerEventData eventData)
    {
        dragIcon.enabled = false;

        GameObject dragingObj = eventData.pointerEnter;
        if (!dragingObj) return;

        ItemSlot targetSlot = dragingObj.GetComponent<ItemSlot>();

        //슬롯이 아니거나, 자기 자신 슬롯에 드랍하면 아무것도 안함
        if (!targetSlot || targetSlot == originSlot) return;

        //빈슬롯에 드랍 시 그냥 Swap
        if (targetSlot.IsEmpty)
        {
            ItemSlotSwap(originSlot, targetSlot);
            return;
        }

        //슬롯 아이템이 있으면 조합 시도
        var a = originSlot.CurrentItem.Data;
        var b = targetSlot.CurrentItem.Data;

        if (combineManager.TryCombine(a, b, out var combined))
        {
            //조합 성공 : 완성템으로 교체, 출발 슬롯은 비움
            targetSlot.SetItem(combined);
            originSlot.ClearSlot();
            return;
        }

        ItemSlotSwap(originSlot, targetSlot);
        //드래그 후 드랍 위치 오류 -> 원래 있던 위치로 자동 드랍
    }

    //===================== 아이템 슬롯 스왑 함수 ========================
    // 임의의 슬롯 a와 b에 있는 아이템 교환
    // - b가 비면 a를 비우고 b에 a를 넣는 이동처럼 보임
    // - temp변수로 a의 아이템을 잠시 보관함. 
    private void ItemSlotSwap(ItemSlot a, ItemSlot b)
    {
        //아이템 임시 저장
        ItemBase temp = a.CurrentItem;

        // b -> a
        if(b.IsEmpty)
        {
            a.ClearSlot();
        }
        else
        {
            a.SetItem(b.CurrentItem);
        }

        // temp -> b
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
