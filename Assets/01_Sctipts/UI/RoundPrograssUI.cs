using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundPrograssUI : MonoBehaviour
{
    [SerializeField] private RoundIconUI[] roundIcons;
    [SerializeField] private int maxRound = 5;

    private void Start()
    {
        var gm = GameManager.Instance;

        gm.OnRoundStarted += OnRoundStarted;
        gm.OnRoundEnded += OnRoundEnded;

        InitIcons();
    }

    private void InitIcons()
    {
        for (int i = 0; i < roundIcons.Length; i++)
        {
            roundIcons[i].SetState(RoundIconState.None);
        }
    }

    private void OnRoundStarted(int round)
    {
        int index = round - 1;
        if (index < 0 || index >= roundIcons.Length) return;

        roundIcons[index].SetState(RoundIconState.Current);
    }

    private void OnRoundEnded(int round, bool win)
    {
        int index = round - 1;
        if (index < 0 || index >= roundIcons.Length) return;

        roundIcons[index].SetState(
            win ? RoundIconState.Win : RoundIconState.Lose
        );
    }

    // 공개 초기화 메서드
    public void ResetUI()
    {
        InitIcons();
    }

}
