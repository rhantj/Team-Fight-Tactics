using UnityEngine;

public class VayneBasicAttackSFXListener : MonoBehaviour
{
    [Header("Normal Attack SFX")]
    [SerializeField]
    private string basicAttackSfxName;

    [Header("Vayne W SFX")]
    [SerializeField]
    private string vayneWSfxName;

    [SerializeField, Tooltip("몇 타마다 W 사운드")]
    private int procEveryHits = 3;

    private int hitCount = 0;
    private Chess chess;

    private void Awake()
    {
        chess = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        if (chess != null)
            chess.OnBasicAttackHit += OnBasicAttack;
    }

    private void OnDisable()
    {
        if (chess != null)
            chess.OnBasicAttackHit -= OnBasicAttack;

        hitCount = 0;
    }

    private void OnBasicAttack()
    {
        hitCount++;

        // 3타째 → W 사운드
        if (hitCount >= procEveryHits)
        {
            hitCount = 0;

            if (!string.IsNullOrEmpty(vayneWSfxName))
            {
                SettingsUI.PlaySFX(
                    vayneWSfxName,
                    transform.position,
                    1f,
                    1f
                );
            }

            return; // 기본 평타 소리 차단
        }

        // 일반 평타
        if (!string.IsNullOrEmpty(basicAttackSfxName))
        {
            SettingsUI.PlaySFX(
                basicAttackSfxName,
                transform.position,
                1f,
                1f
            );
        }
    }
}
