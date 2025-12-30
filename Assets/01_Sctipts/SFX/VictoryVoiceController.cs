using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 라운드 승리 시점에 아직 생존해 있는 플레이어 기물 중 하나를
/// 무작위로 선택해 승리 음성을 한 번만 재생하는 연출 전용 컨트롤러
/// </summary>
public class VictoryVoiceController : MonoBehaviour
{
    // 같은 라운드에서 중복 재생 방지 bool변수
    private bool playedThisRound = false;

    // 라운드 시작&종료 이벤트 구독 해제
    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        GameManager.Instance.OnRoundStarted += OnRoundStarted;
    }

    // 이벤트 누수 방지
    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        GameManager.Instance.OnRoundStarted -= OnRoundStarted;
    }

    // 진입 조건 제어
    private void HandleRoundEnded(int round, bool win)
    {
        // 라운드 종료시 호출하며 승리한 경우만 처리
        if (!win) return;
        if (playedThisRound) return;

        PlayVictoryVoice();
        playedThisRound = true;
    }

    private void PlayVictoryVoice()
    {
        // 레지스트리 기반 탐색
        // 필드 유닛 목록은 FieldGrid가 단일 책임을 가지므로
        // 스태틱레지스트리를 통해 간접 접근
        var fieldGrid = StaticRegistry<FieldGrid>.Find();
        if (fieldGrid == null) return;

        // 살아있는 애들만 뽑아서 리스트에 저장
        List<Chess> alive = new();

        foreach (var unit in fieldGrid.GetAllFieldUnits())
        {
            var chess = unit.GetComponent<Chess>();
            if (chess == null) continue;
            if (chess.team != Team.Player) continue;
            if (chess.IsDead) continue;

            alive.Add(chess);
        }

        if (alive.Count == 0) return;

        // 리스트에서 랜덤으로 뽑고
        var pick = alive[Random.Range(0, alive.Count)];
        var clip = pick.BaseData.victoryVoice;

        if (clip == null) return;

        SettingsUI.PlaySFX(clip, pick.transform.position, 1f); // spatialBlend는 기본값 1f 사용


    }

    // 다음 라운드 대비 리셋
    private void OnRoundStarted(int round)
    {
        playedThisRound = false;
    }
}
