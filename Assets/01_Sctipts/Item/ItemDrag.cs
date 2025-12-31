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

        SettingsUI.PlaySFX("DragChess", Vector3.zero, 1f, 1f);
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

        if (originSlot == null || originSlot.IsEmpty)
        {
            return;
        }

        GameObject dragingObj = eventData.pointerEnter;

        if(dragingObj != null)
        {
            ItemSlot targetSlot = dragingObj.GetComponentInParent<ItemSlot>();

            if(targetSlot != null && targetSlot != originSlot)
            {
                if(targetSlot.IsEmpty)
                {
                    ItemSlotSwap(originSlot, targetSlot);

                    SettingsUI.PlaySFX("DropChess", Vector3.zero, 1f, 1f);
                    return;
                }
                var a = originSlot.CurrentItem.Data;
                var b = targetSlot.CurrentItem.Data;

                if(combineManager != null && combineManager.TryCombine(a,b,out var combined))
                {
                    targetSlot.SetItem(combined);
                    originSlot.ClearSlot();

                    ItemSlotManager.Instance.SortSlots();

                    SettingsUI.PlaySFX("ItemEquip(Comb)", Vector3.zero, 1f, 1f);
                    return;
                }

                ItemSlotSwap(originSlot, targetSlot);
                return;
            }
        }

        if (TryAttachItemToChess(eventData.position))
        {
            originSlot.ClearSlot();

            ItemSlotManager.Instance.SortSlots();
            SettingsUI.PlaySFX("DropChess", Vector3.zero, 1f, 1f);
            return;
        }
        SettingsUI.PlaySFX("DropChess", Vector3.zero, 1f, 1f);
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

        ItemSlotManager.Instance.SortSlots();
    }

    private bool TryAttachItemToChess(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if(!Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            return false;
        }

        Chess chess = hit.transform.GetComponentInParent<Chess>();
        if (chess == null)
        {
            return false;
        }

        ChessItemUI itemUI = chess.GetComponentInChildren<ChessItemUI>();
        if(itemUI == null)
        {
            return false;
        }

        ItemData itemData = originSlot.CurrentItem.Data;
        return itemUI.AddItem(itemData);
    }
}
