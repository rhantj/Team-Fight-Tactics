using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public void SetIdle() => ChangeState(UnitState.Idle);
    public void SetMove() => ChangeState(UnitState.Move);
    public void SetBattle() => ChangeState(UnitState.Battle);
    public void SetSkill() => ChangeState(UnitState.Skill);
    public void SetDie() => ChangeState(UnitState.Die);
    public void SetVictory() => ChangeState(UnitState.Victory); //12.12 add Kim

    //=====================================================
    //                  필드
    //=====================================================
    [SerializeField] private Animator animator;
    public UnitState CurrentState { get; private set; }

    //=====================================================
    //                  초기화
    //=====================================================
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        Debug.Log(
            $"[SM Awake] {name} " +
            $"controller={(animator && animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "NULL")} " +
            $"avatar={(animator && animator.avatar ? animator.avatar.name : "NULL")}"
        );
    }

    private void OnEnable()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        Debug.Log($"[SM OnEnable] {name} -> Idle");
        ChangeState(UnitState.Idle);
    }

    //=====================================================
    //                  상태 변경
    //=====================================================
    public void ChangeState(UnitState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        animator.SetInteger("State", (int)newState);
    }
}
