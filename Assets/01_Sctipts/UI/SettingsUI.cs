using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameBGMData
{
    public string key;          // 실제 재생 키 (preloadSFX 이름)
    public string displayName;  // UI에 보여줄 이름
}

/// <summary>
/// 게임 내 설정(옵션) UI를 담당하는 컨트롤러.
///
/// - 설정 패널 열기 / 닫기
/// - 배경음(BGM), 효과음(SFX) 전역 볼륨 설정
/// - ESC 키 입력 처리
/// - 게임 일시정지(Time.timeScale) 제어
/// - 사운드 재생을 위한 전역 래퍼 메서드 제공
///
/// 실제 사운드 재생은 SoundSystem.SoundPlayer에 위임하며,
/// 이 클래스는 "UI 입력 + 전역 볼륨 관리" 역할만 담당한다.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    //전역 사운드 옵션 (static)
    public static float BGMVolume = 1f; //0~1
    public static float SFXVolume = 1f; //0~1

    // 현재 선택된 게임 BGM Key
    public static string SelectedGameBGMKey = "BGM1";

    [Header("UI Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Volume Value Text")]
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    // ==============================
    // Game BGM Select UI
    // ==============================

    [Header("Game BGM Select")]
    [SerializeField] private TMP_Text currentBGMText;      // 현재 선택된 BGM 이름 표시
    [SerializeField] private Button currentBGMButton;     // 클릭 시 목록 토글
    [SerializeField] private GameObject bgmListPanel;      // BGM 목록 패널
    [SerializeField] private Button bgmButtonPrefab;       // BGM 선택 버튼 프리팹
    [SerializeField] private Transform bgmButtonParent;    // 버튼 생성 부모

    [Header("BGM List")]
    [SerializeField] private GameBGMData[] gameBGMs;       // 선택 가능한 BGM 목록

    [Header("Initial Volume")]
    [SerializeField, Range(0f, 1f)]
    private float initialBGMVolume = 0.5f;

    //설정창 열리는 불변수
    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Start()
    {
        // 여기서 설정창 꺼두고
        settingsPanel.SetActive(false);

        // 전역 볼륨 초기화
        BGMVolume = initialBGMVolume;

        // UI 초기화
        bgmSlider.value = BGMVolume * 100f;
        sfxSlider.value = SFXVolume * 100f;

        bgmValueText.text = ((int)bgmSlider.value).ToString();
        sfxValueText.text = ((int)sfxSlider.value).ToString();

        // 실제 사운드에도 반영
        SoundSystem.SoundPlayer?.SetBGMVolume(BGMVolume);

        // BGM 초기 세팅
        InitializeGameBGM();
    }


    void Update()
    {
        //ESC 누르면 설정창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsUI();
        }
    }

    // 기존의 토글함수는 입력 전용 래퍼메서드로만 유지
    public void ToggleSettingsUI()
    {
        if (isOpen)
            Hide();
        else
            Show();
    }


    // 설정창 여닫는 함수 추가

    public void Show()
    {
        if (isOpen) return;

        isOpen = true;
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (!isOpen) return;

        isOpen = false;
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }


    //배경음 조절 함수
    public void OnChangeBGM(float value)
    {
        BGMVolume = value / 100f; //0~1로 변환
        bgmValueText.text = ((int)value).ToString();

        // 실시간 반영
        SoundSystem.SoundPlayer?.SetBGMVolume(BGMVolume);
    }

    //효과음 조절 함수
    public void OnChangeSFX(float value)
    {
        SFXVolume = value / 100f; //0~1로 변환
        sfxValueText.text = ((int)value).ToString();

        SoundSystem.SoundPlayer?.SetSFXVolume(SFXVolume);
    }

    //설정창 닫는 함수
    public void OnClickClose()
    {
        //Debug.Log("CLOSE CLICKED");
        ToggleSettingsUI();
    }

    //게임 종료 함수
    public void OnClickExitGame() => Application.Quit();

    //메인 메뉴로 돌아가는 함수
    public void OnClickReturnToMainMenu()
    {
        if (GameManager.Instance == null)
        {
            //Debug.LogError("[SettingsUI] GameManager.Instance is null");
            return;
        }

        GameManager.Instance.ReturnToMainMenu();
    }

    // ==============================
    // Game BGM Logic
    // ==============================

    // 초기 Game BGM 세팅
    private void InitializeGameBGM()
    {
        if (gameBGMs == null || gameBGMs.Length == 0)
            return;

        // 기본값 보정
        SelectedGameBGMKey = gameBGMs[0].key;
        currentBGMText.text = gameBGMs[0].displayName;

        CreateBGMButtons();
    }

    // 현재 BGM 버튼 클릭 → 목록 토글
    public void OnClickCurrentBGM()
    {
        if (bgmListPanel == null)
            return;

        bgmListPanel.SetActive(!bgmListPanel.activeSelf);
    }

    // BGM 선택 버튼 생성
    private void CreateBGMButtons()
    {
        foreach (Transform child in bgmButtonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var bgm in gameBGMs)
        {
            var btn = Instantiate(bgmButtonPrefab, bgmButtonParent);
            btn.GetComponentInChildren<TMP_Text>().text = bgm.displayName;

            btn.onClick.AddListener(() =>
            {
                SelectGameBGM(bgm);
            });
        }
    }

    // BGM 선택 시 즉시 교체
    private void SelectGameBGM(GameBGMData bgm)
    {
        SelectedGameBGMKey = bgm.key;
        currentBGMText.text = bgm.displayName;

        // 기존 BGM 중단 + 새 BGM 즉시 재생
        PlayBGM(SelectedGameBGMKey, 0.5f);

        if (bgmListPanel != null)
            bgmListPanel.SetActive(false);
    }

    // ==============================
    // Sound Wrapper
    // ==============================

    //사운드 재생 래퍼(Wrapper) 함수
    public static void PlaySFX(string clipName, Vector3 pos, float volume = 1f, float spatialBlend = 1f)
    {
        // spatialBlend == 1 -> 효과음
        float finalVolume = volume * SFXVolume;
        SoundSystem.SoundPlayer.PlaySound(clipName, pos, finalVolume, spatialBlend);
    }
    public static void PlaySFX(AudioClip clip, Vector3 pos, float volume = 1f, float spatialBlend = 1f)
    {
        if (clip == null || SoundSystem.SoundPlayer == null)
            return;

        float finalVolume = volume * SFXVolume;

        SoundSystem.SoundPlayer.PlaySound(
            clip,
            pos,
            finalVolume,
            spatialBlend
        );
    }


    public static void PlayBGM(string clipName, float volume = 1f)
    {
        // spatialBlend == 0 -> 배경음
        float finalVolume = volume * BGMVolume;
        SoundSystem.SoundPlayer.PlaySound(clipName, Vector3.zero, finalVolume, 0f);
    }

    // BGM을 실제로 한번 재생시키는 코드
    // 브금예시 : SettingsUI.PlayBGM("BackgroundMusic", pos);
    // SFX예시 : SettingsUI.PlaySFX("Darius_AttackSound", pos);
    // 현재 SFXManager.PlaySound는 전역 볼륨 정보를 사용하지 않기 때문에
    // 전역 볼륨 기능을 넣으려면 재생 전 volume x SFXVolume 또는 BGMVolume 계산이 필요하다.
}
