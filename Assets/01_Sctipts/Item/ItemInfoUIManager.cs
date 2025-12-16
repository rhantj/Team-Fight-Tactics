using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*전체 정리
 아이템 정보UI를 관리하는 코드
- 슬롯에 마우스를 올리면 Show
- 마우스를 떼면 Hide
- UI는 마우스를 따라다니고, 화면 밖으로 나가지 않도록 Clamp처리함.
*/
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
        //UI가 있으면 마우스 위치를 따라감
        if (currentRect != null)
        {
            FollowMouse(currentRect);
        }
    }

    /*===================== Show메서드 ====================
    정보 UI 표시하는 메서드
    - 만약 이미 같은 Data를 띄우고 있으면 Return
    - 기존의 UI는 hide로 정리하고 새로 생성
    - data.infoUIPrefab이 없으면 표시 안함.
    */
    public void Show(ItemData data)
    {
        if (currentUI != null && currentData == data)
        {
            return;
        }

        //이전 UI제거
        Hide();

        //표시할게 없으면 종료
        if(data.infoUIPrefab == null)
        {
            return;
        }

        //UI 생성 + 활성화
        currentUI = Instantiate(data.infoUIPrefab, uiRoot);
        currentRect = currentUI.GetComponent<RectTransform>();
        currentData = data;
        currentUI.SetActive(true);

        //생성 후 위치 조정
        FollowMouse(currentRect);
    }

    /*================== Hide메서드 ===================
     * 현재 정보 UI 제거
     */
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

    /* ================ FollowMouse메서드 ===================
     - 마우스 위치에 따라 UI 위치 이동
     - Canvas 영역 밖으로 UI가 나가지 않도록 Clamp처리
     */
    private void FollowMouse(RectTransform ui)
    {
        // 1. 스크린 좌표를 uiRoot 로컬 좌표로 변환
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
