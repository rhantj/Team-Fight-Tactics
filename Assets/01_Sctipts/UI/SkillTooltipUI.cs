using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스킬 툴팁 UI를 실제로 표시하는 컨트롤러.
///
/// - SkillTooltipTrigger로부터 데이터를 전달받아
///   아이콘 / 스킬명 / 설명을 UI에 출력
/// - 표시 / 숨김 기능만 담당
/// - 싱글톤으로 관리되어 어디서든 접근 가능
///
/// 이 클래스는 "어떤 데이터를 보여줄지"에 대한 판단은 하지 않으며,
/// 전달받은 정보를 그대로 화면에 출력하는 역할만 수행한다.
/// </summary>
public class SkillTooltipUI : Singleton<SkillTooltipUI>
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    protected override void Awake()
    {
        base.Awake();

        // 싱글톤 중복 파괴 후 살아남은 인스턴스만 실행
        if (Instance != this) return;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 스킬 툴팁을 화면에 표시한다.
    /// 전달받은 데이터를 그대로 UI에 반영한다.
    /// </summary>
    public void Show(Sprite icon, string skillName, string description)
    {
        iconImage.sprite = icon;
        nameText.text = skillName;
        descText.text = description;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 스킬 툴팁을 숨긴다.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
