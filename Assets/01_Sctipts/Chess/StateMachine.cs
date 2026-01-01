using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private static readonly int HashState = Animator.StringToHash("State");  // 0=IdlePose, 1=Run
    private static readonly int HashToIdle = Animator.StringToHash("ToIdle"); // 강제복귀용(라운드 전환 등)
    private bool locked;
    public void Lock() => locked = true;    
    public void Unlock() => locked = false;
    [SerializeField] private Animator animator;
    public UnitState CurrentState { get; private set; }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
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

        SetIdlePose();
    }

    // ====== 전투 루프용 ======
    public void SetIdle() => SetIdlePose();        
    public void SetMove() => SetMoveState();           // State=1

    private void SetIdlePose()
    {
        if (animator == null) return;
        if (locked) return;
        CurrentState = UnitState.Idle;
        animator.SetInteger(HashState, 0);
    }

    private void SetMoveState()
    {
        if (animator == null) return;
        if (locked) return;
        if (CurrentState == UnitState.Move) return;
        CurrentState = UnitState.Move;
        animator.SetInteger(HashState, 1);
    }

    // ====== 라운드 전환/강제 연출용 ======
    public void ForceIdle()
    {
        if (animator == null) return;
        if (locked) return;
        CurrentState = UnitState.Idle;

        animator.SetInteger(HashState, 0);     // 포즈 먼저 맞추고
        animator.ResetTrigger(HashToIdle);
        animator.SetTrigger(HashToIdle);       // 필요할 때만 트리거
    }
}
