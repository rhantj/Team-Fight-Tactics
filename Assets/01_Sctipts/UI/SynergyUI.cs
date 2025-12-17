using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 시너지 UI 프리팹 1개의 표시 로직
/// </summary>
public class SynergyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image synergyIcon;
    [SerializeField] private TMP_Text synergyNameText;
    [SerializeField] private TMP_Text synergyCountText;

    /// <summary>
    /// 시너지 UI 갱신
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
    /// 시너지 카운트 표시 규칙
    /// 1  -> "1 > 2"
    /// 2+ -> "2 > 3 > 4"
    /// </summary>
    private string BuildCountText(int count)
    {
        if (count <= 1)
            return "1 > 2";

        return "2 > 3 > 4";
    }
}
