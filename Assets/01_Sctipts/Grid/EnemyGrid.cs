using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrid : GridDivideBase
{
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

    #region - TEST FIELD -
    [SerializeField] GameObject enemyPF;
    [SerializeField] int startNode = 0;
    private void OnEnable()
    {
        SpawnEnemy();
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

            node.ChessPiece = obj.GetComponent<Enemy>(); //12.12 add Kim

        }
    }
    #endregion
}
