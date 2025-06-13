using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RhythmUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform promptContainer;
    public GameObject promptPrefab;
    public GameObject feedbackPopupPrefab;
    
    private float lastSpawnTimeHold;
    
    

    public List<PromptObject> ShowPromptHardTurn(EInputType inputType, Vector3 spawnPos, Vector3 targetPos, 
        float lastPromptTime, float promptInterval, out bool pressKeyCreated)
    {
        var prompts = new List<PromptObject>();
        bool isLeft = inputType == EInputType.LeftHard;
        pressKeyCreated = false;
        
        // Always create first prompt
        if (Time.time - lastSpawnTimeHold > 0.05f)
        {
            GameObject promptObj1 = Instantiate(promptPrefab, promptContainer);
            var prompt1 = SetupPromptObject(promptObj1, inputType, spawnPos, targetPos, !isLeft);
            prompt1.canBePressed = false;
            prompts.Add(prompt1);
            lastSpawnTimeHold = Time.time;
        }

        //Debug.Log(Time.time + "  " + lastPromptTime);
        //Debug.Log(Time.time - lastPromptTime);
        // Create second prompt if enough time has passed
        if (Time.time - lastPromptTime > promptInterval)
        {
            GameObject promptObj2 = Instantiate(promptPrefab, promptContainer);
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
            
            GameObject promptObj1 = Instantiate(promptPrefab, promptContainer);
            GameObject promptObj2 = Instantiate(promptPrefab, promptContainer);
                
            prompts.Add(SetupPromptObject(promptObj1, inputType, spawnPos, targetPos, isLeft, true));
            prompts.Add(SetupPromptObject(promptObj2, inputType, spawnPos, targetPos, isLeft, false));
        }
        else
        {
            GameObject promptObjL = Instantiate(promptPrefab, promptContainer);
            GameObject promptObjR = Instantiate(promptPrefab, promptContainer);
            
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
        promptObj.transform.position = new Vector3(promptObj.transform.position.x,
                                                    0f,
                                                    promptObj.transform.position.z);

        promptComponent.SetVisuals(inputType, isRight, isFront);
        
        // Setup visual text
        //SetupPromptVisual(promptObj, inputType, isRight);
        
        return promptComponent;
    }
    
    public void ShowPopup()
    {
        Transform t = CanoeController.Instance.transform;
        if (feedbackPopupPrefab == null) return;
        
        // Instantiate popup
        GameObject popupObj = Instantiate(feedbackPopupPrefab, t.position + (t.forward + t.up) * 3f,
            Quaternion.LookRotation(CanoeController.Instance.transform.forward), promptContainer);
        FeedbackPopup popup = popupObj.GetComponent<FeedbackPopup>();

        if (popup != null)
        {
            popup.Initialize(FeedbackType.Hold, "HOLD IT");
            HealthSystem.Instance.OnFeedbackReceived(FeedbackType.Hold);
        }
    }
    
    public void ShowFeedbackPopup(Vector3 spawnPosition, FeedbackType type, float distance = 0f, string customMessage = "")
    {
        if (feedbackPopupPrefab == null) return;
        
        // Instantiate popup
        GameObject popupObj = Instantiate(feedbackPopupPrefab, spawnPosition + CanoeController.Instance.transform.forward * 5f,
                                            Quaternion.LookRotation(CanoeController.Instance.transform.forward), promptContainer);
        FeedbackPopup popup = popupObj.GetComponent<FeedbackPopup>();
        
        if (popup != null)
        {
            // Initialize the popup based on feedback type
            if (type == FeedbackType.Wrong ||  type == FeedbackType.Miss)
                popup.Initialize(type, customMessage);
            else
                popup.Initialize(type, "", distance);
        }
    }
}
