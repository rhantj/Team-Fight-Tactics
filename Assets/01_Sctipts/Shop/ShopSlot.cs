using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점 슬롯 1칸의 UI 처리
/// - 유닛 정보 표시
/// - 구매 버튼 이벤트 전달
/// - 빈 슬롯 처리
/// - 특성(시너지) 아이콘 표시
/// </summary>
public class ShopSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;       // 유닛 초상화
    [SerializeField] private TMP_Text nameText;         // 유닛 이름
    [SerializeField] private TMP_Text costText;         // 유닛 가격
    [SerializeField] private Image costFrameImage;      // 코스트 프레임
    [SerializeField] private Image bgImage;             // 배경 이미지
    [SerializeField] private Image goldImage;           // 골드 아이콘 이미지

    [Header("Synergy UI")]
    [SerializeField] private Transform synergyContainer;     // 시너지 아이콘들이 들어갈 부모
    [SerializeField] private GameObject synergyIconPrefab;   // 시너지 아이콘 프리팹

    [Header("Trait Icon Database")]
    [SerializeField] private TraitIconDatabase traitIconDB;  // 시너지 아이콘 데이터베이스

    public ChessStatData CurrentData { get; private set; }

    private int slotIndex;
    private ShopManager shopManager;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Init(ChessStatData data, CostUIData uiData, int index, ShopManager manager)
    {
        slotIndex = index;
        shopManager = manager;
        CurrentData = data;

        // 시너지 아이콘 먼저 초기화
        ClearSynergyIcons();

        // 빈 슬롯 처리
        if (data == null)
        {
            ClearSlot();
            return;
        }

        // UI 복구
        portraitImage.color = Color.white;
        costFrameImage.color = Color.white;
        goldImage.color = Color.white;
        goldImage.enabled = true;

        portraitImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // 코스트 UI 적용
        CostUIInfo info = uiData.GetInfo(data.cost);
        if (info != null)
        {
            costFrameImage.sprite = info.frameSprite;

            // 배경색 alpha 보정 (투명 문제 방지)
            Color bg = info.backgroundColor;
            bg.a = 1f;
            bgImage.color = bg;
        }
        else
        {
            bgImage.color = Color.white;
        }

        // ======================
        //   시너지 아이콘 생성
        // ======================
        GenerateSynergyIcons(data);
    }

    /// <summary>
    /// 슬롯 클릭 시 구매
    /// </summary>
    public void OnClickSlot()
    {
        if (CurrentData == null)
            return;

        shopManager.BuyUnit(slotIndex);
    }

    /// <summary>
    /// 빈 슬롯으로 만들기
    /// </summary>
    public void ClearSlot()
    {
        CurrentData = null;

        // 초상화 투명화
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0);

        // 텍스트 제거
        nameText.text = "";
        costText.text = "";

        // 프레임 투명 처리
        costFrameImage.sprite = null;
        costFrameImage.color = new Color(1, 1, 1, 0);

        // 골드 아이콘 숨기기
        goldImage.enabled = false;

        // 배경 투명화
        bgImage.color = new Color(1, 1, 1, 0);

        // 시너지 아이콘 제거
        ClearSynergyIcons();
    }

    // ===============================
    // 시너지 처리용 함수들
    // ===============================

    /// <summary>
    /// 기존에 생성된 시너지 아이콘 전부 제거
    /// </summary>
    private void ClearSynergyIcons()
    {
        if (synergyContainer == null) return;

        foreach (Transform child in synergyContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 유닛의 traits 데이터 기반으로 아이콘 생성
    /// </summary>
    private void GenerateSynergyIcons(ChessStatData data)
    {
        if (data.traits == null || synergyIconPrefab == null || synergyContainer == null)
            return;

        foreach (var trait in data.traits)
        {
            GameObject icon = Instantiate(synergyIconPrefab, synergyContainer);

            // SynergyIconPrefab 구조:
            // icon (root)
            //   ├─ TraitIcon (Image)
            //   └─ TraitName (TMP_Text)

            Image traitIcon = null;
            TMP_Text traitNameObj = null;

            foreach (var t in icon.GetComponentsInChildren<Transform>())
            {
                if (t.name == "TraitIcon")
                    traitIcon = t.GetComponent<Image>();

                if (t.name == "TraitName")
                    traitNameObj = t.GetComponent<TMP_Text>();
            }

            // 아이콘 스프라이트 설정
            if (traitIcon != null && traitIconDB != null)
            {
                Sprite iconSprite = traitIconDB.GetIcon(trait);
                traitIcon.sprite = iconSprite;

                if (iconSprite != null)
                {
                    traitIcon.color = Color.white;   // 아이콘 정상 표시
                }
                else
                {
                    traitIcon.color = Color.gray;    // 아이콘 누락 시 회색 처리
                }
            }

            // 이름 설정 (enum 이름 출력)
            if (traitNameObj != null)
                traitNameObj.text = trait.ToString();
        }
    }
}
