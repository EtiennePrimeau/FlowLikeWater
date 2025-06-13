using System.Collections;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Animation Controller")]
    public Animator animator;
    public GameObject leftPaddle;
    public GameObject rightPaddle;

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

        if (isFront)
            animator.speed = 0.9f;
    }
    
    public void TriggerLeftPaddle()
    {
        if (animator != null)
            animator.SetTrigger(TRIGGER_LEFT);
        rightPaddle.SetActive(true);
        leftPaddle.SetActive(false);
    }
    
    public void TriggerRightPaddle()
    {
        if (animator != null)
            animator.SetTrigger(TRIGGER_RIGHT);
        rightPaddle.SetActive(false);
        leftPaddle.SetActive(true);
    }
    
    public void TriggerLeftHold()
    {
        if (animator != null)
            animator.SetBool(HOLD_LEFT, true);
        rightPaddle.SetActive(true);
        leftPaddle.SetActive(false);
        
        StartCoroutine(SetAnimatorSpeedCoroutine(1f, true));
    }
    
    public void TriggerRightHold()
    {
        if (animator != null)
            animator.SetBool(HOLD_RIGHT, true);
        rightPaddle.SetActive(false);
        leftPaddle.SetActive(true);
        
        StartCoroutine(SetAnimatorSpeedCoroutine(1f, true));
    }
    
    public void TriggerLeftRelease()
    {
        if (animator != null)
            animator.SetBool(HOLD_LEFT, false);
        
        StartCoroutine(SetAnimatorSpeedCoroutine(0.1f, false));
    }
    
    public void TriggerRightRelease()
    {
        if (animator != null)
            animator.SetBool(HOLD_RIGHT, false);
        
        StartCoroutine(SetAnimatorSpeedCoroutine(0.1f, false));
    }
    
    IEnumerator SetAnimatorSpeedCoroutine(float time, bool isStopping)
    {
        
        yield return new WaitForSeconds(time);
        
        animator.speed = isStopping ? 0 : 1;
    }
}