using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RhythmUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform promptLeftContainer;
    public Transform promptRightContainer;
    public GameObject promptPrefab;
    public TextMeshProUGUI feedbackText;
    
    [Header("Visual Settings")]
    public float fallSpeed = 150f; // Units per second
    public Transform targetZoneVisual; // Optional: visual indicator for target zone

    public float ShowPromptHardTurn(InputPrompt prompt, float lastPromptTime, float promptInterval, bool canoeRight)
    {
        bool isLeft = prompt.inputType == EInputType.LeftHard;
        float lastPromptTimeLocal = lastPromptTime;
            
        GameObject promptObj1 = Instantiate(promptPrefab, 
            isLeft  ? promptLeftContainer : promptRightContainer);
        GameObject promptObj2 = null;
        
        if (Time.time - lastPromptTime > promptInterval)
        {
            promptObj2 = Instantiate(promptPrefab, 
                !isLeft  ? promptLeftContainer : promptRightContainer);
            lastPromptTimeLocal = Time.time;
        }
        
        if (!canoeRight)
            SetupPrompt(promptObj2, promptObj1, prompt);
        else
            SetupPrompt(promptObj1, promptObj2, prompt);
        StartCoroutine(AnimatePrompt(promptObj1, prompt));
        if (promptObj2 != null)
            StartCoroutine(AnimatePrompt(promptObj2, prompt));

        return lastPromptTimeLocal;
    }
    
    public void ShowPrompt(InputPrompt prompt)
    {
        if (prompt.inputType == EInputType.LeftHard ||
            prompt.inputType == EInputType.RightHard)
        {
            Debug.Log("Hard Turn return");
            return;
        }
        
        if (prompt.inputType == EInputType.Left ||
            prompt.inputType == EInputType.Right)
        {
            bool isLeft = prompt.inputType == EInputType.Left;
            
            GameObject promptObj1 = Instantiate(promptPrefab, 
                isLeft  ? promptLeftContainer : promptRightContainer);
            GameObject promptObj2 = Instantiate(promptPrefab, 
                isLeft  ? promptLeftContainer : promptRightContainer);
            SetupPrompt(promptObj1, promptObj2, prompt);
            StartCoroutine(AnimatePrompt(promptObj1, prompt));
            StartCoroutine(AnimatePrompt(promptObj2, prompt));
        }
        else
        {
            GameObject promptObjL = Instantiate(promptPrefab, promptLeftContainer);
            GameObject promptObjR = Instantiate(promptPrefab, promptRightContainer);
            SetupPrompt(promptObjL, promptObjR, prompt);
            StartCoroutine(AnimatePrompt(promptObjL, prompt));
            StartCoroutine(AnimatePrompt(promptObjR, prompt));
        }
        
        
    }
    
    void SetupPrompt(GameObject promptObjLeft, GameObject promptObjRight, InputPrompt prompt)
    {
        TextMeshProUGUI textL = null;
        TextMeshProUGUI textR = null;
        
        if (promptObjLeft)
            textL = promptObjLeft.GetComponentInChildren<TextMeshProUGUI>();
        if (promptObjRight)
            textR = promptObjRight.GetComponentInChildren<TextMeshProUGUI>();
        
        switch (prompt.inputType)
        {
            case EInputType.StraightLeft:
                if (textL)
                {
                    textL.text = "Z";
                    textL.color = Color.blue;
                }
                if (textR)
                {
                    textR.text = "P";
                    textR.color = Color.blue;
                }
                break;
            case EInputType.StraightRight:
                if (textL)
                {
                    textL.text = "C";
                    textL.color = Color.blue;
                }
                if (textR)
                {
                    textR.text = "I";
                    textR.color = Color.blue;
                }
                break;
            case EInputType.Left:
                if (textL)
                {
                    textL.text = "C";
                    textL.color = Color.green;
                }
                if (textR)
                {
                    textR.text = "P";
                    textR.color = Color.green;
                }
                break;
            case EInputType.Right:
                if (textL)
                {
                    textL.text = "Z";
                    textL.color = Color.yellow;
                }
                if (textR)
                {
                    textR.text = "I";
                    textR.color = Color.yellow;
                }
                break;
            case EInputType.LeftHard:
                if (textL)
                {
                    textL.text = "Z";
                    textL.color = Color.red;
                }
                if (textR)
                {
                    textR.text = "P";
                    textR.color = Color.red;
                }
                break;
            case EInputType.RightHard:
                if (textL)
                {
                    textL.text = "I";
                    textL.color = Color.red;
                }
                if (textR)
                {
                    textR.text = "C";
                    textR.color = Color.red;
                }
                break;

        }
    }
    
    IEnumerator AnimatePrompt(GameObject promptObj, InputPrompt prompt)
    {
        RectTransform rect = promptObj.GetComponent<RectTransform>();
        
        // Set initial position
        prompt.currentPosition = prompt.spawnPosition;
        rect.anchoredPosition = WorldToScreenPoint(prompt.spawnPosition);
        
        while (prompt.isActive && prompt.currentPosition.y > prompt.targetPosition.y - 200f)
        {
            // Move prompt down
            prompt.currentPosition += Vector3.down * fallSpeed * Time.deltaTime;
            rect.anchoredPosition = WorldToScreenPoint(prompt.currentPosition);
            
            yield return null;
        }
        
        Destroy(promptObj);
    }
    
    Vector2 WorldToScreenPoint(Vector3 worldPos)
    {
        // Convert 3D world position to 2D screen position
        // You might need to adjust this based on your camera setup
        return new Vector2(0f, worldPos.y - 200f); // Simple Y-axis mapping
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
