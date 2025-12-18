using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Chess chess;
    private Animator animator;
    private StateMachine sm;

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

        StartCoroutine(CastRoutine(skill));
        return true;
    }

    private IEnumerator CastRoutine(SkillBase skill)
    {
        IsCasting = true;

        if (chess != null) chess.overrideState = true;

        sm?.SetSkill();

        if (HasAnimParam("UseSkill"))
            animator.SetTrigger("UseSkill");

        // 스킬 실제 실행
        yield return skill.Execute(chess);

        if (chess != null) chess.overrideState = false;

        sm?.SetBattle();

        IsCasting = false;
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
}
