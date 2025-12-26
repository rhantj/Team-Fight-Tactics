using UnityEngine;


public class BGMStarter : MonoBehaviour
{
    private enum BGMState
    {
        None,
        Intro,
        Game,
        GameOver
    }

    private BGMState currentState = BGMState.None;

    [Header("References")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("BGM Keys")]
    [SerializeField] private string introBGMKey = "BGM_Intro";
    [SerializeField] private string gameBGMKey = "BGM1";
    [SerializeField] private string gameOverBGMKey = "BGM_GameOver";


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
