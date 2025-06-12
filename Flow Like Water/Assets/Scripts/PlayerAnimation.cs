using System.Collections;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Animation Controller")]
    public Animator animator;
    public Animator paddleAnimator;

    public bool isFront;
    
    [Header("Animation Parameters")]
    private readonly string TRIGGER_LEFT = "TriggerLeft";
    private readonly string HOLD_LEFT = "HoldLeft";
    
    private readonly string TRIGGER_RIGHT = "TriggerRight";
    private readonly string HOLD_RIGHT = "HoldRight";
    
    
    
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (paddleAnimator == null)
            paddleAnimator = GetComponent<Animator>();

        if (isFront)
            animator.speed = 0.9f;
    }
    
    public void TriggerLeftPaddle()
    {
        if (animator != null)
            animator.SetTrigger(TRIGGER_LEFT);
        if (paddleAnimator != null)
            paddleAnimator.SetTrigger(TRIGGER_LEFT);
    }
    
    public void TriggerRightPaddle()
    {
        if (animator != null)
            animator.SetTrigger(TRIGGER_RIGHT);
        if (paddleAnimator != null)
            paddleAnimator.SetTrigger(TRIGGER_RIGHT);
    }
    
    public void TriggerLeftHold()
    {
        if (animator != null)
            animator.SetBool(HOLD_LEFT, true);
        if (paddleAnimator != null)
            paddleAnimator.SetBool(HOLD_LEFT, true);
        StartCoroutine(SetAnimatorSpeedCoroutine(1f, true));
    }
    
    public void TriggerRightHold()
    {
        if (animator != null)
            animator.SetBool(HOLD_RIGHT, true);
        if (paddleAnimator != null)
            paddleAnimator.SetBool(HOLD_RIGHT, true);
        StartCoroutine(SetAnimatorSpeedCoroutine(1f, true));
    }
    
    public void TriggerLeftRelease()
    {
        if (animator != null)
            animator.SetBool(HOLD_LEFT, false);
        if (paddleAnimator != null)
            paddleAnimator.SetBool(HOLD_LEFT, false);
        StartCoroutine(SetAnimatorSpeedCoroutine(0.1f, false));
    }
    
    public void TriggerRightRelease()
    {
        if (animator != null)
            animator.SetBool(HOLD_RIGHT, false);
        if (paddleAnimator != null)
            paddleAnimator.SetBool(HOLD_RIGHT, false);
        StartCoroutine(SetAnimatorSpeedCoroutine(0.1f, false));
    }
    
    // Optional: Get current animation state
    public bool IsInIdle()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }
    
    public bool IsInLeftPaddle()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("PaddleLeft");
    }
    
    public bool IsInRightPaddle()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("PaddleRight");
    }

    IEnumerator SetAnimatorSpeedCoroutine(float time, bool isStopping)
    {
        
        yield return new WaitForSeconds(time);
        
        animator.speed = isStopping ? 0 : 1;
        paddleAnimator.speed = isStopping ? 0 : 1;
    }
}