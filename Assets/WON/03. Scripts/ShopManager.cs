using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 상점 전체 로직을 관리하는 매니저
/// - 유닛 슬롯 초기화 및 갱신
/// - 골드 사용 및 UI 갱신
/// - 경험치 구매 및 레벨업 처리
/// - 코스트 확률 기반 랜덤 유닛 생성
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CostUIData costUIData;     // 코스트별 UI 스타일 SO
    [SerializeField] private Transform slotContainer;   // 슬롯 5개가 배치된 부모 오브젝트
    [SerializeField] private TMP_Text currentGoldText;  // 현재 골드 텍스트
    [SerializeField] private TMP_Text levelText;        // 현재 레벨 텍스트
    [SerializeField] private TMP_Text expText;          // 경험치 텍스트

    [Header("Unit Data (임시)")]
    [SerializeField] private ChessStatData[] allUnits;  // 나중에 PoolManager에서 제공받을 예정

    [Header("Level Data Table")]
    [SerializeField] private LevelDataTable levelDataTable;  // 레벨 기반 확률/필드/경험치 정보

    [Header("Player Info (임시)")]
    [SerializeField] private int playerLevel = 1;  // 현재 플레이어 레벨
    [SerializeField] private int playerExp = 0;     // 현재 경험치

    [Header("Player Gold")]
    [SerializeField] private int currentGold = 10;  // 현재 보유 골드

    private ShopSlot[] slots;                        // SlotContainer 내부의 슬롯들
    private Dictionary<int, List<ChessStatData>> unitsByCost; // 코스트별 유닛 목록


    // ================================================================
    // 초기화
    // ================================================================
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 슬롯 5개 자동 탐색
        slots = slotContainer.GetComponentsInChildren<ShopSlot>();

        // 코스트별 유닛 분류
        unitsByCost = new Dictionary<int, List<ChessStatData>>();

        foreach (var unit in allUnits)
        {
            if (!unitsByCost.ContainsKey(unit.cost))
                unitsByCost[unit.cost] = new List<ChessStatData>();

            unitsByCost[unit.cost].Add(unit);
        }
    }

    private void Start()
    {
        UpdateGoldUI();
        UpdateLevelUI();
        UpdateExpUI();
        RefreshShop();
    }


    // ================================================================
    // 골드 관련 기능
    // ================================================================

    /// <summary>
    /// 현재 골드를 UI에 반영한다.
    /// </summary>
    private void UpdateGoldUI()
    {
        if (currentGoldText != null)
            currentGoldText.text = currentGold.ToString();
    }

    /// <summary>
    /// 지정된 금액을 지불할 수 있는지 확인 후 차감한다.
    /// 실패하면 false를 반환한다.
    /// </summary>
    private bool TrySpendGold(int amount)
    {
        if (currentGold < amount)
        {
            Debug.Log("골드 부족");
            return false;
        }

        currentGold -= amount;
        UpdateGoldUI();
        return true;
    }

    /// <summary>
    /// 외부에서 골드를 획득할 때 호출하는 함수
    /// </summary>
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }


    // ================================================================
    // EXP & 레벨업 관련 기능
    // ================================================================

    /// <summary>
    /// 경험치 구매 기능 (기본 4골드 사용)
    /// </summary>
    public void BuyExp()
    {
        if (!TrySpendGold(4))
            return;

        playerExp += 4;
        UpdateExpUI();
        CheckLevelUp();
    }

    /// <summary>
    /// 현재 레벨의 요구 경험치를 확인하여 레벨업 처리
    /// </summary>
    private void CheckLevelUp()
    {
        LevelData currentLevelData = GetLevelData(playerLevel);
        if (currentLevelData == null)
            return;

        while (playerExp >= currentLevelData.requiredExp)
        {
            playerExp -= currentLevelData.requiredExp;
            playerLevel++;

            UpdateLevelUI();
            UpdateExpUI();

            currentLevelData = GetLevelData(playerLevel);
            if (currentLevelData == null)
                break;
        }
    }

    /// <summary>
    /// 레벨 텍스트 UI 갱신
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = $"Lv. {playerLevel}";
    }

    /// <summary>
    /// 경험치 텍스트 UI 갱신
    /// </summary>
    private void UpdateExpUI()
    {
        LevelData data = GetLevelData(playerLevel);

        if (data != null)
            expText.text = $"EXP: {playerExp} / {data.requiredExp}";
        else
            expText.text = "EXP: -";
    }


    // ================================================================
    // 상점 슬롯 갱신 기능
    // ================================================================

    /// <summary>
    /// 상점 슬롯 5개를 모두 새 유닛으로 갱신한다.
    /// </summary>
    public void RefreshShop()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int cost = GetRandomCostByLevel(playerLevel);
            ChessStatData unit = GetRandomUnitByCost(cost);

            slots[i].Init(unit, costUIData, i, this);
        }
    }

    /// <summary>
    /// 슬롯 클릭 시 호출되는 구매 처리
    /// </summary>
    public void BuyUnit(int index)
    {
        ChessStatData data = slots[index].CurrentData;

        if (data == null)
        {
            Debug.Log("빈 슬롯 클릭");
            return;
        }

        if (!TrySpendGold(data.cost))
            return;

        Debug.Log($"{data.unitName} 구매 완료");

        // 슬롯을 빈 상태로 초기화
        slots[index].ClearSlot();

        // TODO: 구매한 기물을 벤치 AddToBench(data) 로 넘길 예정
    }


    // ================================================================
    // 랜덤 확률 기반 유닛 선택 기능
    // ================================================================

    /// <summary>
    /// 현재 레벨 기반으로 코스트 확률을 계산하여 하나를 반환한다.
    /// </summary>
    private int GetRandomCostByLevel(int level)
    {
        LevelData data = GetLevelData(level);
        if (data == null)
            return 1;

        float total = 0;
        foreach (var r in data.rates)
            total += r.rate;

        float rand = Random.Range(0f, total);
        float cumulative = 0;

        foreach (var r in data.rates)
        {
            cumulative += r.rate;
            if (rand <= cumulative)
                return r.cost;
        }

        return data.rates[0].cost;
    }

    /// <summary>
    /// 해당 코스트의 유닛 리스트에서 랜덤으로 하나 선택
    /// </summary>
    private ChessStatData GetRandomUnitByCost(int cost)
    {
        var list = unitsByCost[cost];
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 입력된 레벨에 해당하는 LevelData를 반환한다.
    /// </summary>
    private LevelData GetLevelData(int level)
    {
        foreach (var lv in levelDataTable.levels)
        {
            if (lv.level == level)
                return lv;
        }

        Debug.LogWarning($"Level {level} 데이터가 존재하지 않음");
        return null;
    }


    // ================================================================
    // 리롤 기능
    // ================================================================

    /// <summary>
    /// 리롤 기능 (2골드 사용)
    /// </summary>
    public void Reroll()
    {
        if (!TrySpendGold(2))
            return;

        RefreshShop();
    }
}
