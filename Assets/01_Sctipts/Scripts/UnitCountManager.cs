using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }
    
    public List<Chess> playerUnits = new List<Chess>();
    public List<Chess> enemyUnits = new List<Chess>();

    private void Awake()
    {
        Instance = this;
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

        unit.OnDead += HandleUnitDead;
    }

    private void HandleUnitDead(Chess unit)
    {
        if(playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);
        }
        //else if(enemyUnits.Contains(unit)
        //    {

        //}
    }
}
