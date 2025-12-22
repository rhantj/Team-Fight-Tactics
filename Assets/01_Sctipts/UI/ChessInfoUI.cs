using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 선택된 기물(Chess)의 상세 정보를 화면에 표시하는 UI 매니저.
///
/// - 기물 기본 정보(아이콘, 이름, 코스트)
/// - 스탯 정보(방어력, 공격력, 공격속도)
/// - 스킬 아이콘 및 스킬 툴팁 연동
/// - 체력 / 마나 UI 실시간 갱신
/// - 시너지(특성) 아이콘 표시
///
/// 씬 내에서 단 하나만 존재하도록 Singleton 기반으로 설계되었으며,
/// 기물 선택 시 ShowInfo, 해제 시 Hide를 통해 제어된다.
/// </summary>
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

    /// <summary>
    /// 스킬 아이콘에 마우스를 올렸을 때 표시되는 툴팁 트리거.
    /// </summary>
    private SkillTooltipTrigger skillTooltipTrigger;

    [Header("Cost UI Data")]
    [SerializeField] private CostUIData costUIData;

    [Header("HP / Shield UI")]
    [SerializeField] private RectTransform hpFill;          // 실제 체력(초록색) 바
    [SerializeField] private RectTransform shieldFill;      // 실드(하얀색) 바
    [SerializeField] private float hpBarMaxWidth = 200f;    // HPBar 전체 길이 (Inspector에서 설정)
    [SerializeField] private TMP_Text hpText;

    [Header("Mana UI")]
    [SerializeField] private Image manaFillImage;
    [SerializeField] private TMP_Text manaText;

    private ChessStateBase currentChess;

    [Header("Synerge UI")]
    [SerializeField] private Transform synergyContainer;
    [SerializeField] private GameObject synergyIconPrefab;

    [Header("Trait Icon Database")]
    [SerializeField] private TraitIconDatabase traitIconDB;

    [Header("Item Slots (Info UI)")]
    [SerializeField] private ChessInfoItemSlot[] itemSlots;

    [Header("Item Slot Sprites")]
    [SerializeField] private Sprite emptyItemSlotSprite;



    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        skillTooltipTrigger = skillIconImage.GetComponent<SkillTooltipTrigger>();

        panel.SetActive(false);
    }

    /// <summary>
    /// 특정 기물의 정보를 UI에 표시한다.
    /// 기물 선택 시 외부에서 호출된다.
    /// </summary>
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

        // 스탯 (현재 스탯 기준)
        UpdateStatUI();

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
            if (info != null && info.infoFrameSprite != null)
            {
                frameImage.sprite = info.infoFrameSprite;
            }
        }

        // 체력 / 마나 초기 갱신
        UpdateHPUI();
        UpdateManaUI();

        panel.SetActive(true);

        CreateSynergyUI(chess);

        // 스킬 아이콘 동기화 함수
        SyncItemSlotsFromWorldUI(chess);
    }

    private void Update()
    {
        if (!panel.activeSelf) return;

        // HP / Mana 실시간 갱신
        if (currentChess != null)
        {
            UpdateHPUI();
            UpdateManaUI();
            UpdateStatUI(); // 시너지 반영 실시간 갱신
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

    /// <summary>
    /// 현재 기물의 체력 정보를 UI에 반영한다.
    ///
    /// - 실드가 없는 경우: 일반적인 HP Bar 표시
    /// - 실드가 있는 경우:
    ///   * 초록색 = 실제 HP
    ///   * 하얀색 = 실드
    ///   * 실드는 항상 오른쪽부터 차오르며,
    ///     HP + Shield 합이 MaxHP를 초과하면
    ///     (HP + Shield) 기준 비율로 실드를 표시한다.
    /// </summary>
    private void UpdateHPUI()
    {
        if (currentChess == null) return;

        int currentHP = currentChess.CurrentHP;
        int currentShield = currentChess.CurrentShield;
        int maxHP = currentChess.MaxHP;

        // 텍스트는 HP + Shield 합산값으로 표시
        hpText.text = $"{currentHP + currentShield} / {maxHP}";

        // ===========================
        // HP Bar (항상 MaxHP 기준)
        // ===========================
        float hpRatio = Mathf.Clamp01((float)currentHP / maxHP);
        float hpWidth = hpBarMaxWidth * hpRatio;

        hpFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hpWidth);
        hpFill.anchoredPosition = Vector2.zero;

        // 실드 없으면 종료
        if (currentShield <= 0)
        {
            shieldFill.gameObject.SetActive(false);
            return;
        }

        shieldFill.gameObject.SetActive(true);

        // ===========================
        // CASE 1
        // CurrentHP + Shield <= MaxHP
        // ===========================
        if (currentHP + currentShield <= maxHP)
        {
            float shieldRatio = (float)currentShield / maxHP;
            float shieldWidth = hpBarMaxWidth * shieldRatio;

            shieldFill.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                shieldWidth
            );

            // HP 오른쪽에 붙여서 표시
            shieldFill.anchoredPosition =
                new Vector2(hpWidth, 0f);
        }
        // ===========================
        // CASE 2
        // CurrentHP + Shield > MaxHP
        // ===========================
        else
        {
            // (HP + Shield) 기준으로 Shield 비율 계산
            float total = currentHP + currentShield;
            float shieldRatio = currentShield / total;
            float shieldWidth = hpBarMaxWidth * shieldRatio;

            shieldFill.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                shieldWidth
            );

            // 오른쪽부터 차오르게 배치
            shieldFill.anchoredPosition =
                new Vector2(hpBarMaxWidth - shieldWidth, 0f);
        }
    }


    /// <summary>
    /// 현재 기물의 마나 정보를 UI에 반영한다.
    /// </summary>
    private void UpdateManaUI()
    {
        if (currentChess == null) return;

        int currentMana = currentChess.CurrentMana;
        int maxMana = currentChess.BaseData.mana;

        manaFillImage.fillAmount = (float)currentMana / maxMana;
        manaText.text = $"{currentMana} / {maxMana}";
    }

    /// <summary>
    /// 현재 기물의 스탯 정보를 UI에 반영한다.
    /// 시너지 효과가 적용된 "실시간 스탯"을 표시한다.
    /// </summary>
    private void UpdateStatUI()
    {
        if (currentChess == null) return;

        armorText.text = currentChess.Armor.ToString();
        attackDamageText.text = currentChess.AttackDamage.ToString();
        attackSpeedText.text = currentChess.AttackSpeed.ToString("0.00");
    }

    /// <summary>
    /// 기존에 생성된 시너지 UI를 모두 제거한다.
    /// </summary>
    private void ClearSynergyUI()
    {
        foreach (Transform child in synergyContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 기물이 보유한 특성(traits)을 기반으로 시너지 아이콘 UI를 생성한다.
    /// </summary>
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

    /// <summary>
    /// 기물 정보 UI를 숨기고 상태를 초기화한다.
    /// </summary>
    public void Hide()
    {
        panel.SetActive(false);
        currentChess = null;

        ClearSynergyUI();

        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
    }

    /// <summary>
    /// 기물에 장착된 아이템 데이터를 기준으로
    /// 기물 정보 UI의 아이템 슬롯을 갱신한다.
    /// 
    /// - ChessItemHandler의 EquippedItems를 기준으로 동작
    /// - 슬롯은 항상 존재하며, 아이템 유무만 갱신
    /// - 마우스 오버 / 우클릭 툴팁은 ChessInfoItemSlot에서 처리
    /// </summary>
    private void SyncItemSlotsFromWorldUI(ChessStateBase chess)
    {
        Debug.Log("[ChessInfoUI] SyncItemSlotsFromWorldUI CALLED");

        if (chess == null || itemSlots == null)
            return;

        // 1. 모든 슬롯 초기화
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].Clear();
        }

        // 2. ChessItemHandler에서 실제 장착 아이템 데이터 가져오기
        ChessItemHandler itemHandler = chess.GetComponent<ChessItemHandler>();
        if (itemHandler == null)
            return;

        IReadOnlyList<ItemData> equippedItems = itemHandler.EquippedItems;

        // 3. 장착된 아이템을 슬롯에 순서대로 반영
        for (int i = 0; i < equippedItems.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].SetItem(equippedItems[i]);
        }
    }


    public void RefreshItemUIOnly()
    {
        if (currentChess == null) return;
        SyncItemSlotsFromWorldUI(currentChess);

        UpdateStatUI();
        UpdateHPUI();
    }

    // 기물이 팔릴때 알릴 메서드 -> 기물정보UI닫기와 연결
    public void NotifyChessSold(ChessStateBase soldChess)
    {
        if (currentChess == soldChess)
        {
            Hide();
        }
    }


}
