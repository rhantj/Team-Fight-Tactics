using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Round Info")]
    [SerializeField] private TMP_Text survivedRoundText;

    [Header("Star Sprites")]
    [SerializeField] private Sprite silverStarSprite;
    [SerializeField] private Sprite goldStarSprite;

    private readonly List<Image> spawnedPortraits = new();

    // 버튼 할당
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
        RefreshRoundText();
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

    // 초상화 생성
    private void CreatePortrait(EndGameUnitSnapshot data)
    {
        var portrait = Instantiate(chessPortraitPrefab, chessPortraitList);
        portrait.sprite = data.portrait;
        portrait.gameObject.SetActive(true);

        // ★ StarImage는 기본적으로 꺼둔다
        var starTf = portrait.transform.Find("StarImage");
        if (starTf != null)
        {
            starTf.gameObject.SetActive(false);

            // 2성 / 3성만 표시
            if (data.starLevel >= 2)
            {
                var starImg = starTf.GetComponent<Image>();
                if (starImg != null)
                {
                    starImg.sprite = data.starLevel == 3
                        ? goldStarSprite
                        : silverStarSprite;

                    starTf.gameObject.SetActive(true);
                }
            }
        }

        spawnedPortraits.Add(portrait);
    }


    // 초상화 비우기
    private void ClearPortraits()
    {
        foreach (var img in spawnedPortraits)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        spawnedPortraits.Clear();
    }

    private void RefreshRoundText()
    {
        if (survivedRoundText == null)
            return;

        if (GameManager.Instance == null)
            return;

        int round = GameManager.Instance.LastReachedRound;

        survivedRoundText.text = $"{round} 라운드까지 생존!!";
        // 또는
        // survivedRoundText.text = $"{round} 라운드까지 생존";
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    // 게임종료 이벤트 구독 해제
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }
}
