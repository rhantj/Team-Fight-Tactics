using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SynergyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image synergyIcon;
    [SerializeField] private TMP_Text synergyNameText;
    [SerializeField] private TMP_Text synergyCountText;

    [Header("Tooltip")]
    [SerializeField] private SynergyTooltipTrigger tooltipTrigger;

    /// <summary>
    /// 시너지 UI 기본 정보 세팅
    /// </summary>
    public void SetUI(Sprite icon, string name, int count)
    {
        if (synergyIcon != null)
            synergyIcon.sprite = icon;

        if (synergyNameText != null)
            synergyNameText.text = name;

        if (synergyCountText != null)
            synergyCountText.text = BuildCountText(count);
    }

    /// <summary>
    /// 툴팁 데이터 주입 (SynergyUIController에서 호출)
    /// </summary>
    public void SetTooltipData(
        Sprite icon,
        string name,
        TraitTooltipData tooltipData
    )
    {
        if (tooltipTrigger == null) return;

        tooltipTrigger.SetData(icon, name, tooltipData);
    }

    /// <summary>
    /// 카운트 표시 규칙
    /// </summary>
    private string BuildCountText(int count)
    {
        if (count <= 1)
            return "1 > 2";

        return "2 > 3 > 4";
    }
}
