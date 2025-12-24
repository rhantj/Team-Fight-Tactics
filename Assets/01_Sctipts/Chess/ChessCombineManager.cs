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

        if (!list.Contains(chess))
        {
            list.Add(chess);
            chess.OnUsedAsMaterial += HandleUsedAsMaterial;
            chess.OnDead += HandleDead; //사망하면 그룹에서 제외

            TryCombine(key); //등록이후 3개라면 즉시 조합
        }
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
        if (!chessGroups.TryGetValue(key, out var list))
            return;

        while (list.Count >= 3) //3개이상이라면 반복합성
        {
            Chess main = list[0];
            Chess material1 = list[1];
            Chess material2 = list[2];

            //ID비교입니다.
            string mainID = !string.IsNullOrEmpty(main.BaseData.poolID)
                ? main.BaseData.poolID
                : main.BaseData.unitName;
            string mat1ID = !string.IsNullOrEmpty(material1.BaseData.poolID)
                ? material1.BaseData.poolID
                : material1.BaseData.unitName;
            string mat2ID = !string.IsNullOrEmpty(material2.BaseData.poolID)
                ? material2.BaseData.poolID
                : material2.BaseData.unitName;

            if (mainID != mat1ID || mainID != mat2ID)
            {
                break;
            }

            if (main.StarLevel != material1.StarLevel ||
                main.StarLevel != material2.StarLevel)
            {
                break;
            }

            if (main.StarLevel >= 3)
                break;

            main.CombineWith(material1, material2); //본체 승급,재료 비활성화

            list.Remove(main);
            list.Remove(material1);
            list.Remove(material2);

            if (list.Count == 0)
            {
                chessGroups.Remove(key);
            }

            Register(main);//승급후 재등록을 통해서 추가합성이 가능하게.

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



}