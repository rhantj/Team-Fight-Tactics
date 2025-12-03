using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점의 한 슬롯을 표현하는 클래스
/// 유닛 데이터를 UI에 표시하고, 클릭 시 구매 요청을 ShopManager에 전달한다.
/// </summary>
public class ShopSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;   // 초상화 이미지
    [SerializeField] private TMP_Text nameText;     // 기물 이름
    [SerializeField] private TMP_Text costText;     // 기물 코스트 텍스트
    [SerializeField] private Image costFrameImage;  // 프레임 이미지
    [SerializeField] private Image bgImage;         // 배경 색상

    [Header("Debug Test (테스트 전용 설정)")]
    [SerializeField] private CostUIData debugUIData; // 테스트할 때 사용할 CostUIData

    public ChessStatData CurrentData { get; private set; }

    private int slotIndex;
    private ShopManager shopManager;

    // ================================================================
    // 초기화
    // ================================================================

    /// <summary>
    /// 상점 매니저가 호출하는 슬롯 초기화 함수
    /// 슬롯 인덱스, 매니저 참조, 유닛 데이터 및 UI 세팅을 모두 처리한다.
    /// </summary>
    public void Init(ChessStatData data, CostUIData uiData, int index, ShopManager manager)
    {
        slotIndex = index;
        shopManager = manager;
        CurrentData = data;

        // UI 표시
        portraitImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // 코스트 UI 적용
        var ui = uiData.GetInfo(data.cost);
        if (ui != null)
        {
            costFrameImage.sprite = ui.frameSprite;
            bgImage.color = ui.backgroundColor;
        }
    }

    // ================================================================
    // 슬롯 클릭
    // ================================================================

    /// <summary>
    /// 버튼 클릭 시 호출되는 함수
    /// ShopManager에 구매 요청을 전달한다.
    /// </summary>
    public void OnClickSlot()
    {
        if (CurrentData == null)
        {
            Debug.Log("빈 슬롯 클릭됨. 구매 불가.");
            return;
        }

        shopManager.BuyUnit(slotIndex);
    }

    // ================================================================
    // 슬롯 초기화 (구매 후 빈 상태)
    // ================================================================

    /// <summary>
    /// 슬롯을 "빈 슬롯" 상태로 만든다.
    /// 실제 오브젝트는 유지되고 UI만 초기화된다.
    /// </summary>
    public void ClearSlot()
    {
        CurrentData = null;

        portraitImage.sprite = null;
        nameText.text = "";
        costText.text = "";
        costFrameImage.sprite = null;

        // 투명 배경 처리
        bgImage.color = new Color(0f, 0f, 0f, 0f);
    }


    // ================================================================
    // 테스트 기능 (코스트별 UI 확인용)
    // ================================================================

    /// <summary>
    /// 공통 UI 테스트용 내부 함수
    /// </summary>
    private void ApplyTestUI(int testCost)
    {
        if (debugUIData == null)
        {
            Debug.LogError("debugUIData가 설정되지 않았습니다.");
            return;
        }

        // 가짜 데이터 생성
        ChessStatData fake = ScriptableObject.CreateInstance<ChessStatData>();
        fake.unitName = $"Test {testCost}";
        fake.cost = testCost;
        fake.icon = null;

        CurrentData = fake;
        nameText.text = fake.unitName;
        costText.text = fake.cost.ToString();

        // UI 정보 적용
        var ui = debugUIData.GetInfo(testCost);
        if (ui != null)
        {
            costFrameImage.sprite = ui.frameSprite;
            bgImage.color = ui.backgroundColor;
        }
    }

    // 버튼에서 직접 호출할 테스트 함수들
    public void TestCost1() => ApplyTestUI(1);
    public void TestCost2() => ApplyTestUI(2);
    public void TestCost3() => ApplyTestUI(3);
}
