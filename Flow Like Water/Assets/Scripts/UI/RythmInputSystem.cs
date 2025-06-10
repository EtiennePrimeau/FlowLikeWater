using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum EInputType { Left, Right, LeftHard, RightHard, StraightLeft, StraightRight }

public class RhythmInputSystem : MonoBehaviour
{
    [Header("References")]
    public CanoeController canoe;
    public RhythmUI rhythmUI;
    public Transform perfectSpot; // The zone where timing is perfect
    
    [Header("Position Settings")]
    public float spawnDistance = 300f;
    public float promptInterval = 0.8f;
    public float perfectZoneSize = 50f;
    public float goodZoneSize = 100f;
    
    [Header("Input Settings")]
    public float combinationWindow = 0.2f;
    
    private Dictionary<KeyCode, float> keyPressTimes = new Dictionary<KeyCode, float>();  
    public List<PromptObject> activePrompts = new List<PromptObject>(); // Track actual prefabs
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
        EInputType inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
        
        if (canoe.CurrentState == ECanoeState.hardTurning)
            Debug.Log($"Hard Turn - isRotatingRight: {canoe.isRotatingRight}, InputType: {inputType}");

        Vector3 targetPos = GetTargetZonePosition();
        Vector3 spawnPos = targetPos + canoe.transform.forward * spawnDistance;
        List<PromptObject> newPrompts = new List<PromptObject>();
        
        if (canoe.CurrentState == ECanoeState.hardTurning)
        {
            //EInputType inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
            //Vector3 targetPos = GetTargetZonePosition();
            //Vector3 spawnPos = targetPos + Vector3.up * spawnDistance;
            
            // Spawn prompts and get the GameObjects back
            newPrompts = rhythmUI.ShowPromptHardTurn(inputType, spawnPos, targetPos, 
                lastPromptTime, promptInterval, out bool pressKeyCreated);
            
            activePrompts.AddRange(newPrompts);
            if (pressKeyCreated) lastPromptTime = Time.time;
            
            return;
        }
        
        if (Time.time - lastPromptTime < promptInterval) return;
        
        //EInputType inputType = GetInputTypeFromState(canoe.CurrentState, canoe.isRotatingRight);
        //Vector3 targetPos = GetTargetZonePosition();
        //Vector3 spawnPos = targetPos + Vector3.up * spawnDistance;
        
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
        bool holdingKey = Input.GetKey(holdKey);
        if (Input.GetKeyDown(pressKey))
            keyPressTimes[pressKey] = Time.time;
        
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
            if (promptObj != null && promptObj.inputType == inputType && promptObj.canBePressed)
            {
                float distance = Vector3.Distance(promptObj.transform.position, targetPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrompt = promptObj;
                }
            }
        }
        
        if (closestPrompt != null)
        {
            GuiDebug.Instance.PrintFloat("distance", closestDistance);
            Debug.DrawLine(closestPrompt.transform.position, targetPos, Color.magenta, 10f);
            
            EInputType expectedInput = GetInputTypeFromState(canoe.DelayedState, canoe.isRotatingRightDelayed);
        
            if (inputType == expectedInput)
            {
                if (closestDistance <= perfectZoneSize)
                {
                    rhythmUI.ShowFeedback($"PERFECT! {closestDistance:F1}");
                    Destroy(closestPrompt.gameObject);
                }
                else if (closestDistance <= goodZoneSize)
                {
                    rhythmUI.ShowFeedback($"GOOD! {closestDistance:F1}");
                    Destroy(closestPrompt.gameObject);
                }
                else
                {
                    rhythmUI.ShowFeedback($"TOO FAR! {closestDistance:F1}");
                }
            }
            else
            {
                rhythmUI.ShowFeedback($"WRONG INPUT! {expectedInput}");
            }
        }
    }
    
    void CleanupPrompts()
    {
        // Remove null references (destroyed objects)
        activePrompts.RemoveAll(p => p == null);
        
        // Remove prompts that have fallen too far
        Vector3 targetPos = GetTargetZonePosition();
        activePrompts.RemoveAll(p => p != null && 
            p.transform.position.y < targetPos.y - goodZoneSize);
    }
}
