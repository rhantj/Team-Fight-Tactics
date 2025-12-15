using UnityEngine;

public class StateMachine : MonoBehaviour
{
    //=====================================================
    //                  Animator 파라미터 명
    //=====================================================
    private static readonly int HashState = Animator.StringToHash("State"); 
    private static readonly int HashToIdle = Animator.StringToHash("ToIdle"); 

    //=====================================================
    //                  필드
    //=====================================================
    [SerializeField] private Animator animator; 
    public UnitState CurrentState { get; private set; } 

    //=====================================================
    //                  외부 호출용
    //=====================================================

    //Idle은 Animator 기본 상태이므로 Trigger(ToIdle)로만 복귀합니다
    public void SetIdle() => ForceIdle();

    public void SetMove() => ChangeState(UnitState.Move);     
    public void SetBattle() => ChangeState(UnitState.Battle);
    public void SetSkill() => ChangeState(UnitState.Skill);   
    public void SetDie() => ChangeState(UnitState.Die);     
    public void SetVictory() => ChangeState(UnitState.Victory); 

    //=====================================================
    //                  초기화
    //=====================================================
    private void Awake()
    {
        //Animator 미지정이면 오브젝트에서 탐색
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        //Animator 내부 상태를 초기화하여 이전 애니 잔상을 제거합니다.
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        //풀에서 꺼낼 때 항상 Idle 상태로 시작
        ForceIdle();
    }

    //=====================================================
    //                  상태 변경
    //=====================================================
    public void ChangeState(UnitState newState)
    {
        if (animator == null) return;

        if (newState == UnitState.Idle)
        {
            ForceIdle();
            return;
        }

        if (CurrentState == newState) return;

        CurrentState = newState;
        animator.SetInteger(HashState, (int)newState); 
    }

    //=====================================================
    //                  Idle 강제 복귀
    //=====================================================
    public void ForceIdle()
    {
        if (animator == null) return;

        CurrentState = UnitState.Idle;

        //Trigger 중복 입력 꼬임 방지
        animator.ResetTrigger(HashToIdle);
        animator.SetTrigger(HashToIdle);
    }
}
