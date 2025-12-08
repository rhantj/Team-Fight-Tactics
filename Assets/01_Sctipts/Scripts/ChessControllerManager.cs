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
        switch(state)
        {
            case RoundState.Preparation:
                foreach(var chess in allChess)
                {
                    chess.SetTarget(null);
                    chess.ForceIdle();
                }
                break;
            case RoundState.Battle:
                foreach(var chess in allChess)
                {
                    chess.ForceBattle();
                    //전투 타겟 자동 설정 로직 구현해야함
                }
                break;
            case RoundState.Result:
                foreach(var chess in allChess)
                {
                    chess.ForceIdle();
                }
                break;
        }
    }
    
    private void OnPreparationTimerUpdated(float time)
    {
        //기물 준비 애니메이션/이펙트
    }
}
