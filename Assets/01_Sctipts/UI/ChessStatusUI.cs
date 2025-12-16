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

    [Tooltip("Index 0 = 1¼º, 1 = 2¼º, 2 = 3¼º")]
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
        if (hpFillImage == null) return;

        int maxHP = targetChess.MaxHP;
        if (maxHP <= 0) return;

        hpFillImage.fillAmount = (float)targetChess.CurrentHP / maxHP;
    }

    private void UpdateMana()
    {
        if (manaFillImage == null) return;

        int maxMana = targetChess.BaseData.mana;
        if (maxMana <= 0) return;

        manaFillImage.fillAmount = (float)targetChess.CurrentMana / maxMana;
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
