using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public static TimeUI instance;

    [SerializeField] private TMP_Text roundStateText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerBar;

    private float maxTime = 60.0f; //준비 시간과 동일하게 설정해야함

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnRoundStateChanged += UpdateRoundStateText;
        GameManager.Instance.OnPreparationTimerUpdated += UpdateTimerUI;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnRoundStateChanged -= UpdateRoundStateText;
        GameManager.Instance.OnPreparationTimerUpdated -= UpdateTimerUI;
    }

    private void UpdateRoundStateText(RoundState state)
    {
        roundStateText.text = state.ToString();

        if(state == RoundState.Preparation)
        {
            maxTime = GameManager.Instance.preparationTime;
        }
        else
        {
            timerBar.fillAmount = 0f;
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (time < 0) time = 0;

        timerText.text = $"{time:F1} 초";
        timerBar.fillAmount = time / maxTime;
    }
}
