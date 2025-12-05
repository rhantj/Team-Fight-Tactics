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
    [SerializeField] private Image portraitImage;           // 유닛 초상화
    [SerializeField] private TMP_Text nameText;             // 유닛 이름
    [SerializeField] private TMP_Text costText;             // 유닛 가격
    [SerializeField] private Image costFrameImage;          // 코스트 프레임 이미지
    [SerializeField] private Image bgImage;                 // 배경 이미지

    public ChessStatData CurrentData { get; private set; }  // 현재 슬롯에 표시 중인 유닛 데이터

    private int slotIndex;
    private ShopManager shopManager;

    /// <summary>
    /// 슬롯 초기화
    /// - null이면 빈 슬롯으로 처리
    /// - 데이터가 있으면 UI 세팅
    /// </summary>
    public void Init(ChessStatData data, CostUIData uiData, int index, ShopManager manager)
    {
        slotIndex = index;
        shopManager = manager;
        CurrentData = data;

        if (data == null)
        {
            ClearSlot();
            return;
        }

        portraitImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // 코스트에 따른 UI 스타일 적용
        CostUIInfo info = uiData.GetInfo(data.cost);
        if (info != null)
        {
            costFrameImage.sprite = info.frameSprite;
            bgImage.color = info.backgroundColor;
        }
    }

    /// <summary>
    /// 슬롯 클릭 시 ShopManager의 BuyUnit 호출
    /// </summary>
    public void OnClickSlot()
    {
        if (CurrentData == null)
            return;

        shopManager.BuyUnit(slotIndex);
    }

    /// <summary>
    /// 슬롯을 빈 상태로 초기화
    /// </summary>
    public void ClearSlot()
    {
        CurrentData = null;

        portraitImage.sprite = null;
        nameText.text = "";
        costText.text = "";
        costFrameImage.sprite = null;

        // 배경 투명화
        bgImage.color = new Color(0, 0, 0, 0);
    }
}
