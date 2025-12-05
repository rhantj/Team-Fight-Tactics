using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField] private CostUIData costUIData;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TMP_Text currentGoldText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text costRateText;

    [Header("Unit Data (임시)")]
    [SerializeField] private ChessStatData[] allUnits;

    [Header("Level Data Table")]
    [SerializeField] private LevelDataTable levelDataTable;

    [Header("Player Info (임시)")]
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private int playerExp = 0;

    [Header("Player Gold")]
    [SerializeField] private int currentGold = 10;

    private ShopSlot[] slots;
    private Dictionary<int, List<ChessStatData>> unitsByCost;

    // ================================================================
    // Shop Lock System
    // ================================================================
    [Header("Shop Lock System")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private UnityEngine.UI.Image lockIconImage;
    [SerializeField] private Sprite lockedSprite;
    private Sprite defaultUnlockedSprite;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 슬롯 자동 탐색
        slots = slotContainer.GetComponentsInChildren<ShopSlot>();

        // 최초 아이콘 스프라이트 저장
        if (lockIconImage != null)
            defaultUnlockedSprite = lockIconImage.sprite;

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
        StartCoroutine(InitUI());
    }

    // ================================================================
    // 잠금 버튼 기능
    // ================================================================
    public void ToggleLock()
    {
        isLocked = !isLocked;

        if (isLocked)
            lockIconImage.sprite = lockedSprite;
        else
            lockIconImage.sprite = defaultUnlockedSprite;

        Debug.Log("Shop Lock State = " + isLocked);
    }

    // ================================================================
    // 골드 관련
    // ================================================================
    private void UpdateGoldUI()
    {
        if (currentGoldText != null)
            currentGoldText.text = currentGold.ToString();
    }

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

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    // ================================================================
    // EXP & 레벨업
    // ================================================================
    public void BuyExp()
    {
        if (!TrySpendGold(4))
            return;

        playerExp += 4;
        UpdateExpUI();
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        LevelData current = GetLevelData(playerLevel);
        if (current == null)
            return;

        while (playerExp >= current.requiredExp)
        {
            playerExp -= current.requiredExp;
            playerLevel++;

            UpdateLevelUI();
            UpdateExpUI();
            UpdateCostRateUI();

            current = GetLevelData(playerLevel);
            if (current == null)
                break;
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "Lv. " + playerLevel;
    }

    private void UpdateExpUI()
    {
        LevelData data = GetLevelData(playerLevel);

        if (data != null)
            expText.text = "EXP: " + playerExp + " / " + data.requiredExp;
        else
            expText.text = "EXP: -";
    }

    // ================================================================
    // 상점 갱신 기능
    // ================================================================
    public void RefreshShop()
    {
        if (isLocked)
        {
            Debug.Log("상점 잠금 상태. RefreshShop 실행되지 않음.");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            int cost = GetRandomCostByLevel(playerLevel);

            // 수정된 유닛 뽑기 함수
            ChessStatData unit = GetRandomUnitByCost(cost);

            slots[i].Init(unit, costUIData, i, this);
        }
    }

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

        Debug.Log(data.unitName + " 구매 완료");

        // 슬롯 비우기
        slots[index].ClearSlot();

        // PoolManager에서 미리 만들어둔 비활성 프리팹을 Spawn
        //GameObject spawned = PoolManager.Instance.Spawn(data.prefabPoolID);

        //AddToBench
    }

    // 판매 기능
    public void SellUnit(ChessStatData data, GameObject obj)
    {
        if (data == null)
            return;

        int sellPrice = data.cost;
        AddGold(sellPrice);

        Debug.Log(data.unitName + " 판매 완료. +" + sellPrice + " Gold");

        // 판매된 유닛을 풀로 되돌림
        //PoolManager.Instance.Despawn(data.prefabPoolID, obj);
    }

    // ================================================================
    // 확률 / 유닛 생성
    // ================================================================
    private int GetRandomCostByLevel(int level)
    {
        LevelData data = GetLevelData(level);
        if (data == null)
            return 1;

        float total = 0;
        foreach (var r in data.rates)
            total += r.rate;

        float rand = Random.Range(0f, total);
        float sum = 0;

        foreach (var r in data.rates)
        {
            sum += r.rate;
            if (rand <= sum)
                return r.cost;
        }

        return data.rates[0].cost;
    }

    private ChessStatData GetRandomUnitByCost(int cost)
    {
        List<ChessStatData> list = unitsByCost[cost];

        // 재고 있는 유닛만 후보에 넣는다
        List<ChessStatData> candidates = new List<ChessStatData>();

        for (int i = 0; i < list.Count; i++)
        {
            ChessStatData unit = list[i];

            // 풀에서 사용할 ID. 미리 SO에 세팅되어 있어야 한다
            /*
            int stock = PoolManager.Instance.GetAvailableCount(unit.prefabPoolID);

            if (stock > 0)
                candidates.Add(unit);
            */
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private LevelData GetLevelData(int level)
    {
        foreach (var lv in levelDataTable.levels)
        {
            if (lv.level == level)
                return lv;
        }
        return null;
    }

    // ================================================================
    // 등장확률 UI
    // ================================================================
    private void UpdateCostRateUI()
    {
        LevelData data = GetLevelData(playerLevel);
        if (data == null)
        {
            if (costRateText != null)
                costRateText.text = "-";
            return;
        }

        string result = "";
        foreach (var r in data.rates)
        {
            result += r.cost + "Cost: " + r.rate + "%  ";
        }

        if (costRateText != null)
            costRateText.text = result;
    }
    // ================================================================
    // 리롤 기능
    // ================================================================
    public void Reroll()
    {
        if (isLocked)
        {
            Debug.Log("상점이 잠겨 있어 리롤 불가");
            return;
        }

        if (!TrySpendGold(2))
            return;

        RefreshShop();
    }


    // ================================================================
    // UI 초기화
    // ================================================================
    private IEnumerator InitUI()
    {
        yield return null;

        UpdateGoldUI();
        UpdateLevelUI();
        UpdateExpUI();
        UpdateCostRateUI();
        RefreshShop();
    }
}
