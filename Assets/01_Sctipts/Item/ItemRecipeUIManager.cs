using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ItemInfoUIManager코드와 설명 동일
public class ItemRecipeUIManager : MonoBehaviour
{
    public static ItemRecipeUIManager Instance;

    [SerializeField] private RectTransform uiRoot;

    [SerializeField] private Vector2 offset = new Vector2(60f, -60f);

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
        if(currentUI != null && currentData == data)
        {
            return;
        }
        Hide();

        if(data.recipeUIPrefab == null)
        {
            return;
        }

        currentUI = Instantiate(data.recipeUIPrefab, uiRoot);
        currentRect = currentUI.GetComponent<RectTransform>();
        currentData = data;
        currentUI.SetActive(true);

        FollowMouse(currentRect);
    }

    public void Hide()
    {
        if(currentUI != null)
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
        // 2. 오프셋 적용
        Vector2 pos = localPos + offset;

        // 3. Canvas / UI 크기
        Vector2 canvasSize = uiRoot.rect.size;
        Vector2 uiSize = ui.rect.size;

        // 4. Clamp 범위 계산 (Canvas 중앙 기준)
        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f - uiSize.x;

        float minY = -canvasSize.y * 0.5f + uiSize.y;
        float maxY = canvasSize.y * 0.5f;

        // 5. Canvas 밖으로 못 나가게 고정
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // 6. 적용
        ui.anchoredPosition = pos;
    }
}
