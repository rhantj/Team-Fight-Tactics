using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Chess Portrait")]
    [SerializeField] private Transform chessPortraitList;   // ChessPortraitList
    [SerializeField] private Image chessPortraitPrefab;     // ChessPortrait (Image)

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;

    private readonly List<Image> spawnedPortraits = new();

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        panelRoot.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnClickRetry);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnClickMainMenu);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnClickExit);
    }

    // 게임종료 이벤트 구독
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += Show;
    }


    // ==============================
    // Public API
    // ==============================

    /// <summary>
    /// 게임 종료 패널 표시 + 마지막 필드 유닛 초상화 표시
    /// </summary>
    public void Show()
    {
        panelRoot.SetActive(true);
        RefreshChessPortraits();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        ClearPortraits();
    }

    // ==============================
    // Portrait Logic
    // ==============================

    private void RefreshChessPortraits()
    {
        ClearPortraits();

        if (GameManager.Instance == null)
            return;

        var snapshots = GameManager.Instance.LastBattleUnits;
        foreach (var data in snapshots)
        {
            CreatePortrait(data);
        }
    }


    private void CreatePortrait(EndGameUnitSnapshot data)
    {
        var portrait = Instantiate(chessPortraitPrefab, chessPortraitList);

        portrait.sprite = data.portrait;
        portrait.gameObject.SetActive(true);

        spawnedPortraits.Add(portrait);
    }


    private void ClearPortraits()
    {
        foreach (var img in spawnedPortraits)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        spawnedPortraits.Clear();
    }

    // ==============================
    // Button Callbacks
    // ==============================

    private void OnClickRetry()
    {
        Hide();
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.RestartGame();
        GameManager.Instance.StartGameFromMainMenu();
    }

    private void OnClickMainMenu()
    {
        Hide();
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void OnClickExit()
    {
        Application.Quit();
    }

    // 게임종료 이벤트 구독 해제
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }
}
