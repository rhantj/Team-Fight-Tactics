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

        var fieldGrid = FindAnyObjectByType<FieldGrid>();
        if (fieldGrid == null)
        {
            Debug.LogWarning("[GameOverPanelUI] FieldGrid not found.");
            return;
        }

        var fieldUnits = fieldGrid.GetAllFieldUnits();
        foreach (var unit in fieldUnits)
        {
            var chess = unit.GetComponent<Chess>();
            if (chess == null) continue;

            CreatePortrait(chess);
        }
    }

    private void CreatePortrait(Chess chess)
    {
        var portrait = Instantiate(chessPortraitPrefab, chessPortraitList);
        portrait.gameObject.SetActive(true);

        // 여기서 마지막 라운드 필드 위에 배치된 애들 정보만 가져와서
        // 초상화 배치하기
        
        

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
        GameManager.Instance?.StartGame();
    }

    private void OnClickMainMenu()
    {
        Hide();
        // 메인메뉴로 가는 로직 추가하기
        // 게임매니저에서 만들면 좋을듯
    }

    private void OnClickExit()
    {
        Application.Quit();
    }
}
