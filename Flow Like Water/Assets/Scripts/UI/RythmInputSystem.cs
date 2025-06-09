using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class RhythmInputSystem : MonoBehaviour
{
    [Header("References")]
    public CanoeController canoe;
    public RhythmUI rhythmUI;
    
    [Header("Timing Settings")]
    public float lookAheadTime = 2f;        // How far ahead to generate prompts
    public float promptInterval = 0.5f;     // How often to check for new prompts
    
    [Header("Input Mapping")]
    public KeyCode leftPaddleKey = KeyCode.A;
    public KeyCode rightPaddleKey = KeyCode.D;
    public KeyCode syncKey = KeyCode.S;
    public KeyCode powerKey = KeyCode.Space;
    
    private List<InputPrompt> activePrompts = new List<InputPrompt>();
    private float lastPromptTime;
    private ECanoeState lastState = ECanoeState.straight;
    
    void Update()
    {
        if (canoe == null || rhythmUI == null) return;
        
        // Generate new prompts based on canoe state
        GeneratePrompts();
        
        // Handle player input
        HandleInput();
        
        // Update active prompts
        UpdatePrompts();
    }
    
    void GeneratePrompts()
    {
        if (Time.time - lastPromptTime < promptInterval) return;
        
        ECanoeState currentState = canoe.CurrentState;
        
        // Generate prompts based on state changes or continued states
        if (currentState != lastState || ShouldGenerateContinuousPrompt(currentState))
        {
            InputPrompt newPrompt = CreatePromptForState(currentState);
            if (newPrompt != null)
            {
                activePrompts.Add(newPrompt);
                rhythmUI.ShowInputPrompt(newPrompt);
                lastPromptTime = Time.time;
            }
        }
        
        lastState = currentState;
    }
    
    bool ShouldGenerateContinuousPrompt(ECanoeState state)
    {
        // Generate continuous prompts for turning states
        return state == ECanoeState.turning || state == ECanoeState.hardTurning;
    }
    
    InputPrompt CreatePromptForState(ECanoeState state)
    {
        switch (state)
        {
            case ECanoeState.straight:
                return new InputPrompt(EInputType.Sync, Time.time + lookAheadTime);
                
            case ECanoeState.turning:
                // Choose left or right based on canoe's turning direction
                EInputType turnType = canoe.isRotatingRight ? EInputType.RightPaddle : EInputType.LeftPaddle;
                return new InputPrompt(turnType, Time.time + lookAheadTime);
                
            case ECanoeState.hardTurning:
                return new InputPrompt(EInputType.Power, Time.time + lookAheadTime);
                
            default:
                return null;
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(leftPaddleKey))
            ProcessInput(EInputType.LeftPaddle);
        if (Input.GetKeyDown(rightPaddleKey))
            ProcessInput(EInputType.RightPaddle);
        if (Input.GetKeyDown(syncKey))
            ProcessInput(EInputType.Sync);
        if (Input.GetKeyDown(powerKey))
            ProcessInput(EInputType.Power);
    }
    
    void ProcessInput(EInputType inputType)
    {
        InputPrompt bestMatch = null;
        float bestScore = float.MaxValue;
        
        // Find the closest matching prompt
        foreach (var prompt in activePrompts)
        {
            if (prompt.inputType == inputType && prompt.isActive)
            {
                float timeDiff = Mathf.Abs(Time.time - prompt.timing);
                if (timeDiff < bestScore)
                {
                    bestScore = timeDiff;
                    bestMatch = prompt;
                }
            }
        }
        
        if (bestMatch != null)
        {
            ScoreInput(bestMatch, bestScore);
            bestMatch.isActive = false;
            rhythmUI.HideInputPrompt(bestMatch);
        }
    }
    
    void ScoreInput(InputPrompt prompt, float timeDiff)
    {
        string result;
        if (timeDiff <= prompt.perfectWindow)
        {
            result = "PERFECT!";
            rhythmUI.ShowFeedback(result, Color.yellow);
        }
        else if (timeDiff <= prompt.goodWindow)
        {
            result = "GOOD!";
            rhythmUI.ShowFeedback(result, Color.green);
        }
        else
        {
            result = "MISS";
            rhythmUI.ShowFeedback(result, Color.red);
        }
        
        Debug.Log($"Input: {prompt.inputType} - {result} (diff: {timeDiff:F3}s)");
    }
    
    void UpdatePrompts()
    {
        // Remove expired prompts
        for (int i = activePrompts.Count - 1; i >= 0; i--)
        {
            var prompt = activePrompts[i];
            if (Time.time > prompt.timing + prompt.goodWindow)
            {
                if (prompt.isActive)
                {
                    rhythmUI.ShowFeedback("MISS", Color.red);
                    rhythmUI.HideInputPrompt(prompt);
                }
                activePrompts.RemoveAt(i);
            }
        }
    }
}


public enum EInputType 
{ 
    LeftPaddle,    // For turning left
    RightPaddle,   // For turning right
    Sync,          // For staying straight
    Power          // For hard turns
}

[System.Serializable]
public class InputPrompt
{
    public EInputType inputType;
    public float timing;           // When to show the prompt (seconds ahead)
    public float perfectWindow;    // Perfect timing window
    public float goodWindow;       // Good timing window
    public bool isActive;
    
    public InputPrompt(EInputType type, float time)
    {
        inputType = type;
        timing = time;
        perfectWindow = 0.1f;
        goodWindow = 0.3f;
        isActive = true;
    }
}