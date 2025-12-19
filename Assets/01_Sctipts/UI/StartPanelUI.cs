using UnityEngine;
using UnityEngine.UI;

public class StartPanelUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject startPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;

    private void Awake()
    {
        // 안전장치
        if (startPanel == null)
            startPanel = gameObject;

        // 버튼 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(OnClickStart);

        if (optionButton != null)
            optionButton.onClick.AddListener(OnClickOption);
    }

    /// <summary>
    /// 게임 시작 버튼
    /// 현재는 StartPanel만 닫는다.
    /// </summary>
    private void OnClickStart()
    {
        Close();

        // 나중에 연결할 자리
        // GameManager.Instance?.StartGame();
    }

    /// <summary>
    /// 옵션 버튼 (미구현)
    /// </summary>
    private void OnClickOption()
    {
        Debug.Log("[StartPanel] Option button clicked (TODO)");
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
