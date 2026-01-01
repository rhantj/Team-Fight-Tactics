using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessControllerManager : MonoBehaviour
{
    private List<Chess> allChess = new List<Chess>();

    private void Start()
    {
        allChess.AddRange(FindObjectsOfType<Chess>());

        GameManager.Instance.OnRoundStateChanged += OnRoundStateChanged;
        GameManager.Instance.OnPreparationTimerUpdated += OnPreparationTimerUpdated;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundStateChanged -= OnRoundStateChanged;
        GameManager.Instance.OnPreparationTimerUpdated -= OnPreparationTimerUpdated;
    }

    private void OnRoundStateChanged(RoundState state)
    {
        RefreshChessList();

        foreach (var chess in allChess)
        {


            if (chess.GetComponent<Enemy>() != null) continue; //12.11 Kim add

            switch (state)
            {
                case RoundState.Preparation:
                case RoundState.Result:
                    chess.overrideState = true;
                    break;

                case RoundState.Battle:
                    chess.overrideState = false; //12.22 Kim add : true -> false로 바꿨습니다
                    chess.ForceBattle();
                    break;
            }
        }
    }
    
    private void OnPreparationTimerUpdated(float time)
    {
        //기물 준비 애니메이션/이펙트
    }
    private void RefreshChessList()
    {
        allChess.Clear();
        allChess.AddRange(FindObjectsOfType<Chess>());
    }
}
