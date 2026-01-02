using UnityEngine;
using UnityEngine.UI;

public class StartPanelUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject startPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button exitButton;

    [Header("Option Panel")]
    [SerializeField] private GameObject optionPanel;

    private void Awake()
    {
        // 안전장치
        if (startPanel == null)
            startPanel = gameObject;

        // 시작 시 옵션 패널은 꺼둠
        if (optionPanel != null)
            optionPanel.SetActive(false);

        // 버튼 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(OnClickStart);

        if (optionButton != null)
            optionButton.onClick.AddListener(OnClickOption);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnClickExit);
    }

    /// <summary>
    /// 게임 시작 버튼
    /// 현재는 StartPanel만 닫는다.
    /// </summary>
    private void OnClickStart()
    {
        Close();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGameFromMainMenu();
        }
    }


    /// <summary>
    /// 옵션 버튼 (미구현, 일단은 게임의 간단한 설명과 팀원을 밝힘)
    /// </summary>
    private void OnClickOption()
    {
        if (optionPanel == null)
        {
            return;
        }

        bool isActive = optionPanel.activeSelf;
        optionPanel.SetActive(!isActive);
    }

    /// <summary>
    /// 게임 종료 버튼
    /// </summary>
    private void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void Open()
    {
        startPanel.SetActive(true);
    }

    public void Close()
    {
        startPanel.SetActive(false);
    }
}
