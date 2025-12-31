using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMTest : MonoBehaviour
{
    private int lastSec = -1;
    private void Start()
    {
        //Debug.Log("<color=yellow>=== GameManager 테스트 시작 ===</color>");
        //Debug.Log(GameManager.Instance == null ? "<color=red>GM NULL</color>" : "<color:green>GM FOUND</color>");
        // GameManager 이벤트 연결
        GameManager.Instance.OnRoundStarted += HandleRoundStarted;
        GameManager.Instance.OnPreparationTimerUpdated += HandlePreparationTimer;
        GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;

        // 실제 게임 시작
        //GameManager.Instance.StartGame(); 12-16 Won Edit To HotFix
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
        GameManager.Instance.OnPreparationTimerUpdated -= HandlePreparationTimer;
        GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
    }

    //===== 이벤트 콜백들 =====

    private void HandleRoundStarted(int round)
    {
        //Debug.Log($"<color=cyan>[RoundStart] 라운드 {round} 시작!</color>");
    }

    private void HandlePreparationTimer(float t)
    {
        int sec = Mathf.CeilToInt(t);

        if (sec == lastSec) return;
        lastSec = sec;

        if (sec % 5 == 0)
        {
            //Debug.Log($"[PrepTimer] 남은 준비시간: {sec}초");
        }
    }

    private void HandleRoundEnded(int round, bool win)
    {
        //Debug.Log($"<color=magenta>[RoundEnd] 라운드 {round} 종료! 결과 = {(win ? "승리" : "패배")}</color>");
    }

    private void HandleRoundStateChanged(RoundState newState)
    {
        ///Debug.Log($"<color=green>[StateChange] 새 라운드 상태: {newState}</color>");
    }
}
