using UnityEngine;
using UnityEngine.UI;

public class ChessStatusUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ChessStateBase targetChess;

    [Header("HP / Shield / Mana")]
    [SerializeField] private Image hpFillImage;        // 초록색
    [SerializeField] private Image shieldFillImage;    // 흰색
    [SerializeField] private Image manaFillImage;

    [Header("HP Bar Settings")]
    [SerializeField] private float barMaxWidth = 100f; // 프레임 기준 전체 폭

    [Header("Star Frame")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite[] starFrameSprites;

    [Header("Position")]
    [SerializeField] private float extraHeadOffset = 0.3f; // 머리 위 여유 (기존 heightOffset 대체)

    private Vector3 cachedLocalAnchor; 
    private bool hasAnchor = false;

    [SerializeField] private float followSmoothTime = 0.05f; 
    private Vector3 followVel;

    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;

    private RectTransform hpRect;
    private RectTransform shieldRect;

    private void Awake()
    {
        hpRect = hpFillImage.rectTransform;
        shieldRect = shieldFillImage.rectTransform;
    }

    private void LateUpdate()
    {
        if (targetChess == null) return;

        Vector3 desired;

        if (hasAnchor)
        {   
            desired = targetChess.transform.TransformPoint(cachedLocalAnchor);
            desired += Vector3.up * extraHeadOffset;
        }
        else
        {
            desired = targetChess.transform.position + Vector3.up * 2f;
        }

        if (followSmoothTime <= 0f)
            transform.position = desired;
        else
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVel, followSmoothTime);

        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }

    // 실드가 HP바와 같이 보이다보니 분기를 나눠 시각적으로 깨지지 않도록 처리
    private void UpdateHP()
    {
        if (targetChess == null) return;

        int hp = Mathf.Max(0, targetChess.CurrentHP);
        int shield = targetChess.CurrentShield;
        int maxHp = targetChess.MaxHP;
        if (maxHp <= 0) return;

        // HP 비율 (항상 MaxHP 기준)
        float hpRatio = Mathf.Clamp01((float)hp / maxHp);
        hpFillImage.fillAmount = hpRatio;

        // Shield 없으면 종료
        if (shield <= 0)
        {
            shieldFillImage.gameObject.SetActive(false);
            return;
        }

        shieldFillImage.gameObject.SetActive(true);

        RectTransform shieldRT = shieldFillImage.rectTransform;

        // 프리팹에서 잡아둔 Y값 절대 보존
        float originalY = shieldRT.anchoredPosition.y;

        // ===========================
        // CASE 1
        // CurrentHP + Shield <= MaxHP
        // ===========================
        if (hp + shield <= maxHp)
        {
            // MaxHP 기준 비율
            float shieldRatio = (float)shield / maxHp;

            shieldFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            shieldFillImage.fillAmount = shieldRatio;

            // HP 오른쪽에 붙임
            float hpWidth = hpFillImage.rectTransform.rect.width * hpRatio;
            shieldRT.anchoredPosition = new Vector2(hpWidth, originalY);
        }
        // ===========================
        // CASE 2
        // CurrentHP + Shield > MaxHP
        // ===========================
        else
        {
            // 변경 부분
            // (HP + Shield) 를 기준으로 Shield 비율 계산
            float total = hp + shield;
            float shieldRatio = shield / total;

            shieldFillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
            shieldFillImage.fillAmount = shieldRatio;

            // 프레임 기준, 오른쪽부터 덮음 (Y 유지)
            shieldRT.anchoredPosition = new Vector2(0f, originalY);
        }
    }



    private void UpdateMana()
    {
        if (manaFillImage == null) return;

        int maxMana = targetChess.BaseData.mana;
        if (maxMana <= 0) return;

        manaFillImage.fillAmount =
            (float)targetChess.CurrentMana / maxMana;
    }

    private void UpdateStarFrame()
    {
        if (frameImage == null) return;

        int starLevel = targetChess.StarLevel;
        if (starLevel <= 0 || starLevel > starFrameSprites.Length)
            return;

        frameImage.sprite = starFrameSprites[starLevel - 1];
    }

    // 초기화 진입점
    // 기물 생성 또는 재사용 시 UI를 안전하게 초기화하기 위한 메서드
    public void Bind(ChessStateBase chess)
    {
        targetChess = chess;

        if (targetChess != null)
        {
            cachedRenderers = targetChess.GetComponentsInChildren<Renderer>(true);
            cachedColliders = targetChess.GetComponentsInChildren<Collider>(true);
            CacheAnchorFromBounds();
        }
        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }

    // 강제 HP 갱신용
    public void ForceRefreshHP()
    {
        UpdateHP();
    }

    /// <summary>
    /// Renderer bounds -> 전체 캡슐화
    /// 없다면 Collider bounds 사용
    /// 모델의 실제 최고점 (b.max.y) 기준
    /// Local Anchor로 캐싱
    /// 캐릭터 크기가 달라도 자동 대응되도록 함
    /// </summary>
    private void CacheAnchorFromBounds()
    {
        hasAnchor = false;
        Transform t = targetChess.transform;

        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            Bounds b = cachedRenderers[0].bounds;
            for (int i = 1; i < cachedRenderers.Length; i++)
                b.Encapsulate(cachedRenderers[i].bounds);

            Vector3 rootPos = t.position;
            Vector3 topCenterWorld = new Vector3(rootPos.x, b.max.y, rootPos.z);

            cachedLocalAnchor = t.InverseTransformPoint(topCenterWorld);
            hasAnchor = true;
            return;
        }

        if (cachedColliders != null && cachedColliders.Length > 0)
        {
            Bounds b = cachedColliders[0].bounds;
            for (int i = 1; i < cachedColliders.Length; i++)
                b.Encapsulate(cachedColliders[i].bounds);

            Vector3 rootPos = t.position;
            Vector3 topCenterWorld = new Vector3(rootPos.x, b.max.y, rootPos.z);

            cachedLocalAnchor = t.InverseTransformPoint(topCenterWorld);
            hasAnchor = true;
            return;
        }
    }

}
