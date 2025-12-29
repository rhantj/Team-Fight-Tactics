using System.Collections.Generic;
using UnityEngine;

public class Enemy : Chess
{
    public Dictionary<string, List<float>> statPerRound;

    protected override void Awake()
    {
        base.Awake();
        team = Team.Enemy;
        if (baseData != null)
            baseData = Instantiate(baseData); //복사본쓰께끔햇어요

        string[] objName = gameObject.name.Split("(Clone)");

        statPerRound = CSVReader.BuildStatPerRound(objName[0]);
    }
    private void Start()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 180f);

        var statusUI = GetComponentInChildren<ChessStatusUI>(); //12.19 add Kim
        if (statusUI != null)
        {
            statusUI.Bind(this);
        }
        else
        {
        }

        SetStats(1);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStarted += SetStats;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStarted -= SetStats;
    }


    public void SetStats(int round)
    {
        InitFromSO();
        int idx = round - 1;

        baseData.maxHP = (int)statPerRound["maxHp"][idx];
        baseData.armor = (int)statPerRound["armor"][idx];
        baseData.attackDamage = (int)statPerRound["attackDamage"][idx];
        baseData.attackSpeed = statPerRound["attackSpeed"][idx];
        baseData.mana = (int)statPerRound["mana"][idx];

        //baseData.maxHP = (int)statPerRound["maxHp"].values[idx];
        //baseData.armor = (int)statPerRound["armor"].values[idx];
        //baseData.attackDamage = (int)statPerRound["attackDamage"].values[idx];
        //baseData.attackSpeed = statPerRound["attackSpeed"].values[idx];
        //baseData.mana = (int)statPerRound["mana"].values[idx];
    }
}
