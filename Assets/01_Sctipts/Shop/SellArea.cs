using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 상점 UI 내에서 기물 판매 영역을 감지하기 위한 컴포넌트
/// 드래그 중인 기물이 현재 판매 영역 위에 있는지를 판단하기 위해
/// 마우스 포인터의 진입/이탈 상태를 전역 플래그로 제공한다.
/// 실제 판매 로직은 다른 스크립트(예 : DragEvents)에서 처리한다.
/// </summary>
public class SellArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 현재 마우스 포인터가 판매 영역 위에 있는지를 나타내는 전역 상태 값
    /// 드래그 종료 시 판매 여부를 판단하는 용도로 사용된다.
    /// </summary>
    public static bool IsPointerOverSellArea = false;

    //판매 가능 상태 On
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverSellArea = true;
    }

    //판매 가능 상태 Off
    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverSellArea = false;
    }
}

// DragEvents 등 다른 입력 처리 스크립트에서
// 참조 연결 없이 즉시 접근할 수 있도록 static 상태로 유지했습니다.
// 판매로직은 ShopManager에서 만든 메서드를 DragEvents에서 드래그 종료 시점에 호출하는 식으로 연동
