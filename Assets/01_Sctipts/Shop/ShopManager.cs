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
public class ShopManager : Singleton<ShopManager>
{
    
    [Header("UI References")]
    [SerializeField] private CostUIData costUIData;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TMP_Text currentGoldText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text costRateText;
    [SerializeField] private TMP_Text sellPriceText;

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
    private Dictionary<ChessStatData, int> unitBuyCount = new Dictionary<ChessStatData, int>();


    // ================================================================
    // Shop Lock System
    // ================================================================
    [Header("Shop Lock System")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private UnityEngine.UI.Image lockIconImage;
    [SerializeField] private Sprite lockedSprite;
    private Sprite defaultUnlockedSprite;

    protected override void Awake()
    {
        base.Awake();

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
            ChessStatData unit = GetRandomUnitByCost(cost);

            slots[i].Init(unit, costUIData, i, this);

            //if (unit != null)
            //{
            //    if (!shopAppearCount.ContainsKey(unit))
            //        shopAppearCount[unit] = 0;

            //    shopAppearCount[unit]++;
            //} // 안씁니다.
        }
    }

    private ChessStatData GetRandomUnitByCost(int cost)
    {
        List<ChessStatData> list = unitsByCost[cost];
        List<ChessStatData> candidates = new List<ChessStatData>();

        foreach (var unit in list)
        {
            int stock = PoolManager.Instance.GetRemainCount(unit.poolID);
            if (stock <= 0)
                continue;

            if (ChessCombineManager.Instance != null &&
                ChessCombineManager.Instance.IsUnitCompleted(unit))
                continue;

            if (unitBuyCount.TryGetValue(unit, out int bought) && bought >= 9)
                continue;

            candidates.Add(unit);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }



    // ================================================================
    // 판매 가격 계산 (성급 반영)
    // ================================================================
    public int CalculateSellPrice(ChessStatData data, int starLevel)
    {
        int cost = data.cost;
        int price = 0;

        switch (starLevel)
        {
            case 1:
                price = cost;
                break;

            case 2:
                price = cost * 3;
                break;

            case 3:
                price = cost * 9;
                break;

            default:
                price = cost;
                break;
        }

        // cost가 2 이상이고, 2성 또는 3성일 때 -1 적용
        if (cost >= 2 && starLevel >= 2)
            price -= 1;

        return price;
    }


    public void BuyUnit(int index)
    {
        // 1) 슬롯 데이터 확인
        ChessStatData data = slots[index].CurrentData;

        if (data == null)
        {
            Debug.Log("빈 슬롯 클릭");
            return;
        }

        // 2) 골드 체크
        if (!TrySpendGold(data.cost))
            return;

        Debug.Log(data.unitName + " 구매 시도");

        // 슬롯은 아직 지우지 않고 벤치 배치 성공 여부 확인 후 지우도록 하자

        // 3) 풀에서 유닛 생성
        GameObject obj = PoolManager.Instance.Spawn(data.poolID);

        // Chess가 자식에 있으므로 GetComponentInChildren 사용
        Chess chess = obj.GetComponentInChildren<Chess>();

        if (chess == null)
        {
            Debug.LogError("Spawn된 오브젝트에서 Chess 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        chess.SetBaseData(data); //25.12.08 Add Kim


        // 4) BenchGrid 찾기
        BenchGrid bench = FindObjectOfType<BenchGrid>();

        if (bench == null)
        {
            Debug.LogError("BenchGrid를 찾을 수 없습니다.");
            return;
        }

        // 5) 배치 실패 대비 기존 위치 저장
        Vector3 beforePos = obj.transform.position;

        // 6) 벤치 배치 시도
        bench.SetChessOnBenchNode(chess);

        // 7) 벤치가 꽉 차서 배치 실패한 경우
        if (chess.transform.position == beforePos)
        {
            Debug.Log("벤치가 가득 차서 구매 불가!");

            // 유닛 반환
            PoolManager.Instance.Despawn(data.poolID, obj);

            
            AddGold(data.cost);

            return;
        }

        // 8) 여기서야 구매 확정 → 슬롯 비우기
        slots[index].ClearSlot();

        Debug.Log($"{data.unitName} 구매 완료 및 벤치 배치 성공!");

        if (!unitBuyCount.ContainsKey(data))
            unitBuyCount[data] = 0;

        unitBuyCount[data]++;

        ChessCombineManager.Instance?.Register(chess); //25.12.08 Add KIM
    }

    // 판매 기능
    public void SellUnit(ChessStatData data, GameObject obj)
    {
        if (data == null)
            return;

        // 판매되는 유닛의 Chess 컴포넌트 가져오기
        Chess chess = obj.GetComponentInChildren<Chess>(); //25.12.08 Add Kim : 합성 매니저에서 제거합니다.

        // 유닛의 성급 정보 가져오기 (없으면 기본 1성 처리)
        int starLevel = 1;
        if (chess != null)
            starLevel = chess.StarLevel;

        // 성급을 고려한 판매가격 계산
        int sellPrice = CalculateSellPrice(data, starLevel);

        // 판매 골드 지급
        AddGold(sellPrice);

        Debug.Log(data.unitName + " 판매 완료. +" + sellPrice + " Gold");

        if (chess != null)
        {
            ChessCombineManager.Instance?.Unregister(chess);
        }

        // 판매된 유닛을 풀로 되돌림
        PoolManager.Instance.Despawn(data.poolID, obj);
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

    // ================================================================
    // 판매 모드 진입/종료
    // ================================================================
    public void EnterSellMode(int price)
    {
        // 상점 슬롯 전체 숨김
        if (slotContainer != null)
            slotContainer.gameObject.SetActive(false);

        // 판매 가격 텍스트 활성화
        if (sellPriceText != null)
        {
            sellPriceText.gameObject.SetActive(true);
            sellPriceText.text = "판매 가격 : " + price.ToString() + " 골드";
        }
    }
    public void ExitSellMode()
    {
        // 판매 텍스트 숨김
        if (sellPriceText != null)
            sellPriceText.gameObject.SetActive(false);

        // 상점 슬롯 다시 표시
        if (slotContainer != null)
            slotContainer.gameObject.SetActive(true);
    }

}
