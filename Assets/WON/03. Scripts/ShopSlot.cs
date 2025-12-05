using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점 슬롯 1칸의 UI 처리
/// - 유닛 정보 표시
/// - 구매 버튼 이벤트 전달
/// - 빈 슬롯 처리
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

        // 빈 슬롯일 때
        if (data == null)
        {
            ClearSlot();
            return;
        }

        // UI 완전 복구
        portraitImage.color = Color.white;
        costFrameImage.color = Color.white;
        goldImage.color = Color.white;

        portraitImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        goldImage.enabled = true;

        // 코스트 스타일 적용
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
    /// 빈 슬롯으로 초기화 (구매 후 슬롯 비우기)
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
    }
}
