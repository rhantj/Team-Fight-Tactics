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
    private GameManager gm;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gm = GameManager.Instance;
        gm.OnRoundStateChanged += UpdateRoundStateText;
        gm.OnPreparationTimerUpdated += UpdateTimerUI;
        gm.OnBattleTimerUpdated += UpdateBattleTimerUI;
        GameManager.Instance.OnTimerMaxTimeChanged += SetMaxTime;

    }

    private void OnDestroy()
    {
        if (gm == null) return;
        gm.OnRoundStateChanged -= UpdateRoundStateText;
        gm.OnPreparationTimerUpdated -= UpdateTimerUI;
        gm.OnBattleTimerUpdated -= UpdateBattleTimerUI;
        //GameManager.Instance.OnTimerMaxTimeChanged -= SetMaxTime;

    }

    private void SetMaxTime(float t)
    {
        maxTime = Mathf.Max(0.01f, t);
    }

    private void UpdateRoundStateText(RoundState state)
    {
        roundStateText.text = state.ToString();

        if(state == RoundState.Preparation)
        {
            maxTime = GameManager.Instance.preparationTime;
        }
        else if(state == RoundState.Battle)
        {
            maxTime = GameManager.Instance.battleTime;
        }
        else
        {
            timerBar.fillAmount = 0f;
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (time < 0) time = 0;

        timerText.text = $"{time:F1} ";
        timerBar.fillAmount = time / maxTime;
    }


    private void UpdateBattleTimerUI(float time)
    {
        if (time < 0) time = 0;

        timerText.text = $"{time:F1} ";
        timerBar.fillAmount = time / GameManager.Instance.battleTime;
    }
}
