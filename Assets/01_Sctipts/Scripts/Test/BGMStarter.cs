using UnityEngine;

public class BGMStarter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject startPanel;

    [Header("BGM Keys")]
    [SerializeField] private string introBGMKey = "BGM_Intro";
    [SerializeField] private string gameBGMKey = "BGM1";

    private bool isIntroPlaying = false;

    private void Start()
    {
        // 시작 시 StartPanel 상태에 맞는 BGM 재생
        UpdateBGM();
    }

    private void Update()
    {
        // StartPanel 활성 상태 변화 감지
        UpdateBGM();

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Over UI? {UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()}");
        }

    }

    private void UpdateBGM()
    {
        if (startPanel == null) return;

        // 시작 패널이 켜져 있고, 아직 인트로 BGM이 아니라면
        if (startPanel.activeSelf && !isIntroPlaying)
        {
            SettingsUI.PlayBGM(introBGMKey, 1f);
            isIntroPlaying = true;
        }
        // 시작 패널이 꺼졌고, 게임 BGM이 아니라면
        else if (!startPanel.activeSelf && isIntroPlaying)
        {
            SettingsUI.PlayBGM(gameBGMKey, 1f);
            isIntroPlaying = false;
        }
    }
}
