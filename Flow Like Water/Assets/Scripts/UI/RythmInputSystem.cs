using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum EInputType { Left, Right, LeftHard, RightHard, StraightLeft, StraightRight }

public class RhythmInputSystem : MonoBehaviour
{
    public static RhythmInputSystem Instance;
    
    [Header("References")]
    public CanoeController canoe;
    public RhythmUI rhythmUI;
    public Transform perfectSpot; // The zone where timing is perfect
    public CharacterAnimator frontAnimator;
    public CharacterAnimator backAnimator;
    
    [Header("Position Settings")]
    public float spawnDistance = 300f;
    public float promptInterval = 0.8f;
    public float perfectZoneSize = 50f;
    public float goodZoneSize = 100f;
    public float deleteRadius = 10f;
    
    [Header("Input Settings")]
    public float combinationWindow = 0.2f;
    
    private Dictionary<KeyCode, float> keyPressTimes = new Dictionary<KeyCode, float>();  
    public List<PromptObject> activePrompts = new List<PromptObject>(); // Track actual prefabs
    private float lastPromptTime;
    
    private EInputType expectedStraightInput;
    
    private bool inputChecked = false;
    private float inputCheckTimer;
    private float inputCheckTimerMax = 0.1f;


    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;
    }

    void Update()
    {
        if (canoe == null) return;

        if (GameManager.Instance.currentState == GameState.GameOver)
            return;

        if (inputChecked)
        {
            if (inputCheckTimer > inputCheckTimerMax)
            {
                inputChecked = false;
                inputCheckTimer = 0f;
            }
            else
                inputCheckTimer += Time.deltaTime;
        }
        
        GeneratePrompts();
        HandleInput();
        CleanupPrompts();
    }
    
    public void GeneratePrompts()
    {
        EInputType inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
        
        //if (canoe.CurrentState == ECanoeState.hardTurning)
        //    Debug.Log($"Hard Turn - isRotatingRight: {canoe.isRotatingRight}, InputType: {inputType}");

        Vector3 targetPos = GetTargetZonePosition();
        Vector3 spawnPos = targetPos + canoe.transform.forward * spawnDistance;
        List<PromptObject> newPrompts = new List<PromptObject>();
        
        if (canoe.CurrentState == ECanoeState.hardTurning)
        {
            // Spawn prompts and get the GameObjects back
            newPrompts = rhythmUI.ShowPromptHardTurn(inputType, spawnPos, targetPos, 
                lastPromptTime, promptInterval, out bool pressKeyCreated);
            
            activePrompts.AddRange(newPrompts);
            if (pressKeyCreated) lastPromptTime = Time.time;
            
            return;
        }
        
        if (Time.time - lastPromptTime < promptInterval) return;
        
        // Spawn prompts and get the GameObjects back
        newPrompts = rhythmUI.ShowPrompt(inputType, spawnPos, targetPos);
        
        
        
        
        activePrompts.AddRange(newPrompts);
        lastPromptTime = Time.time;
    }
    
    Vector3 GetTargetZonePosition()
    {
        if (perfectSpot != null)
            return perfectSpot.position;
        
        return canoe.transform.position + Vector3.up * 2f;
    }

    EInputType GetInputTypeFromState(ECanoeState state, bool isRight)
    {
        switch (state)
        {
            case ECanoeState.straight:

                // Generate random prompt for straight state
                EInputType randomStraightInput = (UnityEngine.Random.Range(0, 2) == 0) ? 
                    EInputType.StraightLeft : EInputType.StraightRight;
                expectedStraightInput = randomStraightInput;
                return randomStraightInput; 
                
            case ECanoeState.turning:
                return isRight ? EInputType.Right : EInputType.Left;
            case ECanoeState.hardTurning:
                return isRight ? EInputType.RightHard : EInputType.LeftHard;
            default:
                Debug.LogError("Unknown state: " + canoe.CurrentState);
                return EInputType.StraightLeft;
        }
    }
    
    void HandleInput()
    {
        if (canoe.DelayedState == ECanoeState.hardTurning)
        {
            if (Input.GetKeyDown(KeyCode.V))
                backAnimator.TriggerLeftHold();
            if (Input.GetKeyDown(KeyCode.N))
                backAnimator.TriggerRightHold();
            
            if (Input.GetKeyUp(KeyCode.V))
                backAnimator.TriggerLeftRelease();
            if (Input.GetKeyUp(KeyCode.N))
                backAnimator.TriggerRightRelease();
            
            if (Input.GetKeyDown(KeyCode.R))
                frontAnimator.TriggerLeftPaddle();
            if (Input.GetKeyDown(KeyCode.U))
                frontAnimator.TriggerRightPaddle();   
            
            CheckHardTurnInput(KeyCode.V, KeyCode.U, EInputType.LeftHard);
            CheckHardTurnInput(KeyCode.N, KeyCode.R, EInputType.RightHard);
            return;
        }
        backAnimator.TriggerLeftRelease();
        backAnimator.TriggerRightRelease();
        
        CheckInputCombination(KeyCode.V, KeyCode.U, EInputType.StraightLeft);
        CheckInputCombination(KeyCode.N, KeyCode.R, EInputType.StraightRight);
        CheckInputCombination(KeyCode.U, KeyCode.N, EInputType.Left);
        CheckInputCombination(KeyCode.V, KeyCode.R, EInputType.Right);

        if (Input.GetKeyDown(KeyCode.V))
            backAnimator.TriggerLeftPaddle();
        if (Input.GetKeyDown(KeyCode.R))
            frontAnimator.TriggerLeftPaddle();
        if (Input.GetKeyDown(KeyCode.U))
            frontAnimator.TriggerRightPaddle();
        if (Input.GetKeyDown(KeyCode.N))
            backAnimator.TriggerRightPaddle();
    }

    void CheckInputCombination(KeyCode key1, KeyCode key2, EInputType inputType)
    {
        if (Input.GetKeyDown(key1))
            keyPressTimes[key1] = Time.time;

        if (Input.GetKeyDown(key2))
            keyPressTimes[key2] = Time.time;

    

        if (keyPressTimes.ContainsKey(key1) && keyPressTimes.ContainsKey(key2))
        {
            float timeDiff = Mathf.Abs(keyPressTimes[key1] - keyPressTimes[key2]);
            if (timeDiff <= combinationWindow)
            {
                CheckInput(inputType);
                keyPressTimes.Remove(key1);
                keyPressTimes.Remove(key2);
            }
        }
        
        CleanupOldKeyPresses();
    }

    void CheckHardTurnInput(KeyCode holdKey, KeyCode pressKey, EInputType inputType)
    {
        bool holdingKey = Input.GetKey(holdKey);
        if (Input.GetKeyDown(pressKey))
            keyPressTimes[pressKey] = Time.time;

        if (holdingKey)
        {
            rhythmUI.ShowPopup();
            DeletePromptsOnSuccess(CanoeController.Instance.transform, false);
        }
            
        
        if (keyPressTimes.ContainsKey(pressKey) && holdingKey)
        {
            CheckInput(inputType);
            keyPressTimes.Remove(pressKey);
        }
        
        CleanupOldKeyPresses();
    }

    void CleanupOldKeyPresses()
    {
        var keysToRemove = keyPressTimes.Where(kvp => Time.time - kvp.Value > combinationWindow)
                                       .Select(kvp => kvp.Key).ToList();
        
        foreach (var key in keysToRemove)
            keyPressTimes.Remove(key);
    }
    
    void CheckInput(EInputType inputType)
    {
        PromptObject closestPrompt = null;
        float closestDistance = float.MaxValue;
        Vector3 targetPos = GetTargetZonePosition();
        
        // Find the closest prompt of the right type
        foreach (var promptObj in activePrompts)
        {
            if (promptObj != null && promptObj.canBePressed)
            {
                float distance = Vector3.Distance(promptObj.transform.position, targetPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrompt = promptObj;
                }
            }
        }
        
        if (!inputChecked && closestPrompt != null)
        {
            inputChecked = true;
            //GuiDebug.Instance.PrintFloat("distance", closestDistance);
            //Debug.DrawLine(closestPrompt.transform.position, targetPos, Color.magenta, 10f);
            
            EInputType expectedInput = closestPrompt.inputType;
            //Debug.Log($"Player Input: {inputType}, Prompt Expected: {expectedInput}, Match: {inputType == expectedInput}");

            Vector3 defaultPos = CanoeController.Instance.transform.position + CanoeController.Instance.transform.forward * 5f;

            FeedbackType feedbackType;
            if (inputType == expectedInput)
            {
                if (closestDistance <= perfectZoneSize)
                {
                    feedbackType = FeedbackType.Perfect;
                    rhythmUI.ShowFeedbackPopup(closestPrompt.transform.position,FeedbackType.Perfect, closestDistance);
                    DeletePromptsOnSuccess(closestPrompt.transform, true);
                    GameManager.Instance.IncrementPromptsHit();
                }
                else if (closestDistance <= goodZoneSize)
                {
                    feedbackType = FeedbackType.Good;
                    rhythmUI.ShowFeedbackPopup(closestPrompt.transform.position,FeedbackType.Good, closestDistance);
                    DeletePromptsOnSuccess(closestPrompt.transform, true);
                    GameManager.Instance.IncrementPromptsHit();
                }
                else
                {
                    feedbackType = FeedbackType.Bad;
                    rhythmUI.ShowFeedbackPopup(defaultPos,FeedbackType.Bad, closestDistance);
                    //DeletePromptsOnSuccess(closestPrompt.transform);
                }
            }
            else
            {
                feedbackType = FeedbackType.Wrong;
                rhythmUI.ShowFeedbackPopup(defaultPos, FeedbackType.Wrong, 0, "WRONG INPUT");
            }
            
            HealthSystem.Instance.OnFeedbackReceived(feedbackType);
        }
        else
        {
            //Debug.Log(" blocked     " + inputChecked);
        }
    }

    void DeletePromptsOnSuccess(Transform prompt1, bool pressKeys)
    {
        float radius = pressKeys ? deleteRadius : deleteRadius/2;
    
        List<PromptObject> promptsToDelete = new List<PromptObject>();

        // Check distance to each active prompt directly instead of using Physics
        foreach (PromptObject promptObj in activePrompts.ToList()) // ToList() to avoid modification during enumeration
        {
            if (promptObj != null && promptObj.canBePressed == pressKeys)
            {
                if (promptObj.transform == prompt1)
                    continue;
            
                float distance = Vector3.Distance(prompt1.position, promptObj.transform.position);
                //Debug.Log($"Checking prompt {promptObj.name}: distance = {distance}, radius = {radius}");
            
                if (distance <= radius)
                {
                    promptsToDelete.Add(promptObj);
                    //Debug.Log($"Found prompt to delete within radius: {promptObj.name}");
                    break; // Only delete one prompt
                }
            }
        }

        // Delete the found prompts
        foreach (PromptObject prompt in promptsToDelete)
        {
            activePrompts.Remove(prompt);
        
            if (prompt.gameObject != null)
            {
                float distance = Vector3.Distance(prompt1.transform.position, prompt.transform.position);
                //Debug.Log($"Deleting additional prompt. Distance: {distance}, Radius: {radius}");
                GameManager.Instance.IncrementPromptsHit();
                Destroy(prompt.gameObject);
            }
        }
    
        if (pressKeys)
            Destroy(prompt1.gameObject);
    }

    
    
    // New method to get expected input type based on delayed state
    EInputType GetExpectedInputType()
    {
        switch (canoe.DelayedState)
        {
            case ECanoeState.straight:
                // For straight state, use the stored expected input from when prompt was generated
                return expectedStraightInput;
                
            case ECanoeState.turning:
                return canoe.isRotatingRightDelayed ? EInputType.Right : EInputType.Left;
                
            case ECanoeState.hardTurning:
                return canoe.isRotatingRightDelayed ? EInputType.RightHard : EInputType.LeftHard;
                
            default:
                return EInputType.StraightLeft;
        }
    }
    void CleanupPrompts()
    {
        
        // Remove null references (destroyed objects)
        int nullCount = activePrompts.RemoveAll(p => p == null);
        //if (nullCount > 0)
        //{
        //    Debug.Log($"[CleanupPrompts] Removed {nullCount} null prompt references");
        //}
    }

}
