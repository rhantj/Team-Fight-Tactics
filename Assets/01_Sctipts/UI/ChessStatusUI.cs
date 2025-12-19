using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기물의 현재 상태를 UI로 표시하는 컴포넌트.
/// 
/// - 대상 기물의 체력과 마나를 Fill Image로 표현한다.
/// - 기물의 성급에 따라 프레임 이미지를 변경한다.
/// - 기물 머리 위에 표시되는 오버헤드 UI로 사용된다.
/// </summary>
public class ChessStatusUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private ChessStateBase targetChess;   // 상태를 표시할 대상 기물

    [Header("HP / MP Fill Images")]
    [SerializeField] private Image hpFillImage;            // 체력 표시용 Fill Image
    [SerializeField] private Image manaFillImage;          // 마나 표시용 Fill Image

    [Header("Star Frame")]
    [SerializeField] private Image frameImage;             // 성급 프레임 이미지
    [Tooltip("Index 0 = 1성, 1 = 2성, 2 = 3성")]
    [SerializeField] private Sprite[] starFrameSprites;    // 성급별 프레임 스프라이트

    [Header("Position")]
    [SerializeField] private float heightOffset = 2f;      // Inspector에서 조정 가능한 높이 오프셋

    private void LateUpdate()
    {
        if (targetChess == null || targetChess.IsDead)
            return;

        Vector3 worldPos = targetChess.transform.position;
        worldPos.y += heightOffset;

        if (targetChess.name.Contains("Baron") || targetChess.name.Contains("Enemy"))
        {
            Debug.Log($"[Baron HP] Target Pos: {targetChess.transform.position}, " +
                      $"UI Pos: {worldPos}, heightOffset: {heightOffset}, " +
                      $"Canvas RenderMode: {GetComponentInParent<Canvas>()?.renderMode}");
        }

        transform.position = worldPos;

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

    /// <summary>
    /// UI와 기물을 연결한다.
    /// 외부에서 기물 생성 시 호출되어 대상 기물을 설정한다.
    /// </summary>
    public void Bind(ChessStateBase chess)
    {
        targetChess = chess;

        Debug.Log($"[ChessStatusUI] Bind: {chess.name}, Canvas Parent: {transform.parent?.name ?? "ROOT"}");

        UpdateHP();
        UpdateMana();
        UpdateStarFrame();
    }
}