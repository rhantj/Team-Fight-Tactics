using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrid : GridDivideBase
{
    [SerializeField] GameObject[] enemyPF;
    int[] enemyIdxs;
    int startNode = 10;
    int offset = 2;
    public List<ChessStateBase> allFieldUnits = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        StaticRegistry<EnemyGrid>.Add(this);
    }

    private void Start()
    {
        SpawnEnemy(1);
    }

    // 필드 위의 전체 기물 리스트
    public List<ChessStateBase> GetAllFieldUnits()
    {
        List<ChessStateBase> result = new();

        foreach (var node in fieldGrid)
        {
            if (node.ChessPiece != null)
            {
                result.Add(node.ChessPiece);
            }
        }
        return result;
    }

    public void ResetAllNode()
    {
        if (fieldGrid != null)
        {
            var fieldUnits = GetAllFieldUnits();

            foreach (var unit in fieldUnits)
            {
                if (unit == null) continue;

                // 노드 참조 제거 (CountOfPiece 자동 감소)
                ClearChessPiece(unit);

                // 풀 반환
                var pooled = unit.GetComponentInParent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    unit.gameObject.SetActive(false);
            }
        }
    }

    public void SpawnEnemy(int round)
    {
        ResetAllNode();
        EnemiesIndex(round);

        foreach (int idx in enemyIdxs)
        {
            var node = nodePerInt[startNode];
            var pos = node.worldPosition;
            var objName = enemyPF[idx].name;
            var obj = PoolManager.Instance.Spawn(objName);

            //===== add Kim 12.19
            var enemy = obj.GetComponent<Enemy>();
            enemy.SetPosition(pos);
            enemy.SetOnField(true);
            //=====

            node.ChessPiece = enemy;//12.12 add Kim

            startNode += offset;
        }

        startNode -= offset * enemyIdxs.Length;
    }

    void EnemiesIndex(int round)
    {
        switch (round)
        {
            case 1:
                enemyIdxs = new int[] { 0 };
                break;
            case 2:
                enemyIdxs = new int[] { 1 };
                break;
            case 3:
                enemyIdxs = new int[] { 2 };
                break;
            case 4:
                enemyIdxs = new int[] { 0, 1, 2 };
                break;
            case 5:
                enemyIdxs = new int[] { 2, 3, 2 };
                break;
        }
    }
}
