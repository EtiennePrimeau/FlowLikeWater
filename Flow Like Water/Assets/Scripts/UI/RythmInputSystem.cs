using UnityEngine;
using System.Collections.Generic;

public enum EInputType { Left, Right, LeftHard, RightHard, StraightLeft, StraightRight }

public class RhythmInputSystem : MonoBehaviour
{
    [Header("References")]
    public CanoeController canoe;
    public RhythmUI rhythmUI;
    
    [Header("Settings")]
    public float lookAheadTime = 2f;
    public float promptInterval = 0.8f;
    
    [Header("Input Settings")] public float combinationWindow = 0.2f; // Time window for key combinations
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
        prompt.hitTime = Time.time + lookAheadTime;
        
        activePrompts.Add(prompt);
        rhythmUI.ShowPrompt(prompt);
        lastPromptTime = Time.time;
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
        // Check for key combinations with a small timing window
        CheckInputCombination(KeyCode.Z, KeyCode.P, EInputType.StraightLeft);
        CheckInputCombination(KeyCode.C, KeyCode.I, EInputType.StraightRight);
        CheckInputCombination(KeyCode.C, KeyCode.P, EInputType.Left);
        CheckInputCombination(KeyCode.Z, KeyCode.I, EInputType.Right);
    
        // Hard turns - hold first key, press second
        CheckHoldAndPress(KeyCode.Z, KeyCode.P, EInputType.LeftHard);
        CheckHoldAndPress(KeyCode.C, KeyCode.I, EInputType.RightHard);
    }

    void CheckInputCombination(KeyCode key1, KeyCode key2, EInputType inputType)
    {
        // Track when keys are pressed
        if (Input.GetKeyDown(key1))
            keyPressTimes[key1] = Time.time;
        if (Input.GetKeyDown(key2))
            keyPressTimes[key2] = Time.time;
    
        // Check if both keys were pressed within the time window
        if (keyPressTimes.ContainsKey(key1) && keyPressTimes.ContainsKey(key2))
        {
            float timeDiff = Mathf.Abs(keyPressTimes[key1] - keyPressTimes[key2]);
            if (timeDiff <= combinationWindow)
            {
                CheckInput(inputType);
                // Clear the tracked times to prevent repeated triggers
                keyPressTimes.Remove(key1);
                keyPressTimes.Remove(key2);
            }
        }
    
        // Clean up old key presses
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
        foreach (var prompt in activePrompts)
        {
            if (prompt.inputType == inputType && prompt.isActive)
            {
                float timeDiff = Mathf.Abs(Time.time - prompt.hitTime);
                if (timeDiff < 0.3f)
                {
                    string feedback = timeDiff < 0.1f ? "PERFECT!" : "GOOD!";
                    rhythmUI.ShowFeedback(feedback);
                    prompt.isActive = false;
                    return;
                }
            }
        }
        rhythmUI.ShowFeedback("MISS");
    }
    
    void CleanupPrompts()
    {
        activePrompts.RemoveAll(p => Time.time > p.hitTime + 0.5f);
    }
}


[System.Serializable]
public class InputPrompt
{
    public EInputType inputType;
    public float hitTime;
    public bool isActive = true;
}