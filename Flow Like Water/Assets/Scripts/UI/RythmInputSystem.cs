using System;
using UnityEngine;
using System.Collections.Generic;

public enum EInputType { Left, Right, LeftHard, RightHard, StraightLeft, StraightRight }

public class RhythmInputSystem : MonoBehaviour
{
    [Header("References")]
    public CanoeController canoe;
    public RhythmUI rhythmUI;
    public Transform targetZone; // The zone where timing is perfect
    
    [Header("Position Settings")]
    public float spawnDistance = 300f;     // How far above target zone prompts spawn
    public float promptInterval = 0.8f;
    public float perfectZoneSize = 50f;    // Size of perfect timing zone
    public float goodZoneSize = 100f;      // Size of good timing zone
    
    [Header("Input Settings")]
    public float combinationWindow = 0.2f;
    
    private Dictionary<KeyCode, float> keyPressTimes = new Dictionary<KeyCode, float>();  
    private List<InputPrompt> activePrompts = new List<InputPrompt>();
    private float lastPromptTime;
    
    void Update()
    {
        if (canoe == null) return;
        
        GeneratePrompts();
        HandleInput();
        CleanupPrompts();
    }
    
    void GeneratePrompts()
    {
        if (Time.time - lastPromptTime < promptInterval) return;
        
        InputPrompt prompt = new InputPrompt();
        prompt.inputType = GetInputTypeFromCurrentState();
        prompt.targetPosition = GetTargetZonePosition();
        prompt.spawnPosition = prompt.targetPosition + Vector3.up * spawnDistance;
        
        activePrompts.Add(prompt);
        rhythmUI.ShowPrompt(prompt);
        lastPromptTime = Time.time;
    }
    
    Vector3 GetTargetZonePosition()
    {
        if (targetZone != null)
            return targetZone.position;
        
        // Fallback: use canoe position with offset
        return canoe.transform.position + Vector3.up * 2f;
    }

    EInputType GetInputTypeFromCurrentState()
    {
        bool isRight = canoe.isRotatingRight;
        switch (canoe.CurrentState)
        {
            case ECanoeState.straight:
                return isRight ? EInputType.StraightRight : EInputType.StraightLeft;
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
        CheckInputCombination(KeyCode.Z, KeyCode.P, EInputType.StraightLeft);
        CheckInputCombination(KeyCode.C, KeyCode.I, EInputType.StraightRight);
        CheckInputCombination(KeyCode.C, KeyCode.P, EInputType.Left);
        CheckInputCombination(KeyCode.Z, KeyCode.I, EInputType.Right);
        
        CheckHoldAndPress(KeyCode.Z, KeyCode.P, EInputType.LeftHard);
        CheckHoldAndPress(KeyCode.C, KeyCode.I, EInputType.RightHard);
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

    void CheckHoldAndPress(KeyCode holdKey, KeyCode pressKey, EInputType inputType)
    {
        if (Input.GetKey(holdKey) && Input.GetKeyDown(pressKey))
        {
            CheckInput(inputType);
        }
    }

    void CleanupOldKeyPresses()
    {
        var keysToRemove = new List<KeyCode>();
        foreach (var kvp in keyPressTimes)
        {
            if (Time.time - kvp.Value > combinationWindow)
                keysToRemove.Add(kvp.Key);
        }
        
        foreach (var key in keysToRemove)
            keyPressTimes.Remove(key);
    }
    
    void CheckInput(EInputType inputType)
    {
        Debug.Log("Checking input: " + inputType);
        
        InputPrompt closestPrompt = null;
        float closestDistance = float.MaxValue;
        
        // Find the closest prompt of the right type to the target zone
        foreach (var prompt in activePrompts)
        {
            if (prompt.inputType == inputType && prompt.isActive)
            {
                float distance = Vector3.Distance(prompt.currentPosition, prompt.targetPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrompt = prompt;
                }
            }
        }
        
        if (closestPrompt != null)
        {
            string feedback;
            if (closestDistance <= perfectZoneSize)
            {
                feedback = "PERFECT!";
                rhythmUI.ShowFeedback(feedback + " " + closestDistance.ToString("F1"));
            }
            else if (closestDistance <= goodZoneSize)
            {
                feedback = "GOOD!";
                rhythmUI.ShowFeedback(feedback + " " + closestDistance.ToString("F1"));
            }
            else
            {
                feedback = "MISS";
                rhythmUI.ShowFeedback(feedback + " " + closestDistance.ToString("F1"));
                return; // Don't remove the prompt for misses
            }
            
            closestPrompt.isActive = false;
        }
        else
        {
            rhythmUI.ShowFeedback("NO PROMPT");
        }
    }
    
    void CleanupPrompts()
    {
        // Remove prompts that have passed too far below the target zone
        activePrompts.RemoveAll(p => p.currentPosition.y < p.targetPosition.y - goodZoneSize);
    }
}

[System.Serializable]
public class InputPrompt
{
    public EInputType inputType;
    public Vector3 targetPosition;   // Where perfect timing occurs
    public Vector3 spawnPosition;    // Where prompt starts
    public Vector3 currentPosition;  // Current position (updated by UI)
    public bool isActive = true;
}
