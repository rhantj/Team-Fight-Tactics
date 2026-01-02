using GLTFast.Schema;
using System.Collections.Generic;
using UnityEngine;

public class ChessCombineManager : MonoBehaviour
{
    public static ChessCombineManager Instance { get; private set; } //합성매니저 싱글톤 접근용
    private Dictionary<string, List<Chess>> chessGroups = new Dictionary<string, List<Chess>>();
    private HashSet<ChessStatData> completedUnits = new HashSet<ChessStatData>(); //완성된 기물은 재등장 못하게

    [SerializeField] FieldGrid mainField;
    [SerializeField] BenchGrid benchField;

    private void Awake()
    {
        if (Instance != null && Instance != this) //싱글톤 중복 방지.
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
    }

    private void HandleRoundStateChanged(RoundState state)
    {
        if (state == RoundState.Preparation)
            TryCombineAll();
    }

    private void TryCombineAll()
    {
        var keys = new List<string>(chessGroups.Keys);
        foreach (var k in keys)
            TryCombine(k);
    }


    public bool IsUnitCompleted(ChessStatData data)
    {
        //3성인지
        if (data == null) return false;
        return completedUnits.Contains(data);
    }

    private void MarkCompletedUnit(ChessStatData data)
    {
        //완성된 기물 기록
        if (data == null) return;
        completedUnits.Add(data);
    }

    // =========================
    //        등록 / 해제
    // =========================
    public void Register(Chess chess)
    {
        if (chess == null || chess.BaseData == null) 
            return;

        if (chess.team != Team.Player) return;

        if (chess.StarLevel >= 3) //3성 합성에서 제외
        {
            MarkCompletedUnit(chess.BaseData);
            return;
        }

        string key = GetKey(chess);
        if (!chessGroups.TryGetValue(key, out var list)) //동일유닛 성급을 묶기위함
        {
            list = new List<Chess>();
            chessGroups[key] = list;
        }
        if (list.Contains(chess)) return;

        if (!list.Contains(chess))
        {
            list.Add(chess);
            chess.OnUsedAsMaterial += HandleUsedAsMaterial;
            chess.OnDead += HandleDead; //사망하면 그룹에서 제외

            TryCombine(key); //등록이후 3개라면 즉시 조합
        }
        if (CanCombineNow())
            TryCombine(key);
    }

    public void Unregister(Chess chess)
    {
        if (chess == null || chess.BaseData == null)
            return;

        string key = GetKey(chess);
        if (chessGroups.TryGetValue(key, out var list))
        {
            list.Remove(chess);
            if (list.Count == 0)
                chessGroups.Remove(key);
        }


        //중복호출,누수방지
        chess.OnUsedAsMaterial -= HandleUsedAsMaterial;
        chess.OnDead -= HandleDead;
    }

    private string GetKey(Chess chess)
    {
        string uniqueID = !string.IsNullOrEmpty(chess.BaseData.poolID) //PoolID를 우선사용하고 없으면 unitName사용
            ? chess.BaseData.poolID
            : chess.BaseData.unitName;

        return $"{uniqueID}_Star{chess.StarLevel}"; //그룹화
    }

    // =========================
    //        합성 로직
    // =========================
    private void TryCombine(string key)
    {
        if (GameManager.Instance != null &&
        GameManager.Instance.roundState != RoundState.Preparation)
            return;

        if (!chessGroups.TryGetValue(key, out var list))
            return;

        while (list.Count >= 3)
        {
            Chess main = null;
            for (int i = 0; i < list.Count; i++)
            {
                if (IsPlacedOn(mainField, list[i]))
                {
                    main = list[i];
                    break;
                }
            }
            if (main == null) main = list[0];
            // 나머지 2개는 재료로
            Chess material1 = null;
            Chess material2 = null;
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c == main) continue;
                if (material1 == null) material1 = c;
                else { material2 = c; break; }
            }
            if (material1 == null || material2 == null) break;
            string mainID = !string.IsNullOrEmpty(main.BaseData.poolID) ? main.BaseData.poolID : main.BaseData.unitName;
            string mat1ID = !string.IsNullOrEmpty(material1.BaseData.poolID) ? material1.BaseData.poolID : material1.BaseData.unitName;
            string mat2ID = !string.IsNullOrEmpty(material2.BaseData.poolID) ? material2.BaseData.poolID : material2.BaseData.unitName;
            if (mainID != mat1ID || mainID != mat2ID) break;
            if (main.StarLevel != material1.StarLevel || main.StarLevel != material2.StarLevel) break;
            if (main.StarLevel >= 3) break;
            TransferItemsToMain(main, material1, material2);
            main.CombineWith(material1, material2);
            list.Remove(main);
            list.Remove(material1);
            list.Remove(material2);
            if (list.Count == 0) chessGroups.Remove(key);
            Register(main);
            if (!chessGroups.TryGetValue(key, out list))
                break;
        }

    }

    // =========================
    //     재료 / 사망 처리
    // =========================
    private void HandleUsedAsMaterial(Chess material)
    {
        if (material == null)
            return;

        Unregister(material);

        var pooled = material.GetComponentInParent<PooledObject>();

        ClearPiece(mainField, material);
        ClearPiece(benchField, material);

        if (pooled != null)
            pooled.ReturnToPool();
        else
            Destroy(material.gameObject);
    }

    void ClearPiece(GridDivideBase field, Chess material)
    {
        foreach (var n in field.FieldGrid)
        {
            if (n.ChessPiece == material)
                n.ChessPiece = null;
        }
    }

    private void HandleDead(Chess deadChess)
    {
        if (deadChess == null)
            return;

        Unregister(deadChess);
    }

    // 3성 판매시 다시 상점에 등장하게 설정
    public void UnmarkCompletedUnit(ChessStatData data)
    {
        if (data == null) return;
        completedUnits.Remove(data);
    }

    // 특정 유닛의 1성 환산 개수 계산 메서드
    public int GetOneStarEquivalentCount(ChessStatData data)
    {
        if (data == null) return 0;

        int count = 0;

        foreach (var pair in chessGroups)
        {
            foreach (var chess in pair.Value)
            {
                if (chess == null) continue;
                if (chess.BaseData != data) continue;

                switch (chess.StarLevel)
                {
                    case 1: count += 1; break;
                    case 2: count += 3; break;
                    case 3: count += 9; break;
                }
            }
        }

        return count;
    }

    // 2성 가능 여부
    public bool CanMake2Star(ChessStatData data)
    {
        int oneStarCount = GetOneStarEquivalentCount(data);

        // 3성 조건(8)은 제외
        if (oneStarCount == 8)
            return false;

        // 다음 1개를 샀을 때 3의 배수가 되는 경우
        return oneStarCount % 3 == 2;
    }


    // 3성 가능 여부
    public bool CanMake3Star(ChessStatData data)
    {
        int oneStarCount = GetOneStarEquivalentCount(data);

        // 정확히 8개일 때만
        return oneStarCount == 8;
    }

    private bool IsPlacedOn(GridDivideBase grid, Chess chess)
    {
        if (grid == null || chess == null) return false;

        foreach (var n in grid.FieldGrid)
            if (n.ChessPiece == chess)
                return true;

        return false;
    }

    private void TransferItemsToMain(Chess main, Chess mat1, Chess mat2)
    {
        if(main == null || mat1 == null || mat2 == null) return;

        var mainHandler = main.GetComponent<ChessItemHandler>();
        var mat1Handler = mat1.GetComponent<ChessItemHandler>();
        var mat2Handler = mat2.GetComponent<ChessItemHandler>();

        if (mainHandler == null) return;

        //세개의 기물의 아이템 종합
        var merged = new List<ItemData>();

        merged.AddRange(mainHandler.PopAllItemDatas());

        if (mat1Handler != null) merged.AddRange(mat1Handler.PopAllItemDatas());
        if (mat2Handler != null) merged.AddRange(mat2Handler.PopAllItemDatas());

        //메인에 다시 장착
        mainHandler.SetItemFromDatas(merged, out var overflow);

        //overflow처리
        if(overflow.Count >0)
        {
            //Debug.LogWarning("Item OverFlow");
        }

        ChessInfoUI.Instance?.RefreshItemUIOnly();

        var ui = main.GetComponentInChildren<ChessItemUI>();
        ui?.SyncFromHandler();
    }

    
    // 게임 재시작용 초기화 메서드 12-29 Won Add 
    
    public void ResetAll()
    {
        // 등록된 기물 이벤트 해제
        foreach (var pair in chessGroups)
        {
            foreach (var chess in pair.Value)
            {
                if (chess == null) continue;
                chess.OnUsedAsMaterial -= HandleUsedAsMaterial;
                chess.OnDead -= HandleDead;
            }
        }

        chessGroups.Clear();
        completedUnits.Clear();
    }

    private bool CanCombineNow()
    {
        return GameManager.Instance != null
            && GameManager.Instance.roundState == RoundState.Preparation;
    }

}