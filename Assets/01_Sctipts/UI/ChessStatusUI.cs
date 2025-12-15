using UnityEngine;
using UnityEngine.UI;

public class ChessStatusUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ChessStateBase targetChess;

    [Header("HP / MP Fill Images")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image manaFillImage;

    [Header("Star Frame")]
    [SerializeField] private Image frameImage;

    [Tooltip("Index 0 = 1성, 1 = 2성, 2 = 3성")]
    [SerializeField] private Sprite[] starFrameSprites;

    private RectTransform frameRect;
    private Vector3 baseFrameScale;   // 인스펙터에서 잡은 초기 Scale

    private void Awake()
    {
        if (frameImage != null)
        {
            frameRect = frameImage.rectTransform;
            baseFrameScale = frameRect.localScale; // (예: 1.2, 1.2, 1.2)
        }
    }

    private void Update()
    {
        if (targetChess == null || targetChess.IsDead)
            return;

        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }

    private void UpdateHP()
    {
        int currentHP = targetChess.CurrentHP;
        int maxHP = targetChess.MaxHP;

        if (maxHP <= 0) return;

        hpFillImage.fillAmount = (float)currentHP / maxHP;
    }

    private void UpdateMana()
    {
        int currentMana = targetChess.CurrentMana;
        int maxMana = targetChess.BaseData.mana;

        if (maxMana <= 0) return;

        manaFillImage.fillAmount = (float)currentMana / maxMana;
    }

    private void UpdateStarFrame()
    {
        if (frameImage == null || frameRect == null) return;

        int starLevel = targetChess.StarLevel;
        if (starLevel <= 0 || starLevel > starFrameSprites.Length) return;

        frameImage.sprite = starFrameSprites[starLevel - 1];

        // Anchor / Pivot 고정
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);

        switch (starLevel)
        {
            case 1:
                frameRect.localPosition = new Vector3(-7f, 33f, 0f);
                frameRect.localScale = new Vector3(1.1f, 1.2f, 1.2f);
                break;

            case 2:
                frameRect.localPosition = new Vector3(-12f, 33f, 0f);
                frameRect.localScale = new Vector3(1.2f, 1.8f, 1.2f);
                break;

            case 3:
                frameRect.localPosition = new Vector3(-15f, 33f, 0f);
                frameRect.localScale = new Vector3(1.2f, 1.8f, 1.2f);
                break;
        }
    }


    public void Bind(ChessStateBase chess)
    {
        targetChess = chess;
        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }
}
