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
    
    public void GeneratePrompts()
    {
        if (canoe.CurrentState == ECanoeState.hardTurning)
        {
            InputPrompt hprompt = new InputPrompt();
            hprompt.inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
            hprompt.targetPosition = GetTargetZonePosition();
            hprompt.spawnPosition = hprompt.targetPosition + Vector3.up * spawnDistance;
        
            activePrompts.Add(hprompt);

            InputPrompt newPrompt;
            lastPromptTime = rhythmUI.ShowPromptHardTurn(out newPrompt, hprompt, lastPromptTime, promptInterval, canoe.isRotatingRight);
            if (newPrompt != null)
            {
                activePrompts.Add(newPrompt);
            }
            
            return;
        }
        
        if (Time.time - lastPromptTime < promptInterval) return;
        
        InputPrompt prompt = new InputPrompt();
        prompt.inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
        prompt.targetPosition = GetTargetZonePosition();
        prompt.spawnPosition = prompt.targetPosition + Vector3.up * spawnDistance;
        prompt.isPressKey = true;
        
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

    EInputType GetInputTypeFromState(ECanoeState state, bool isRight)
    {
        //bool isRight = canoe.isRotatingRight;
        switch (state)
        {
            case ECanoeState.straight:
                return isRight ? EInputType.StraightRight : EInputType.StraightLeft;
            case ECanoeState.turning:
                return isRight ? EInputType.Right : EInputType.Left;
            case ECanoeState.hardTurning:
                return isRight ? EInputType.LeftHard : EInputType.RightHard;
            default:
                Debug.LogError("Unknown state: " + canoe.CurrentState);
                return EInputType.StraightLeft;
        }
    }
    
    void HandleInput()
    {
        if (canoe.DelayedState == ECanoeState.hardTurning)
        {
            CheckHardTurnInput(KeyCode.Z, KeyCode.P, EInputType.LeftHard);
            CheckHardTurnInput(KeyCode.C, KeyCode.I, EInputType.RightHard);
            return;
        }
        
        CheckInputCombination(KeyCode.Z, KeyCode.P, EInputType.StraightLeft);
        CheckInputCombination(KeyCode.C, KeyCode.I, EInputType.StraightRight);
        CheckInputCombination(KeyCode.C, KeyCode.P, EInputType.Left);
        CheckInputCombination(KeyCode.Z, KeyCode.I, EInputType.Right);
        
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
        //Debug.Log("Checking hard turn");
        bool holdingKey = false;
        if (Input.GetKey(holdKey))
        {
            holdingKey = true;
            //Debug.Log("holding");
        }
        if (Input.GetKeyDown(pressKey))
        {
            keyPressTimes[pressKey] = Time.time;
        }
        
        if (keyPressTimes.ContainsKey(pressKey) && holdingKey)
        {
            CheckInput(inputType, pressKey);
            keyPressTimes.Remove(pressKey);
        }
        
        CleanupOldKeyPresses();
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
    
    void CheckInput(EInputType inputType,  KeyCode key = default)
    {
        //Debug.Log("Checking input: " + inputType);
        
        InputPrompt closestPrompt = null;
        float closestDistance = float.MaxValue;
        
        // Find the closest prompt of the right type to the target zone
        foreach (var prompt in activePrompts)
        {
            
            if (prompt.inputType == inputType && prompt.isActive)
            {
                if (!prompt.isPressKey)
                {
                    //Debug.Log("blocked");
                    continue;
                }
                //Debug.Log("isPressKey");
                
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
            GuiDebug.Instance.PrintFloat("distance", closestDistance);
            Debug.Log("current : " + closestPrompt.currentPosition);
            Debug.Log("target : " + closestPrompt.targetPosition);
            Debug.DrawLine(closestPrompt.currentPosition, closestPrompt.targetPosition, Color.magenta, 10f);
            
            EInputType expectedInput = GetInputTypeFromState(canoe.DelayedState, canoe.isRotatingRightDelayed);
        
            if (inputType == expectedInput)
            {
                if (closestDistance <= perfectZoneSize)
                {
                    rhythmUI.ShowFeedback("PERFECT!" + (float)Math.Round(closestDistance, 1));
                    closestPrompt.isActive = false;
                }
                else if (closestDistance <= goodZoneSize)
                {
                    rhythmUI.ShowFeedback("GOOD!" + (float)Math.Round(closestDistance, 1));
                    closestPrompt.isActive = false;
                }
                else
                {
                    rhythmUI.ShowFeedback("TOO FAR!" + (float)Math.Round(closestDistance, 1));
                }
            }
            else
            {
                rhythmUI.ShowFeedback("WRONG INPUT! " + expectedInput);
                Debug.Log("WRONG INPUT :: expected : " + expectedInput + " and input : " + inputType);
            }
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
    public bool isPressKey = false;
}
