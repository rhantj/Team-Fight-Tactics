using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoUIManager : MonoBehaviour
{
    public static ItemInfoUIManager Instance;

    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private Vector2 offset = new Vector2(60f, -60f); //오버시 오른쪽 아래 등장

    private GameObject currentUI;
    private RectTransform currentRect;
    private ItemData currentData;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = uiRoot.GetComponentInParent<Canvas>();
    }
    private void Update()
    {
        if (currentRect != null)
        {
            FollowMouse(currentRect);
        }
    }


    public void Show(ItemData data)
    {
        if (currentUI != null && currentData == data)
        {
            return;
        }

        Hide();

        if(data.infoUIPrefab == null)
        {
            return;
        }

        currentUI = Instantiate(data.infoUIPrefab, uiRoot);
        currentRect = currentUI.GetComponent<RectTransform>();
        currentUI.SetActive(true);

        FollowMouse(currentRect);
    }

    public void Hide()
    {
        if (currentUI != null)
        {
            Destroy(currentUI);
            currentUI = null;
            currentRect = null;
            currentData = null;
        }
    }

    private void FollowMouse(RectTransform ui)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRoot, Input.mousePosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPos);

        ui.anchoredPosition = localPos + offset;
    }
}
