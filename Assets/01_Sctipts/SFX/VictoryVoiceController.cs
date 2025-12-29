using System.Collections.Generic;
using UnityEngine;

public class VictoryVoiceController : MonoBehaviour
{
    private bool playedThisRound = false;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundEnded += HandleRoundEnded;
        GameManager.Instance.OnRoundStarted += OnRoundStarted;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
        GameManager.Instance.OnRoundStarted -= OnRoundStarted;
    }

    private void HandleRoundEnded(int round, bool win)
    {
        if (!win) return;
        if (playedThisRound) return;

        PlayVictoryVoice();
        playedThisRound = true;
    }

    private void PlayVictoryVoice()
    {
        var fieldGrid = StaticRegistry<FieldGrid>.Find();
        if (fieldGrid == null) return;

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
