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
        // Rowing
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKeyDown(KeyCode.P))
        {
            CheckInput(EInputType.StraightLeft);
        }
        // Rowing
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.I))
        {
            CheckInput(EInputType.StraightRight);
        }
        
        // Turning left
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.P))
        {
            CheckInput(EInputType.Left);
        }
        // Turning right
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKeyDown(KeyCode.I))
        {
            CheckInput(EInputType.Right);
        }
        
        // Turning left hard
        if (Input.GetKey(KeyCode.Z) && Input.GetKeyDown(KeyCode.P))
        {
            CheckInput(EInputType.LeftHard);
        }
        // Turning right hard
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.P))
        {
            CheckInput(EInputType.RightHard);
        }
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