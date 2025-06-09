using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class RhythmUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform promptContainer;     // Parent for input prompts
    public GameObject promptPrefab;       // Prefab for input prompts
    public TextMeshProUGUI feedbackText;  // For showing PERFECT/GOOD/MISS
    
    [Header("Animation Settings")]
    public float promptTravelTime = 2f;   // How long prompts take to reach target
    public float feedbackDuration = 1f;   // How long feedback shows
    
    private Dictionary<InputPrompt, GameObject> activePromptObjects = new Dictionary<InputPrompt, GameObject>();
    
    public void ShowInputPrompt(InputPrompt prompt)
    {
        if (promptPrefab == null || promptContainer == null) return;
        
        GameObject promptObj = Instantiate(promptPrefab, promptContainer);
        activePromptObjects[prompt] = promptObj;
        
        // Setup prompt visuals based on input type
        SetupPromptVisuals(promptObj, prompt.inputType);
        
        // Start animation
        StartCoroutine(AnimatePrompt(promptObj, prompt));
    }
    
    void SetupPromptVisuals(GameObject promptObj, EInputType inputType)
    {
        Image image = promptObj.GetComponent<Image>();
        TextMeshProUGUI text = promptObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (image != null)
        {
            switch (inputType)
            {
                case EInputType.LeftPaddle:
                    image.color = Color.blue;
                    break;
                case EInputType.RightPaddle:
                    image.color = Color.red;
                    break;
                case EInputType.Sync:
                    image.color = Color.green;
                    break;
                case EInputType.Power:
                    image.color = Color.yellow;
                    break;
            }
        }
        
        if (text != null)
        {
            switch (inputType)
            {
                case EInputType.LeftPaddle:
                    text.text = "A";
                    break;
                case EInputType.RightPaddle:
                    text.text = "D";
                    break;
                case EInputType.Sync:
                    text.text = "S";
                    break;
                case EInputType.Power:
                    text.text = "SPACE";
                    break;
            }
        }
    }
    
    IEnumerator AnimatePrompt(GameObject promptObj, InputPrompt prompt)
    {
        RectTransform rect = promptObj.GetComponent<RectTransform>();
    
        // Start position (top of screen)
        Vector3 startPos = new Vector3(0f, 400f, 0f);
        // End position (bottom target zone)
        Vector3 endPos = new Vector3(0f, -200f, 0f);
    
        rect.anchoredPosition = startPos;
    
        float elapsed = 0f;
        while (elapsed < promptTravelTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / promptTravelTime;
        
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, progress);
        
            yield return null;
        }
    }

    
    public void HideInputPrompt(InputPrompt prompt)
    {
        if (activePromptObjects.TryGetValue(prompt, out GameObject promptObj))
        {
            if (promptObj != null)
                Destroy(promptObj);
            activePromptObjects.Remove(prompt);
        }
    }
    
    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            StartCoroutine(ShowFeedbackCoroutine(message, color));
        }
    }
    
    IEnumerator ShowFeedbackCoroutine(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(feedbackDuration);
        
        feedbackText.gameObject.SetActive(false);
    }
}
