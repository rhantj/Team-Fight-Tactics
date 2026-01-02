using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private const string SkillSpeedParam = "SkillAnimSpeed";
    [SerializeField] private float skillSpeedBase = 1f;

    private Chess chess;
    private Animator animator;
    private StateMachine sm;
    private SkillBase skill;
    private SkillBase currentSkill;
    private int remainingRepeats;
    private bool isRepeatCasting;

    private bool isTimeBasedCasting; // 시간 기반(채널링) 스킬 캐스팅 중인지

    public bool IsCasting { get; private set; }

    private void Awake()
    {
        chess = GetComponent<Chess>();
        animator = GetComponent<Animator>();
        sm = GetComponent<StateMachine>();
        skill = GetComponent<SkillBase>();
    }

    public bool TryCastSkill()
    {
        if (IsCasting) return false;

        //SkillBase skill = GetComponent<SkillBase>();
        if (skill == null) return false;

        if (chess != null)
        {
            var t = chess.CurrentTarget;
            if (t == null || t.IsDead) return false;
        }

        StartCoroutine(CastRoutine(skill));
        return true;
    }

    private IEnumerator CastRoutine(SkillBase skill)
    {
        IsCasting = true;
        if (chess != null) chess.overrideState = true;

        if (animator != null)
            animator.ResetTrigger("Attack");
        // 지속형 / 채널링 스킬 (Execute 시간 기준 반복)
        if (!skill.endByAnimEvent)
        {
            isTimeBasedCasting = true;
            int repeats = Mathf.Max(1, skill.repeatCount);
            for (int i = 0; i < repeats; i++)
            {
                ApplySkillAnimSpeed();
                if (HasAnimParam("UseSkill"))
                    animator.SetTrigger("UseSkill");
                yield return skill.Execute(chess);
                if (skill.repeatInterval > 0f)
                    yield return new WaitForSeconds(skill.repeatInterval);
            }
            EndCasting();
            yield break;
        }
        isTimeBasedCasting = false;
        currentSkill = skill;
        remainingRepeats = Mathf.Max(1, skill.repeatCount);
        isRepeatCasting = true;
        ApplySkillAnimSpeed();
        if (HasAnimParam("UseSkill"))
            animator.SetTrigger("UseSkill");

        yield return skill.Execute(chess);
    }
    private void OnDisable()
    {
        if (IsCasting) ForceStopCasting();
    }

    // 모션딜레이 타이밍 잡기 위해서 애니메이션 이벤트 하나 추가했습니다.
    public void OnSkillAnimEnd()
    {
        // 시간 기반 스킬은 애니 이벤트로 종료/반복 처리하지 않는다
        if (isTimeBasedCasting) return;

        if (!isRepeatCasting || currentSkill == null)
        {
            EndCasting();
            return;
        }

        remainingRepeats--;

        if (remainingRepeats > 0)
        {
            ApplySkillAnimSpeed();

            // 다음 사이클 다시 트리거
            if (animator != null)
                animator.SetTrigger("UseSkill");

            // 다음 1회 Execute 다시 실행
            StartCoroutine(currentSkill.Execute(chess));
            return;
        }

        isRepeatCasting = false;
        currentSkill = null;

        EndCasting();
    }

    private void EndCasting()
    {
        isRepeatCasting = false;
        currentSkill = null;
        isTimeBasedCasting = false;

        if (animator != null && HasAnimParam(SkillSpeedParam))
            animator.SetFloat(SkillSpeedParam, 1f);

        if (chess != null) chess.overrideState = false;
        sm?.SetIdle();
        IsCasting = false;
    }

    private void ApplySkillAnimSpeed()
    {
        if (animator == null) return;
        if (!HasAnimParam(SkillSpeedParam)) return;

        float speed = 1f;

        if (chess != null)
        {
            float denom = Mathf.Max(0.01f, skillSpeedBase);
            speed = chess.FinalAttackSpeed / denom;
        }

        animator.SetFloat(SkillSpeedParam, speed);
        //Debug.Log($"[{chess.name}] BaseAS={chess.BaseAttackSpeed}, FinalAS={chess.FinalAttackSpeed}, set={speed}");
    }

    private bool HasAnimParam(string param)
    {
        if (animator == null) return false;

        foreach (var p in animator.parameters)
        {
            if (p.name == param) return true;
        }
        return false;
    }
    public void ForceStopCasting()
    {
        StopAllCoroutines();

        if (animator != null)
        {
            if (HasAnimParam("UseSkill")) animator.ResetTrigger("UseSkill");
            if (HasAnimParam("Attack")) animator.ResetTrigger("Attack");    
        }

        isRepeatCasting = false;
        currentSkill = null;
        isTimeBasedCasting = false;

        if (animator != null && HasAnimParam(SkillSpeedParam))
            animator.SetFloat(SkillSpeedParam, 1f);

        if (chess != null) chess.overrideState = false;
        sm?.SetIdle();
        IsCasting = false;
    }


}
