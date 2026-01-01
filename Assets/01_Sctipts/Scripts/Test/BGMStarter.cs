using UnityEngine;

/// <summary>
/// BGMStarter는 GameState를 직접 참조하지 않고
/// UI 패널 활성 상태를 기준으로 현재 게임 흐름을 판단한다.
/// (프로젝트 규모와 구조에 맞춘 단순한 상태 감시 방식)
/// </summary>
public class BGMStarter : MonoBehaviour
{
    // BGM 전환시 중복 재생을 막기 위해 내부 상태Enum 추가
    private enum BGMState
    {
        None,    //0
        Intro,   //1
        Game,    //2
        GameOver //3
    }

    private BGMState currentState = BGMState.None;

    [Header("References")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("BGM Keys")]
    [SerializeField] private string introBGMKey = "BGM_Intro";
    [SerializeField] private string gameBGMKey = "BGM1";
    [SerializeField] private string gameOverBGMKey = "BGM_GameOver";
    // SFX매니저에 Preload되어있는 음악파일의 이름을 동일하게 적어두면 해당 음악 재생

    private void Start()
    {
        // 시작 시 상태에 맞는 BGM 재생
        UpdateBGM();
    }

    private void Update()
    {
        // 패널들 상태 변화 감지
        UpdateBGM();

    }

    // 브금 업데이트
    private void UpdateBGM()
    {
        if (startPanel == null) return;

        // 1. GameOver 최우선
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            ChangeBGM(BGMState.GameOver);
            return;
        }

        // 2. StartPanel
        if (startPanel.activeSelf)
        {
            ChangeBGM(BGMState.Intro);
        }
        // 3. In Game
        else
        {
            ChangeBGM(BGMState.Game);
        }
    }

    // 브금 변경 메서드
    private void ChangeBGM(BGMState next)
    {
        if (currentState == next) return;
        currentState = next;

        switch (currentState)
        {
            case BGMState.Intro:
                SettingsUI.PlayBGM(introBGMKey);
                break;

            case BGMState.Game:
                SettingsUI.PlayBGM(gameBGMKey);
                break;

            case BGMState.GameOver:
                SettingsUI.PlayBGM(gameOverBGMKey);
                break;
        }
    }



}
