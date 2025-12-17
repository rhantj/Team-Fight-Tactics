using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 시너지 아이콘에 마우스를 올렸을 때
/// 시너지 툴팁 UI를 표시하기 위한 트리거 컴포넌트.
/// 
/// - SynergyUI에서 필요한 데이터를 주입받는다.
/// - 마우스 진입 시 SynergyTooltipUI 표시
/// - 마우스 이탈 시 SynergyTooltipUI 숨김
/// </summary>
public class SynergyTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private Sprite icon;                     // 툴팁에 표시할 시너지 아이콘
    private string synergyName;              // 툴팁에 표시할 시너지 이름
    private TraitTooltipData tooltipData;    // 시너지 상세 설명 데이터

    /// <summary>
    /// 시너지 UI 생성 시 호출되어
    /// 툴팁에 필요한 데이터를 주입받는다.
    /// </summary>
    public void SetData(
        Sprite icon,
        string name,
        TraitTooltipData data
    )
    {
        this.icon = icon;
        this.synergyName = name;
        this.tooltipData = data;
    }

    // 마우스가 시너지 아이콘 영역에 진입했을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipData == null) return;
        if (SynergyTooltipUI.Instance == null) return;

        SynergyTooltipUI.Instance.Show(
            icon,
            synergyName,
            tooltipData
        );
    }

    // 마우스가 시너지 아이콘 영역을 벗어났을 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (SynergyTooltipUI.Instance != null)
            SynergyTooltipUI.Instance.Hide();
    }
}
