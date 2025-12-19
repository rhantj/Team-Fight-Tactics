using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCountManager : MonoBehaviour
{
    public static UnitCountManager Instance { get; private set; }
    
    public List<Chess> playerUnits = new List<Chess>();
    public List<Chess> enemyUnits = new List<Chess>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
    }

    public void Clear()
    {
        playerUnits.Clear();
        enemyUnits.Clear();
    }
    public void RegisterUnit(Chess unit, bool isPlayer)
    {
        if(isPlayer)
        {
            playerUnits.Add(unit);
        }
        else
        {
            enemyUnits.Add(unit);
        }

        unit.OnDead -= HandleUnitDead;
        unit.OnDead += HandleUnitDead;
    }

    private void HandleUnitDead(Chess unit)
    {
        if(playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);
        }
        else if(enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);
        }
    }

    public int GetPlayerAliveCount() => playerUnits.Count;
    public int GetEnemyAliveCount() => enemyUnits.Count;

    public bool ArePlayerAllDead() => playerUnits.Count == 0;
    public bool AreEnemyAllDead() => enemyUnits.Count == 0;
}
