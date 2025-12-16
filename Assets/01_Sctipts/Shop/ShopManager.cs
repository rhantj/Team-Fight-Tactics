using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 상점 시스템 전체를 제어하는 파사드(Facade) 매니저
/// 
/// 상점과 관련된 여러 하위 시스템(UI, 확률, 골드, 레벨, 풀링, 합성)을
/// 하나의 진입점으로 묶어서 외부에서는 ShopManager만을 통해
/// 상점 기능을 사용하도록 설계했습니다.
/// 
/// 이 클래스는 의도적으로 책임 범위가 넓으며,
/// "상점 시스템이 변경될 때"라는 단일 이유만으로 수정되는 것을 목표로 합니다.
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

    /// <summary>
    /// 상점 슬롯 UI 배열
    /// slotContainer 하위의 ShopSlot을 자동으로 탐색하여 설정됩니다.
    /// </summary>
    private ShopSlot[] slots;

    /// <summary>
    /// 코스트 기준으로 분류된 기물 데이터 캐시.
    /// 랜덤 유닛 선택 시 빠른 조회를 위해 사용됩니다.
    /// </summary>
    private Dictionary<int, List<ChessStatData>> unitsByCost;

    /// <summary>
    /// 상점 등장 제한을 위한 내부 카운트 데이터
    /// 특정 기물이 과도하게 등장하지 않도록 제어하는 용도이며.
    /// 판매 시에는 성급에 따라 복구됩니다.
    /// </summary>
    private Dictionary<ChessStatData, int> unitBuyCount = new Dictionary<ChessStatData, int>();

    private int rewardGold = 5;

    // ================================================================
    // 샵 잠금 시스템
    // ================================================================
    /// <summary>
    /// 상점 잠금 시스템 관련 데이터
    /// 잠금 상태에서는 Refresh 및 Reroll이 제한됩니다.
    /// </summary>
    [Header("Shop Lock System")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private UnityEngine.UI.Image lockIconImage;
    [SerializeField] private Sprite lockedSprite;
    private Sprite defaultUnlockedSprite;

    // ================================================================
    // 초기화
    // ================================================================
    protected override void Awake()
    {
        base.Awake();

        // 슬롯 자동 탐색
        slots = slotContainer.GetComponentsInChildren<ShopSlot>();

        // 잠금 아이콘 기본 상태 저장
        if (lockIconImage != null)
            defaultUnlockedSprite = lockIconImage.sprite;

        // 코스트 기준 기물을 분류
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
    /// <summary>
    /// 상점 잠금 상태를 토글합니다.
    /// 잠금 상태에서는 상점 갱신 및 리롤이 제한됩니다.
    /// </summary>
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
    /// <summary>
    /// 현재 골드UI를 갱신합니다.
    /// </summary>
    private void UpdateGoldUI()
    {
        if (currentGoldText != null)
            currentGoldText.text = currentGold.ToString();
    }

    /// <summary>
    /// 골드 소비를 시도합니다.
    /// 소비 가능할 경우 골드를 차감하고 true를 반환합니다.
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
    /// 골드를 추가하고 UI를 갱신합니다.
    /// </summary>
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    // ================================================================
    // EXP & 레벨업
    // ================================================================
    /// <summary>
    /// 경험치 구매 버튼 처리
    /// 골드를 소모하여 경험치를 획득하고 레벨업 여부를 검사합니다.
    /// </summary>
    public void BuyExp()
    {
        if (!TrySpendGold(4))
            return;

        AddExp(4);
    }

    public void AddExp(int exp)
    {
        playerExp += exp;
        UpdateExpUI();
        CheckLevelUp();
    }
    /// <summary>
    /// 현재 경험치를 기준으로 레벨업을 반복 검사합니다.
    /// </summary>
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

    /// <summary>
    /// 현재 플레이어 레벨 UI를 갱신합니다.
    /// 레벨업 발생 시 및 초기 UI 설정 단계에서 호출됩니다.
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = "Lv. " + playerLevel;
    }

    /// <summary>
    /// 현재 플레이어 경험치UI를 갱신합니다.
    /// 현재 레벨의 요구 경험치(LevelData)를 기준으로
    /// EXP 진행 상황을 텍스트로 표시합니다.
    /// </summary>
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
    /// <summary>
    /// 상점 슬롯을 새로 갱신합니다.
    /// 잠금 상태일 경우 실행되지 않습니다.
    /// </summary>
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
        }
    }

    /// <summary>
    /// 지정된 코스트 범위 내에서 실제 등장 가능한 유닛을 랜덤으로 선택합니다.
    /// 풀 잔여 수량, 합성 완료 여부, 등장 제한 카운트를 모두 고려합니다.
    /// </summary>
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
    /// <summary>
    /// 기물의 코스트와 성급을 기준으로 판매 가격을 계산합니다.
    /// </summary>
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

    /// <summary>
    /// 상점 슬롯에서 기물을 구매하는 트랜잭션 처리 메서드.
    /// </summary>
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

    /// <summary>
    /// 기물을 판매하는 트랜잭션 처리 메서드.
    /// </summary>
    public void SellUnit(ChessStatData data, GameObject obj)
    {
        if (data == null || obj == null)
            return;

        // 판매되는 유닛의 Chess 컴포넌트
        Chess chess = obj.GetComponentInChildren<Chess>();

        // 성급 판별
        int starLevel = 1;
        if (chess != null)
            starLevel = chess.StarLevel;

        // 판매 가격 계산 및 골드 지급
        int sellPrice = CalculateSellPrice(data, starLevel);
        AddGold(sellPrice);

        Debug.Log($"{data.unitName} 판매 완료. +{sellPrice} Gold");

        // ===============================
        // 1. CombineManager 정리
        // ===============================
        if (chess != null)
        {
            ChessCombineManager.Instance?.Unregister(chess);

            // 3성 유닛 판매 시 completed 상태 해제
            if (starLevel >= 3)
            {
                ChessCombineManager.Instance?.UnmarkCompletedUnit(data);
            }
        }

        // ===============================
        // 2. Shop 등장 제한용 구매 카운트 복구
        // ===============================
        if (unitBuyCount.ContainsKey(data))
        {
            int returnCount = 1;

            if (starLevel == 2) returnCount = 3;
            else if (starLevel == 3) returnCount = 9;

            unitBuyCount[data] = Mathf.Max(0, unitBuyCount[data] - returnCount);
        }

        // ===============================
        // 3. 풀로 반환
        // ===============================
        PoolManager.Instance.Despawn(data.poolID, obj);
    }




    // ================================================================
    // 확률 / 유닛 생성
    // ================================================================
    /// <summary>
    /// 현재 레벨의 등장 확률을 기준으로 랜덤 코스트를 반환합니다.
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
        float sum = 0;

        foreach (var r in data.rates)
        {
            sum += r.rate;
            if (rand <= sum)
                return r.cost;
        }

        return data.rates[0].cost;
    }

    /// <summary>
    /// 레벨 데이터 테이블에서 특정 레벨 데이터를 조회합니다.
    /// </summary>
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

    /// <summary>
    /// 샵매니저가 게임매니저의 이벤트를 구독과 해제합니다.
    /// </summary>
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;
            GameManager.Instance.OnRoundReward += HandleRoundReward;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
            GameManager.Instance.OnRoundReward -= HandleRoundReward;
        }
    }

    /// <summary>
    /// GameManager의 라운드 상태 변경 이벤트 콜백.
    /// 준비단계 진입 시 상점을 자동으로 갱신합니다.
    /// </summary>
    private void HandleRoundStateChanged(RoundState newState)
    {
        if (newState == RoundState.Preparation)
        {
            RefreshShop();
        }
    }

    private void HandleRoundReward(int round, bool isWin)
    {
        int rewardExp = 2;

        // 현재 보유 골드 기준 이자 계산
        int interestGold = CalculateInterestGold(currentGold); // ← 보유 골드 변수

        // 이번 라운드 최종 골드 보상
        int totalGoldReward = rewardGold + interestGold;

        // 지급
        AddGold(totalGoldReward);
        AddExp(rewardExp);

        Debug.Log(
            $"[RoundReward] Round {round} | Win={isWin} | " +
            $"BaseReward={rewardGold}, Interest={interestGold}, TotalGold={totalGoldReward}"
        );

        // 다음 라운드를 위한 연승 보상 갱신
        if (isWin)
        {
            rewardGold += 1;
        }
        else
        {
            rewardGold = 5;
        }
    }


    /// <summary>
    /// 이자 계산하는 메서드
    /// </summary>
    /// <param name="currentGold"></param>
    /// <returns></returns>
    private int CalculateInterestGold(int currentGold)
    {
        int interest = currentGold / 10;
        return Mathf.Min(interest, 5); //10원마다 나뉘어 떨어지며 상한 이자골드는 5원
    }



}
