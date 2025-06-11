using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RhythmUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform promptLeftContainer;
    public Transform promptRightContainer;
    public GameObject promptPrefab;
    public TextMeshProUGUI feedbackText;

    //public float fallSpeed;
    
    // Frame counter for hard turn hold key spawning
    private static int hardTurnFrameCounter = 0;
    private const int HARD_TURN_SPAWN_INTERVAL = 30; // Spawn every 5th frame

    private float lastSpawnTimeHold;
    private void Start()
    {
        //fallSpeed = promptPrefab.GetComponent<PromptObject>().GetSpeed();
    }

    public List<PromptObject> ShowPromptHardTurn(EInputType inputType, Vector3 spawnPos, Vector3 targetPos, 
        float lastPromptTime, float promptInterval, out bool pressKeyCreated)
    {
        var prompts = new List<PromptObject>();
        bool isLeft = inputType == EInputType.LeftHard;
        pressKeyCreated = false;
        
        // Always create first prompt
        if (Time.time - lastSpawnTimeHold > 0.1f)
        {
            GameObject promptObj1 = Instantiate(promptPrefab, 
                        isLeft ? promptLeftContainer : promptRightContainer);
            var prompt1 = SetupPromptObject(promptObj1, inputType, spawnPos, targetPos, !isLeft);
            prompts.Add(prompt1);
            lastSpawnTimeHold = Time.time;
        }
        
        
        
        //hardTurnFrameCounter++;
        // Only create first prompt (hold key) every 5th frame
        //if (hardTurnFrameCounter % HARD_TURN_SPAWN_INTERVAL == 0)
        //{
        //    GameObject promptObj1 = Instantiate(promptPrefab, 
        //        isLeft ? promptLeftContainer : promptRightContainer);
        //    var prompt1 = SetupPromptObject(promptObj1, inputType, spawnPos, targetPos, !isLeft);
        //    prompts.Add(prompt1);
        //}

        //Debug.Log(Time.time + "  " + lastPromptTime);
        //Debug.Log(Time.time - lastPromptTime);
        // Create second prompt if enough time has passed
        if (Time.time - lastPromptTime > promptInterval)
        {
            Debug.Log("here " + isLeft);
            GameObject promptObj2 = Instantiate(promptPrefab, 
                isLeft ? promptRightContainer : promptLeftContainer);
            var prompt2 = SetupPromptObject(promptObj2, inputType, spawnPos, targetPos, isLeft);
            prompt2.canBePressed = true;
            prompts.Add(prompt2);
            pressKeyCreated = true;
        }
        
        return prompts;
    }
    
    public List<PromptObject> ShowPrompt(EInputType inputType, Vector3 spawnPos, Vector3 targetPos)
    {
        var prompts = new List<PromptObject>();
        
        if (inputType == EInputType.LeftHard || inputType == EInputType.RightHard)
            return prompts; // Handle in ShowPromptHardTurn
        
        // Create prompts based on input type
        if (inputType == EInputType.Left || inputType == EInputType.Right)
        {
            bool isLeft = inputType == EInputType.Left;
            
            GameObject promptObj1 = Instantiate(promptPrefab, 
                isLeft ? promptRightContainer : promptLeftContainer);
            GameObject promptObj2 = Instantiate(promptPrefab, 
                isLeft ? promptRightContainer : promptLeftContainer);
                
            prompts.Add(SetupPromptObject(promptObj1, inputType, spawnPos, targetPos, isLeft, true));
            prompts.Add(SetupPromptObject(promptObj2, inputType, spawnPos, targetPos, isLeft, false));
        }
        else
        {
            GameObject promptObjL = Instantiate(promptPrefab, promptLeftContainer);
            GameObject promptObjR = Instantiate(promptPrefab, promptRightContainer);
            
            prompts.Add(SetupPromptObject(promptObjL, inputType, spawnPos, targetPos, false));
            prompts.Add(SetupPromptObject(promptObjR, inputType, spawnPos, targetPos, true));
        }
        
        return prompts;
    }
    
    PromptObject SetupPromptObject(GameObject promptObj, EInputType inputType, Vector3 spawnPos, Vector3 targetPos, bool isRight, bool isFront = false)
    {
        // Add PromptObject component
        PromptObject promptComponent = promptObj.GetComponent<PromptObject>();
        if (promptComponent == null)
            promptComponent = promptObj.AddComponent<PromptObject>();
            
        promptComponent.inputType = inputType;
        promptComponent.targetPosition = targetPos;
        promptComponent.canBePressed = true;
        
        // Set initial position
        
        float strength = isRight ? 1 : -1;
        strength *= PromptObject.OffsetStrength;
        Vector3 offset = CanoeController.Instance.transform.right * strength;
        
        switch (inputType)
        {
            case EInputType.StraightLeft:
                break;
            case EInputType.StraightRight:
                break;
            case EInputType.Left:
                if (isFront)
                    offset *= 2f;
                break;
            case EInputType.Right:
                if (isFront)
                    offset *= 2f;
                break;
            case EInputType.LeftHard:
                break;
            case EInputType.RightHard:
                break;
        }
        
        
        //Vector3 offset = CanoeController.Instance.transform.right * -PromptObject.OffsetStrength;
        //if (isRight)
        //    offset = CanoeController.Instance.transform.right * PromptObject.OffsetStrength;
        promptObj.transform.position = spawnPos + offset;

        promptComponent.SetVisuals(inputType, isRight, isFront);
        
        // Setup visual text
        //SetupPromptVisual(promptObj, inputType, isRight);
        
        return promptComponent;
    }
    
    void SetupPromptVisual(GameObject promptObj, EInputType inputType, bool isRight)
    {
        var text = promptObj.GetComponent<TextMeshProUGUI>();
        if (text == null) return;
        
        
        switch (inputType)
        {
            case EInputType.StraightLeft:
                text.text = !isRight ? "Z" : "P";
                text.color = Color.blue;
                break;
            case EInputType.StraightRight:
                text.text = !isRight ? "C" : "I";
                text.color = Color.blue;
                break;
            case EInputType.Left:
                text.text = !isRight ? "C" : "P";
                text.color = Color.green;
                break;
            case EInputType.Right:
                text.text = !isRight ? "Z" : "I";
                text.color = Color.yellow;
                break;
            case EInputType.LeftHard:
                text.text = !isRight ? "Z" : "P";
                text.color = Color.red;
                break;
            case EInputType.RightHard:
                text.text = !isRight ? "I" : "C";
                text.color = Color.red;
                break;
        }
    }
    
    public void ShowFeedback(string message)
    {
        if (feedbackText != null)
            StartCoroutine(ShowFeedbackCoroutine(message));
    }
    
    IEnumerator ShowFeedbackCoroutine(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        feedbackText.gameObject.SetActive(false);
    }
}
