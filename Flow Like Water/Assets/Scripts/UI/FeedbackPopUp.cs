using UnityEngine;
using TMPro;
using System.Collections;


public enum FeedbackType { Perfect, Good, Bad, Wrong, Hold, Miss }


public class FeedbackPopup : MonoBehaviour
{
    [Header("Visual Settings")]
    public TextMeshPro feedbackText;
    public CanvasGroup canvasGroup;
    public float lifetime = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public Vector3 movementDirection = Vector3.up;
    public float movementDistance = 50f;
    
    [Header("Feedback Colors")]
    public Color perfectColor = Color.green;
    public Color goodColor = Color.yellow;
    public Color badColor = Color.red;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float timer = 0f;
    
    
    
    void Start()
    {
        Vector3 playerDirection = transform.position - CanoeController.Instance.transform.position;
        
        startPosition = transform.position;
        targetPosition = startPosition + movementDirection.normalized * movementDistance +
                         playerDirection.normalized * movementDistance * 4f;
        
        // Start the animation
        StartCoroutine(AnimatePopup());
    }
    
    public void Initialize(FeedbackType type, string message, float distance = 0f)
    {
        // Set text and color based on feedback type
        switch (type)
        {
            case FeedbackType.Perfect:
                feedbackText.text = $"PERFECT!\n{distance:F1}m";
                feedbackText.color = perfectColor;
                break;
            case FeedbackType.Good:
                feedbackText.text = $"GOOD!\n{distance:F1}m";
                feedbackText.color = goodColor;
                break;
            case FeedbackType.Bad:
                feedbackText.text = $"TOO FAR!\n{distance:F1}m";
                feedbackText.color = badColor;
                break;
            case FeedbackType.Wrong:
                feedbackText.text = message; // Custom message for wrong input
                feedbackText.color = badColor;
                break;
            case FeedbackType.Hold:
                feedbackText.text = message;
                feedbackText.color = goodColor;
                break;
            case FeedbackType.Miss:
                feedbackText.text = message;
                feedbackText.color = badColor;
                break;
        }
    }
    
    IEnumerator AnimatePopup()
    {
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float normalizedTime = Mathf.Max(timer / lifetime, 0.2f);
            
            // Scale animation
            float scaleValue = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.one * scaleValue;
            
            // Fade animation
            float fadeValue = fadeCurve.Evaluate(normalizedTime);
            if (canvasGroup != null)
                canvasGroup.alpha = fadeValue;
            
            // Movement animation
            transform.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            
            yield return null;
        }
        
        // Destroy after animation completes
        Destroy(gameObject);
    }
}
