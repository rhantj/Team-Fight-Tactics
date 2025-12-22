using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Chess chess;
    private Animator animator;
    private StateMachine sm;
    private SkillBase currentSkill;
    private int remainingRepeats;
    private bool isRepeatCasting;

    public bool IsCasting { get; private set; }

    private void Awake()
    {
        chess = GetComponent<Chess>();
        animator = GetComponent<Animator>();
        sm = GetComponent<StateMachine>();
    }

    public bool TryCastSkill()
    {
        if (IsCasting) return false;

        SkillBase skill = GetComponent<SkillBase>();
        if (skill == null) return false;

        if (chess != null)
        {
            var t = chess.CurrentTarget;
            if (t == null || t.IsDead) return false;
        }

        currentSkill = skill;
        remainingRepeats = Mathf.Max(1, skill.repeatCount);
        isRepeatCasting = true;

        StartCoroutine(CastRoutine(skill));
        return true;
    }



    private IEnumerator CastRoutine(SkillBase skill)
    {
        IsCasting = true;
        if (chess != null) chess.overrideState = true;

        if (animator != null)
            animator.ResetTrigger("Attack");

        if (HasAnimParam("UseSkill"))
            animator.SetTrigger("UseSkill");

        // 2초짜리 1사이클만 돈다
        yield return skill.Execute(chess);

        //if (chess != null)
        //{
        //    chess.overrideState = false;
        //    //chess.attackTimer = 0f;  //이친구 키면 스킬모션 캔슬되고 평타나가용
        //}

        //sm?.SetIdle();
        //IsCasting = false;
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

    //모션딜레이 타이밍 잡기 위해서 애니메이션 이벤트 하나 추가했습니다.
    public void OnSkillAnimEnd()
    {
        if (!isRepeatCasting || currentSkill == null)
        {
            //종료
            if (chess != null) chess.overrideState = false;
            sm?.SetIdle();
            IsCasting = false;
            return;
        }

        remainingRepeats--;

        if (remainingRepeats > 0)
        {
            //다음 사이클 다시 트리거
            if (animator != null)
                animator.SetTrigger("UseSkill");

            //다음 1회 Execute 다시 실행
            StartCoroutine(currentSkill.Execute(chess));
            return;
        }

        isRepeatCasting = false;
        currentSkill = null;

        if (chess != null) chess.overrideState = false;
        sm?.SetIdle();
        IsCasting = false;
    }

}
