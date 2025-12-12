using System.Collections.Generic;
using UnityEngine;

public class ChessCombineManager : MonoBehaviour
{
    public static ChessCombineManager Instance { get; private set; }
    private Dictionary<string, List<Chess>> chessGroups = new Dictionary<string, List<Chess>>();
    private HashSet<ChessStatData> completedUnits = new HashSet<ChessStatData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsUnitCompleted(ChessStatData data)
    {
        if (data == null) return false;
        return completedUnits.Contains(data);
    }

    private void MarkCompletedUnit(ChessStatData data)
    {
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

        if (chess.StarLevel >= 3)
        {
            MarkCompletedUnit(chess.BaseData);
            return;
        }

        string key = GetKey(chess);
        if (!chessGroups.TryGetValue(key, out var list))
        {
            list = new List<Chess>();
            chessGroups[key] = list;
        }

        if (!list.Contains(chess))
        {
            list.Add(chess);
            chess.OnUsedAsMaterial += HandleUsedAsMaterial;
            chess.OnDead += HandleDead;

            TryCombine(key);
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

        chess.OnUsedAsMaterial -= HandleUsedAsMaterial;
        chess.OnDead -= HandleDead;
    }

    private string GetKey(Chess chess)
    {
        string uniqueID = !string.IsNullOrEmpty(chess.BaseData.poolID)
            ? chess.BaseData.poolID
            : chess.BaseData.unitName;

        return $"{uniqueID}_Star{chess.StarLevel}";
    }

    // =========================
    //        합성 로직
    // =========================
    private void TryCombine(string key)
    {
        if (!chessGroups.TryGetValue(key, out var list))
            return;

        while (list.Count >= 3)
        {
            Chess main = list[0];
            Chess material1 = list[1];
            Chess material2 = list[2];

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

            main.CombineWith(material1, material2);

            list.Remove(main);
            list.Remove(material1);
            list.Remove(material2);

            if (list.Count == 0)
            {
                chessGroups.Remove(key);
            }

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
        if (pooled != null)
            pooled.ReturnToPool();
        else
            Destroy(material.gameObject);
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

}