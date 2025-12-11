using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChessInfoUI : Singleton<ChessInfoUI>
{
    [Header("Root Panel")]
    [SerializeField] private GameObject panel;

    [Header("Basic Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;

    [Header("Stats")]
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;

    [Header("Skill Icon")]
    [SerializeField] private Image skillIconImage;

    protected override void Awake()
    {
        base.Awake();     // GenericSingleton 내부 Awake() 실행
        panel.SetActive(false);
    }

    public void ShowInfo(ChessStatData data)
    {
        if (data == null)
        {
            Hide();
            return;
        }

        // 기본 정보
        iconImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // 스탯
        armorText.text = data.armor.ToString();
        attackDamageText.text = data.attackDamage.ToString();
        attackSpeedText.text = data.attackSpeed.ToString("0.00");

        // 스킬 아이콘
        skillIconImage.sprite = data.skillIcon;

        panel.SetActive(true);
    }

    private void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out _))
            {
                Hide();
            }
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
