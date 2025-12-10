using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrid : GridDivideBase
{
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
        for (int i = startNode; i < startNode + 3; ++i)
        {
            var node = nodePerInt[i];
            var pos = node.worldPosition;
            var obj = Instantiate(enemyPF);

            obj.GetComponent<Enemy>().SetPosition(pos);
        }
    }
    #endregion
}
