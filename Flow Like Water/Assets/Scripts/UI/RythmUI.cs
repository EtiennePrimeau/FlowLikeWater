using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RhythmUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform promptContainer;
    public GameObject promptPrefab;
    public TextMeshProUGUI feedbackText;
    
    [Header("Settings")]
    public float fallTime = 2f;
    
    public void ShowPrompt(InputPrompt prompt)
    {
        GameObject promptObj = Instantiate(promptPrefab, promptContainer);
        SetupPrompt(promptObj, prompt);
        StartCoroutine(AnimatePrompt(promptObj));
    }
    
    void SetupPrompt(GameObject promptObj, InputPrompt prompt)
    {
        TextMeshProUGUI text = promptObj.GetComponent<TextMeshProUGUI>();
        
        switch (prompt.inputType)
        {
            case EInputType.StraightLeft:
                if (text)
                {
                    text.text = "ZP";
                    text.color = Color.blue;
                }
                break;
            case EInputType.StraightRight:
                if (text)
                {
                    text.text = "CI";
                    text.color = Color.blue;
                }
                break;
            case EInputType.Left:
                if (text)
                {
                    text.text = "ZI";
                    text.color = Color.green;
                }
                break;
            case EInputType.Right:
                if (text)
                {
                    text.text = "CP";
                    text.color = Color.green;
                }
                break;
            case EInputType.LeftHard:
            case EInputType.RightHard:
                if (text)
                {
                    text.text = "ZC";
                    text.color = Color.red;
                }

                break;

        }
    }
    
    IEnumerator AnimatePrompt(GameObject promptObj)
    {
        RectTransform rect = promptObj.GetComponent<RectTransform>();
        Vector3 startPos = new Vector3(0f, 300f, 0f);
        Vector3 endPos = new Vector3(0f, -300f, 0f);
        
        float elapsed = 0f;
        while (elapsed < fallTime)
        {
            elapsed += Time.deltaTime;
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, elapsed / fallTime);
            yield return null;
        }
        
        Destroy(promptObj);
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
