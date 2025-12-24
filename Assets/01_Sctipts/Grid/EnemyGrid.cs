using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrid : GridDivideBase, IPrepare
{
    [SerializeField] GameObject enemyPF;
    [SerializeField] int startNode = 0;
    public List<ChessStateBase> allFieldUnits = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        SpawnEnemy();

        GameManager.Instance.OnRoundEnded += PrepareChessPieces;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoundEnded -= PrepareChessPieces;
    }

    public void PrepareChessPieces(int arg1, bool arg2)
    {
        allFieldUnits.Clear();
        allFieldUnits = GetAllFieldUnits();

        foreach (var node in FieldGrid)
        {
            var piece = node.ChessPiece;
            if (piece)
            {
                piece.InitOnPrepare();
                piece.SetPosition(node.worldPosition);
                piece.gameObject.SetActive(true);
            }
        }
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

    void SpawnEnemy()
    {
        for (int i = startNode; i < startNode + 1; ++i)
        {
            var node = nodePerInt[i];
            var pos = node.worldPosition;
            var obj = Instantiate(enemyPF);

            //obj.GetComponent<Enemy>().SetPosition(pos);
            //===== add Kim 12.19
            var enemy = obj.GetComponent<Enemy>();
            enemy.SetPosition(pos);
            enemy.SetOnField(true);
            //=====

            node.ChessPiece = enemy; //12.12 add Kim

        }
    }
}
