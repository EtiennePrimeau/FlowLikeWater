using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Animation Controller")]
    public Animator animator;
    public Animator paddleAnimator;

    public bool isFront;
    
    [Header("Animation Parameters")]
    private readonly string TRIGGER_LEFT = "TriggerLeft";
    private readonly string TRIGGER_RIGHT = "TriggerRight";
    
    
    
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
}