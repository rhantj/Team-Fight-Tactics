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
    [SerializeField] private Image frameImage;

    [Header("Stats")]
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;

    [Header("Skill Icon")]
    [SerializeField] private Image skillIconImage;

    private SkillTooltipTrigger skillTooltipTrigger;

    [Header("Cost UI Data")]
    [SerializeField] private CostUIData costUIData;

    [Header("HP / Mana UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private TMP_Text manaText;

    private ChessStateBase currentChess;

    [Header("Synerge UI")]
    [SerializeField] private Transform synergyContainer;
    [SerializeField] private GameObject synergyIconPrefab;

    [Header("Trait Icon Database")]
    [SerializeField] private TraitIconDatabase traitIconDB;


    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        skillTooltipTrigger = skillIconImage.GetComponent<SkillTooltipTrigger>();

        panel.SetActive(false);
    }

    public void ShowInfo(ChessStateBase chess)
    {
        if (chess == null)
        {
            Hide();
            return;
        }

        currentChess = chess;
        ChessStatData data = chess.BaseData;

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

        // 스킬 툴팁 데이터 연결
        if (skillTooltipTrigger != null)
        {
            skillTooltipTrigger.SetData(data);
        }

        // 코스트별 UI 프레임 연결
        if (costUIData != null)
        {
            CostUIInfo info = costUIData.GetInfo(data.cost);
            if(info != null && info.infoFrameSprite != null)
            {
                frameImage.sprite = info.infoFrameSprite;
            }
        }

        // 체력 / 마나 초기 갱신
        UpdateHPUI();
        UpdateManaUI();

        panel.SetActive(true);

        CreateSynergyUI(chess);
    }

    private void Update()
    {
        if (!panel.activeSelf) return;

        // HP / Mana 실시간 갱신
        if (currentChess != null)
        {
            UpdateHPUI();
            UpdateManaUI();
        }

        // 기존 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            // UI 위 클릭이면 무시
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out _))
            {
                Hide();
            }
        }
    }

    private void UpdateHPUI()
    {
        if (currentChess == null) return;

        int currentHP = currentChess.CurrentHP;
        int maxHP = currentChess.MaxHP;

        hpFillImage.fillAmount = (float)currentHP / maxHP;
        hpText.text = $"{currentHP} / {maxHP}";
    }

    private void UpdateManaUI()
    {
        if (currentChess == null) return;

        int currentMana = currentChess.CurrentMana;
        int maxMana = currentChess.BaseData.mana;

        manaFillImage.fillAmount = (float)currentMana / maxMana;
        manaText.text = $"{currentMana} / {maxMana}";
    }

    private void ClearSynergyUI()
    {
        foreach (Transform child in synergyContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateSynergyUI(ChessStateBase chess)
    {
        if (chess == null || chess.BaseData == null)
            return;

        ClearSynergyUI();

        ChessStatData data = chess.BaseData;

        if (data.traits == null || synergyIconPrefab == null || synergyContainer == null)
            return;

        foreach (var trait in data.traits)
        {
            GameObject icon = Instantiate(synergyIconPrefab, synergyContainer);

            Image traitIcon = null;
            TMP_Text traitNameObj = null;

            foreach (var t in icon.GetComponentsInChildren<Transform>())
            {
                if (t.name == "TraitIcon")
                    traitIcon = t.GetComponent<Image>();

                if (t.name == "TraitName")
                    traitNameObj = t.GetComponent<TMP_Text>();
            }

            // 아이콘 설정
            if (traitIcon != null && traitIconDB != null)
            {
                Sprite iconSprite = traitIconDB.GetIcon(trait);
                traitIcon.sprite = iconSprite;

                traitIcon.color = iconSprite != null
                    ? Color.white
                    : Color.gray;
            }

            // 이름 설정
            if (traitNameObj != null)
                traitNameObj.text = traitIconDB.GetDisplayName(trait);
        }
    }



    public void Hide()
    {
        panel.SetActive(false);
        currentChess = null;

        ClearSynergyUI();

        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
    }

}
