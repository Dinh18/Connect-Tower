using UnityEngine;

public enum BossState
{
    Idle,
    Hard,
    SuperHard,
}
public class BossController : MonoBehaviour
{
    public Animator animator;
    private BossState currState;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // GameEventBus.Subscribe<StartBorderFlashEvent>(OnStartBorderFlash);
        GameEventBus.Subscribe<RequestChangeAnimationBoss>(ChangeBossState);
        currState = BossState.Idle;
    }
    void OnDestroy()
    {
        // GameEventBus.UnSubscribe<StartBorderFlashEvent>(OnStartBorderFlash);
        GameEventBus.UnSubscribe<RequestChangeAnimationBoss>(ChangeBossState);
    }
    public void ChangeBossState(RequestChangeAnimationBoss evt)
    {
        currState = evt.newState;
        switch(evt.newState)
        {
            case BossState.Idle:
                animator.Play("idle");
                break;
            case BossState.Hard:
                animator.Play("surprised");
                break;
            case BossState.SuperHard:
                animator.Play("attack");
                break;
            default:
                Debug.LogWarning("Sai tên animation");
                animator.Play("idle");
                break;
        }
    }
}
