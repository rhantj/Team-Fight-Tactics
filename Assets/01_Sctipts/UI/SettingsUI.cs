using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Volume Value Text")]
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    //설정창 열리는 불변수
    private bool isOpen = false;

    void Start()
    {
        //여기서 설정창 꺼두고
        settingsPanel.SetActive(false);

        //UI 초기화(0~100 기준으로)
        bgmSlider.value = BGMVolume * 100f;
        sfxSlider.value = SFXVolume * 100f;

        //볼륨 텍스트도 설정
        bgmValueText.text = ((int)bgmSlider.value).ToString();
        sfxValueText.text = ((int)sfxSlider.value).ToString();
    }

    
    void Update()
    {
        //ESC 누르면 설정창 토글
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsUI();
        }
    }

    //설정창 토글 함수
    public void ToggleSettingsUI()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        //설정창 열리면 시간 멈추기
        Time.timeScale = isOpen ? 0f : 1f;
    }
    //배경음 조절 함수
    public void OnChangeBGM(float value)
    {
        BGMVolume = value / 100f; //0~1로 변환
        bgmValueText.text = ((int)value).ToString();
    }

    //효과음 조절 함수
    public void OnChangeSFX(float value)
    {
        SFXVolume = value / 100f; //0~1로 변환
        sfxValueText.text = ((int)value).ToString();
    }

    //설정창 닫는 함수
    public void OnClickClose() 
    {
        Debug.Log("CLOSE CLICKED");
        ToggleSettingsUI();
    }
       

    //게임 종료 함수
    public void OnClickExitGame() => Application.Quit();

    //메인 메뉴로 돌아가는 함수
    public void OnClickReturnToMainMenu()
    {
        //시간 흐름 복구
        Time.timeScale = 1f;
        //메인 메뉴 씬 로드
        //아직 만들어 두지 않아서 이런식으로 구성 예정
        //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    //사운드 재생 래퍼(Wrapper) 함수
    public static void PlaySFX(string clipName, Vector3 pos, float volume = 1f, float spatialBlend = 0f)
    {
        // spatialBlend == 1 -> 효과음
        float finalVolume = volume * SFXVolume;
        SoundSystem.SoundPlayer.PlaySound(clipName, pos, finalVolume, spatialBlend);
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
