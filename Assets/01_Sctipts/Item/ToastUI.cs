using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
    public static ToastUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private float showDuration = 1.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(string message, Color color)
    {
        if(currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        gameObject.SetActive(true);

        if(toastText != null)
        {
            toastText.text = message;
            toastText.color = color;
        }

        currentRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        gameObject.SetActive(false);
        currentRoutine = null;
    }
}
