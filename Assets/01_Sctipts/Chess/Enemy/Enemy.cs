using System.Collections.Generic;
using UnityEngine;

public class Enemy : Chess
{
    public StringFloatListDict statPerRound;
    bool dirty;

    protected override void Awake()
    {
        if (baseData != null)
            baseData = Instantiate(baseData); //복사본쓰께끔햇어요
        base.Awake();
        team = Team.Enemy;

        string[] objName = gameObject.name.Split("(Clone)");
        statPerRound = CSVReader.BuildStatPerRoundSD(objName[0]);
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 180f);

        var statusUI = GetComponentInChildren<ChessStatusUI>(); //12.19 add Kim
        if (statusUI != null)
        {
            statusUI.Bind(this);
        }

        dirty = true;
    }

    private void LateUpdate()
    {
        if (!dirty) return;
        dirty = false;
        CalculateStats(GameManager.Instance.currentRound);
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
        dirty = true;
    }

    void CalculateStats(int round)
    {
        int idx = round - 1;

        baseData.maxHP = (int)statPerRound["maxHp"].values[idx];
        baseData.armor = (int)statPerRound["armor"].values[idx];
        baseData.attackDamage = (int)statPerRound["attackDamage"].values[idx];
        baseData.attackSpeed = statPerRound["attackSpeed"].values[idx];
        baseData.mana = (int)statPerRound["mana"].values[idx];
        InitFromSO();
    }
}
