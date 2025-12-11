using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;   // 빈 공간 클릭 감지용

public class ChessInfoUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject panel;

    [Header("Basic Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;

    [Header("Stats")]
    //[SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    //[SerializeField] private TMP_Text manaText;

    [Header("Skill")]
    //[SerializeField] private TMP_Text skillNameText;
    //[SerializeField] private TMP_Text skillDescText;

    [Header("Skill Icon (SO에 skillIcon 추가되면 활성화)")]
    [SerializeField] private Image skillIconImage;

    //[Header("Traits")]
    //[SerializeField] private TMP_Text traitText;


    private void Awake()
    {
        panel.SetActive(false);
    }

    // ============================================================
    //              옵저버 패턴: 이벤트 구독/해제
    // ============================================================
    private void OnEnable()
    {
        SelectionSubject.OnUnitSelected += ShowInfo;
    }

    private void OnDisable()
    {
        SelectionSubject.OnUnitSelected -= ShowInfo;
    }


    // ============================================================
    //                   UI 업데이트 (Observer)
    // ============================================================
    public void ShowInfo(ChessStatData data)
    {
        if (data == null)
        {
            Hide();
            return;
        }

        // -------- 기본 정보 --------
        iconImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // -------- 스탯 --------
        //hpText.text = data.maxHP.ToString();
        armorText.text = data.armor.ToString();
        attackDamageText.text = data.attackDamage.ToString();
        attackSpeedText.text = data.attackSpeed.ToString("0.00");
        //manaText.text = data.mana.ToString();

        // -------- 스킬 --------
        //skillNameText.text = data.skillName;
        //skillDescText.text = data.skillDescription;

        // -------- 스킬 아이콘 (SO에 추가되면 사용 가능)
        skillIconImage.sprite = data.skillIcon;

        // -------- 시너지 / 특성 --------
        //traitText.text = GetTraitString(data.traits);

        panel.SetActive(true);
    }


    // ============================================================
    //                   빈 공간 클릭 시 UI 닫기
    // ============================================================
    private void Update()
    {
        if (!panel.activeSelf) return;

        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            // UI 클릭이면 무시
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // 3D 오브젝트 클릭인지 확인
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


    private string GetTraitString(TraitType[] traits)
    {
        if (traits == null || traits.Length == 0)
            return "";
        return string.Join(" / ", traits);
    }
}
