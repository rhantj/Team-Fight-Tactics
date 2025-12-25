using UnityEngine;
using System.Collections;

public class DravenSkill_Q : SkillBase
{
    [Header("Cast Timing")]
    [SerializeField] private float windUpTime = 0.2f;

    [Header("Damage (Raw Physical)")]
    [SerializeField, Tooltip("강화 평타 배율 (기본공격 데미지 * 배율)")]
    private float damageMultiplier = 1.5f;

    [SerializeField, Tooltip("추가 고정 피해(옵션)")]
    private int flatBonusDamage = 0;

    [Header("Star Bonus (Optional)")]
    [SerializeField] private int bonus_1Star = 10;
    [SerializeField] private int bonus_2Star = 25;
    [SerializeField] private int bonus_3Star = 40;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess draven = caster as Chess;
        if (draven == null) yield break;

        Chess target = draven.CurrentTarget;
        if (target == null || target.IsDead) yield break;

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        int scaledBasic = draven.AttackDamage * Mathf.Max(1, draven.StarLevel);

        int starBonus = GetStarBonus(draven.StarLevel);

        int rawDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(scaledBasic * damageMultiplier) + flatBonusDamage + starBonus
        );

        target.TakeDamage(rawDamage, draven);
    }

    private int GetStarBonus(int starLevel)
    {
        if (starLevel >= 3) return bonus_3Star;
        if (starLevel == 2) return bonus_2Star;
        return bonus_1Star;
    }
}
