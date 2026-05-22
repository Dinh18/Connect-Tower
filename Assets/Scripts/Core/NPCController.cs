using System.Collections;
using System.Data;
using System.Linq.Expressions;
using UnityEngine;

public enum NPCState
{
    Idle,
    Waving,
    Crying,
    Excited,
    Sad,
    Sleep
}

public class NPCController : MonoBehaviour
{
    private Animator animator;
    private NPCState currState;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameEventBus.Subscribe<RequestChangeAnimationNPC>(ChangeNPCState);
        currState = NPCState.Idle;
    }

    void OnDestroy()
    {
        GameEventBus.UnSubscribe<RequestChangeAnimationNPC>(ChangeNPCState);
    }
    // public void ChangeAnimation(GameStateChangedEvent evt)
    // {
    //     Debug.Log("Change State: " + evt.newState);
    //     if(evt.newState == GameManager.GameState.MainMenu)
    //     {
    //         switch(CoreServices.Get<GameManager>().GetPrevState())
    //         {
    //             case GameManager.GameState.None:
    //                 StartCoroutine(PlayWavingDelay(3));
    //                 break;
    //             case GameManager.GameState.Win:
    //                 animator.Play("Excited");
    //                 break;
    //             case GameManager.GameState.Lose:
    //                 animator.Play("Crying");
    //                 break;
    //         }
    //     }
    //     if(evt.newState == GameManager.GameState.Win)
    //     {
    //         animator.Play("Excited");
    //     }
    // }

    // public IEnumerator PlayWavingDelay(float time)
    // {
    //     yield return new WaitForSeconds(time);
    //     animator.Play("Waving");
    // }

    public void ChangeNPCState(RequestChangeAnimationNPC evt)
    {
        currState = evt.newState;
        switch(evt.newState)
        {
            case NPCState.Idle:
                animator.Play("Idle");
                break;
            case NPCState.Waving:
                animator.CrossFade("Waving",0.05f);
                break;
            case NPCState.Crying:
                animator.CrossFade("Crying",0.05f);
                break;
            case NPCState.Excited:
                animator.CrossFade("Excited",0.05f);
                break;
            case NPCState.Sad:
                animator.CrossFade("Sad",0.05f);
                break;
            case NPCState.Sleep:
                animator.CrossFade("Sleep",0.05f);
                break;
            default:
                Debug.LogWarning("Sai tên animation");
                animator.Play("Idle");
                break;
        }
    }

    private void PlayAnimation(string nameAnim, float timeTrasition)
    {
        animator.CrossFade(nameAnim, timeTrasition);
    }
}
