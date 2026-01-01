using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 상점 슬롯 1칸의 UI 및 상호작용을 담당하는 컴포넌트.
/// 
/// - 유닛 정보 표시 (초상화, 이름, 가격)
/// - 코스트별 UI 스타일 적용
/// - 시너지(특성) 아이콘 생성
/// - 슬롯 클릭 시 구매 이벤트를 ShopManager로 전달
/// - 빈 슬롯 상태 처리
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
    [SerializeField] private TraitIconDataBase traitIconDB;  // 시너지 아이콘 데이터베이스

    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material grayscaleMaterial;

    [Header("Star Hint UI")]
    [SerializeField] private GameObject starHintRoot;
    [SerializeField] private Image twoStarImage;
    [SerializeField] private Image threeStarImage;

    [Header("Border Frame UI")]
    [SerializeField] private Image borderFrameImage;
    [SerializeField] private Sprite silverBorderSprite;
    [SerializeField] private Sprite goldBorderSprite;



    /// <summary>
    /// 현재 슬롯에 표시 중인 유닛 데이터.
    /// null일 경우 빈 슬롯 상태를 의미한다.
    /// </summary>
    public ChessStatData CurrentData { get; private set; }

    /// <summary>
    /// ShopManager에서 전달받은 슬롯 인덱스.
    /// 클릭 시 어떤 슬롯이 선택되었는지 식별하는 용도로 사용된다.
    /// </summary>
    private int slotIndex;

    /// <summary>
    /// 상점 시스템의 파사드 매니저 참조.
    /// 구매 요청을 위임하기 위해 사용된다.
    /// </summary>
    private ShopManager shopManager;

    private Tween starTween;

    /// <summary>
    /// 상점 갱신시 샵슬롯의 상태를 완성하는 메서드
    /// </summary>
    public void Init(ChessStatData data, CostUIData uiData, int index, ShopManager manager)
    {
        slotIndex = index;
        shopManager = manager;
        CurrentData = data;

        bgImage.enabled = true;

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

            // ShopSlotHoverEffect 기준색 캡처
            bgImage.GetComponent<ShopSlotHoverEffect>()?.CaptureBaseColor();
        }
        else
        {
            bgImage.color = Color.white;

            bgImage.GetComponent<ShopSlotHoverEffect>()?.CaptureBaseColor();
        }


        // ======================
        //   시너지 아이콘 생성
        // ======================
        GenerateSynergyIcons(data);

        ResetStarHint();
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

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }
        bgImage.enabled = false;

        // 시너지 아이콘 제거
        ClearSynergyIcons();

        ResetStarHint();
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
            if (traitNameObj != null && traitIconDB != null)
                traitNameObj.text = traitIconDB.GetDisplayName(trait);

        }
    }


    public void SetAffordable(bool canBuy)
    {
        portraitImage.material = canBuy
            ? defaultMaterial
            : grayscaleMaterial;
    }

    public void SetStarHint(bool canMake2Star, bool canMake3Star)
    {
        if (starHintRoot == null)
            return;

        // ====== [1] 항상 초기화 ======
        starTween?.Kill();
        starTween = null;

        starHintRoot.SetActive(false);

        if (twoStarImage != null)
            twoStarImage.gameObject.SetActive(false);

        if (threeStarImage != null)
            threeStarImage.gameObject.SetActive(false);

        if (borderFrameImage != null)
            borderFrameImage.gameObject.SetActive(false);

        // ====== [2] 조건 없으면 여기서 종료 ======
        if (!canMake2Star && !canMake3Star)
            return;

        // ====== [3] 다시 켜기 ======
        starHintRoot.SetActive(true);

        if (canMake3Star)
        {
            threeStarImage.gameObject.SetActive(true);
            PlayStarBlink(threeStarImage);

            if (borderFrameImage != null)
            {
                borderFrameImage.sprite = goldBorderSprite;
                borderFrameImage.gameObject.SetActive(true);
            }
        }
        else if (canMake2Star)
        {
            twoStarImage.gameObject.SetActive(true);
            PlayStarBlink(twoStarImage);

            if (borderFrameImage != null)
            {
                borderFrameImage.sprite = silverBorderSprite;
                borderFrameImage.gameObject.SetActive(true);
            }
        }
    }




    private void ResetStarHint()
    {
        starTween?.Kill();
        starTween = null;

        if (starHintRoot != null)
            starHintRoot.SetActive(false);

        if (twoStarImage != null)
            twoStarImage.gameObject.SetActive(false);

        if (threeStarImage != null)
            threeStarImage.gameObject.SetActive(false);

        if (borderFrameImage != null)
            borderFrameImage.gameObject.SetActive(false);
    }


    // 별표시 효과연출용 메서드
    private void PlayStarBlink(Image target)
    {
        if (target == null)
            return;

        // 혹시 기존 트윈이 남아있으면 제거
        starTween?.Kill();

        target.transform.localScale = Vector3.one;
        Color c = target.color;
        c.a = 1f;
        target.color = c;

        // 스케일 + 알파 동시 반복
        starTween = DOTween.Sequence()
            .Join(
                target.transform.DOScale(1.1f, 0.6f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
            )
            .Join(
                target.DOFade(0.6f, 0.6f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
            );
    }




}
