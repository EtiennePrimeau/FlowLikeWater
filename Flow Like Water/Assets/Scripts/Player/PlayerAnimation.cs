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
    
    [Header("Difficulty Integration")]
    public float baseAnimationSpeed = 1f;
    
    private float currentDifficultySpeed = 1f;
    private float frontCharacterSpeedModifier = 0.9f;
    
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Calculate difficulty-based speed
        ApplyDifficultySpeed();
    }
    
    void ApplyDifficultySpeed()
    {
        // Get difficulty speed multiplier
        DifficultyState.EnsureInitialized();
        currentDifficultySpeed = baseAnimationSpeed * DifficultyState.GetAnimationSpeedMultiplier();
        
        // Apply front character modifier if needed
        if (isFront)
            currentDifficultySpeed *= frontCharacterSpeedModifier;
            
        // Set the base animation speed
        if (animator != null)
            animator.speed = currentDifficultySpeed;
            
        Debug.Log($"CharacterAnimator speed set to: {currentDifficultySpeed} (Difficulty: {DifficultyState.GetAnimationSpeedMultiplier()}, Front: {isFront})");
    }
    
    // Public method to reapply difficulty (called by DifficultyManager)
    public void RefreshDifficultySpeed()
    {
        ApplyDifficultySpeed();
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
        
        // UPDATED: Use difficulty-based speed instead of hardcoded values
        if (isStopping)
            animator.speed = 0; // Still stop completely when needed
        else
            animator.speed = currentDifficultySpeed; // Restore difficulty-based speed
    }
}
