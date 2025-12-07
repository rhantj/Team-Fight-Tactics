using UnityEngine;
using UnityEngine.EventSystems;

// 상점UI를 판매영역으로 설정해 기물을 드래그 드롭하면 판매되도록 하는 초안 스크립트
public class SellArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsPointerOverSellArea = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverSellArea = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverSellArea = false;
    }
}
