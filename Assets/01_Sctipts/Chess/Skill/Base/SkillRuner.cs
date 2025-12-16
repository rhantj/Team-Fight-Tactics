using System.Collections;
using UnityEngine;

public class SkillRunner : MonoBehaviour
{
    private Chess chess;               
    private Animator animator;
    private StateMachine sm;

    private bool isCasting;

    private void Awake()
    {
        chess = GetComponent<Chess>();
        animator = GetComponent<Animator>();
        sm = GetComponent<StateMachine>();   
    }

    public void RequestCast()
    {
        if (isCasting) return;

        SkillBase skill = GetComponent<SkillBase>();
        if (skill == null) return;

        StartCoroutine(CastRoutine(skill));
    }

    private IEnumerator CastRoutine(SkillBase skill)
    {
        isCasting = true;

        chess.overrideState = true; 

        sm?.SetSkill();
        animator?.SetTrigger("UseSkill");
        yield return skill.Execute(chess);

        chess.overrideState = false;
        sm?.SetBattle();

        isCasting = false;
    }
}
