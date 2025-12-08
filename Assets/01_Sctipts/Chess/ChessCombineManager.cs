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
    //        등록/취소
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

    public int GetRemainingPiecesForThreeStar(ChessStatData data)
    {
        if (data == null) return 9;

        if (completedUnits.Contains(data))
            return 0;

        int fragments = 0;

        foreach (var kvp in chessGroups)
        {
            var list = kvp.Value;
            foreach (var chess in list)
            {
                if (chess.BaseData != data) continue;
                switch (chess.StarLevel)
                {
                    case 1: fragments += 1; break;
                    case 2: fragments += 3; break;
                    case 3: fragments += 9; break;
                }
            }
        }

        int remaining = 9 - fragments;
        if (remaining < 0) remaining = 0;
        return remaining;
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
        return $"{chess.BaseData.unitName}_Star{chess.StarLevel}";
    }

    // =========================
    //         합성 로직
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
            if (main.BaseData != material1.BaseData || main.BaseData != material2.BaseData)
                break;
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
    //    이벤트 핸들러
    // =========================
    private void HandleUsedAsMaterial(Chess material)
    {
        if (material == null)
            return;

        Unregister(material);

        var pooled = material.GetComponentInParent<PooledObject>();
        if (pooled != null)
        {
            pooled.ReturnToPool();
        }
        else
        {
            Destroy(material.gameObject);
        }
    }

    private void HandleDead(Chess deadChess)
    {
        if (deadChess == null)
            return;

        Unregister(deadChess);
    }
}
