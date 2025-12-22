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
    [SerializeField] private float heightOffset = 2f;

    private RectTransform hpRect;
    private RectTransform shieldRect;

    private void Awake()
    {
        hpRect = hpFillImage.rectTransform;
        shieldRect = shieldFillImage.rectTransform;
    }

    private void LateUpdate()
    {
        if (targetChess == null || targetChess.IsDead)
            return;

        Vector3 worldPos = targetChess.transform.position;
        worldPos.y += heightOffset;
        transform.position = worldPos;

        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }

    private void UpdateHP()
    {
        if (targetChess == null) return;

        int hp = targetChess.CurrentHP;
        int shield = targetChess.CurrentShield;
        int maxHp = targetChess.MaxHP;
        if (maxHp <= 0) return;

        // 비율 계산
        float hpRatio = Mathf.Clamp01((float)hp / maxHp);
        float shieldRatio = Mathf.Clamp01((float)shield / maxHp);

        // HP Fill
        hpFillImage.fillAmount = hpRatio;

        // Shield가 없으면 숨김
        if (shield <= 0)
        {
            shieldFillImage.gameObject.SetActive(false);
            return;
        }

        shieldFillImage.gameObject.SetActive(true);
        shieldFillImage.fillAmount = shieldRatio;

        RectTransform shieldRT = shieldFillImage.rectTransform;

        // 프리팹에서 잡아둔 Y값 절대 보존
        float originalY = shieldRT.anchoredPosition.y;

        // ===========================
        // CASE 1
        // CurrentHP + Shield <= MaxHP
        // 초록색 오른쪽에 붙여서 표시
        // ===========================
        if (hp + shield <= maxHp)
        {
            shieldFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            float hpWidth = hpFillImage.rectTransform.rect.width * hpRatio;
            shieldRT.anchoredPosition = new Vector2(hpWidth, originalY);
        }
        // ===========================
        // CASE 2
        // CurrentHP + Shield > MaxHP
        // 쉴드는 오른쪽부터 덮음
        // ===========================
        else
        {
            shieldFillImage.fillOrigin = (int)Image.OriginHorizontal.Right;

            // 위치는 프레임 기준 그대로 (Y 유지)
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

    public void Bind(ChessStateBase chess)
    {
        targetChess = chess;
        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }
}
