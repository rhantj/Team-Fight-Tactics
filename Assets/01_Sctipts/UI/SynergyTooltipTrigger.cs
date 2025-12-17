using UnityEngine;
using UnityEngine.EventSystems;

public class SynergyTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private Sprite icon;
    private string synergyName;
    private TraitTooltipData tooltipData;

    /// <summary>
    /// SynergyUI에서 데이터 주입
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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SynergyTooltipUI.Instance != null)
            SynergyTooltipUI.Instance.Hide();
    }


}
