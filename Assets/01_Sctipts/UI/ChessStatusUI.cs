using UnityEngine;
using UnityEngine.UI;

public class ChessStatusUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ChessStateBase targetChess; // 임시 수동 연결용

    [Header("HP / MP Fill Images")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image manaFillImage;

    [Header("Star Frame")]
    [SerializeField] private Image frameImage;

    [Tooltip("Index 0 = 1성, 1 = 2성, 2 = 3성")]
    [SerializeField] private Sprite[] starFrameSprites;

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
        int starLevel = targetChess.StarLevel;

        if (frameImage == null) return;
        if (starFrameSprites == null || starFrameSprites.Length == 0) return;
        if (starLevel <= 0 || starLevel > starFrameSprites.Length) return;

        frameImage.sprite = starFrameSprites[starLevel - 1];
    }

    // =========================
    // 나중에 자동 연결용
    // =========================
    public void Bind(ChessStateBase chess)
    {
        targetChess = chess;
        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }
    

}
