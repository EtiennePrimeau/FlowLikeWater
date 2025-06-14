using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptObject : MonoBehaviour
{
    public static float FallSpeed = 15f;
    public static float OffsetStrength = 1f;
    public static bool ShowTextIndicators = true;
    
    public EInputType inputType;
    public bool canBePressed = true;
    public Vector3 targetPosition;
    public float dotLimit = -0.5f;
    
    public List<Color> frontColors;
    public List<Color> backColors;

    public GameObject text;
    
    [Header("HDR Intensity Settings")]
    public float minIntensity = 1f;
    public float maxIntensity = 6f;
    public float flashIntensity = 20f;
    
    private float timer;
    private float maxTimer = 5;
    private Material _material;
    private Color _baseColor;
    
    // FIXED: Use time-based progress calculation
    private float _spawnTime;
    private float _expectedTravelTime;
    private Vector3 _spawnPosition;

    private void Start()
    {
        if (_material == null)
            Debug.LogError("No Material attached to PromptObject");
        
        if (text != null)
            text.SetActive(ShowTextIndicators);
            
        // FIXED: Store spawn time and calculate expected travel time
        _spawnTime = Time.time;
        _spawnPosition = transform.position;
        
        // Calculate expected travel time based on distance and fall speed
        float distanceToCanoe = Vector3.Distance(_spawnPosition, CanoeController.Instance.transform.position);
        _expectedTravelTime = distanceToCanoe / FallSpeed;
        
        //Debug.Log($"Prompt spawned - Distance: {distanceToCanoe:F1}, Expected time: {_expectedTravelTime:F1}s");
    }

    void Update()
    {
        // Simple falling animation
        Vector3 newPos = transform.position + CanoeController.Instance.transform.forward * -FallSpeed * Time.deltaTime;
        newPos = new Vector3(newPos.x, 0f, newPos.z);
        transform.position = newPos;

        // UPDATE: Apply dynamic HDR intensity based on time
        UpdateHDRIntensity();

        timer += Time.deltaTime;
        
        Vector3 direction = (transform.position - CanoeController.Instance.transform.position).normalized;
        if (Vector3.Dot(CanoeController.Instance.transform.forward, direction) < dotLimit)
        {
            RhythmInputSystem.Instance.rhythmUI.ShowFeedbackPopup(transform.position, FeedbackType.Miss, 0, "You missed !");
            HealthSystem.Instance.OnFeedbackReceived(FeedbackType.Miss);
            GameManager.Instance.IncrementPromptsMissed();
            Destroy(gameObject);
        }
        
        float t = timer / (CanoeController.Instance.stateDelayTime + 0.5f);
        if (t > 1)
        {
            Destroy(text);
            return;
        }
        
        float size = Mathf.Lerp(0, 1, t);
        Vector3 scale = new Vector3(size, size, size);
        text.transform.localScale = scale;
        
        float y = Mathf.Lerp(1, 3, t);
        text.transform.localPosition = new Vector3(text.transform.localPosition.x, y, text.transform.localPosition.z);
    }
    
    private void UpdateHDRIntensity()
    {
        if (_material == null || _expectedTravelTime <= 0) return;
        
        // FIXED: Calculate progress based on elapsed time vs expected travel time
        float elapsedTime = Time.time - _spawnTime;
        float progress = elapsedTime / _expectedTravelTime;
        progress = Mathf.Clamp01(progress);
        
        // Interpolate intensity from min to max based on time progress
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, progress);
        
        // Add flash effect when very close (last 20% of journey)
        if (progress > 0.97f)
        {
            // Create pulsing effect in the final approach
            float flashProgress = (progress - 0.8f) / 0.2f; // 0 to 1 in the last 20%
            currentIntensity = Mathf.Lerp(maxIntensity, flashIntensity, flashProgress);
        }
        
        // Debug progress (uncomment to see values)
        // Debug.Log($"Progress: {progress:F2}, Elapsed: {elapsedTime:F1}s, Expected: {_expectedTravelTime:F1}s, Intensity: {currentIntensity:F1}");
        
        // Apply intensity to the base color
        Color hdrColor = _baseColor * currentIntensity;
        
        // Set both base color and emission for maximum effect
        _material.SetColor("_BaseColor", hdrColor);
        
        // Optional: Also set emission color for glow effect
        if (_material.HasProperty("_EmissionColor"))
        {
            _material.SetColor("_EmissionColor", hdrColor);
        }
    }
    
    public void SetVisuals(EInputType inputType, bool isRight, bool isFront)
    {
        _material = GetComponent<MeshRenderer>().material;
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        Color color = Color.white;
        switch (inputType)
        {
            case EInputType.StraightLeft:
                color = isRight ? frontColors[0] : backColors[0];
                text.text = !isRight ? "V" : "U";
                break;
            case EInputType.StraightRight:
                color = isRight ? backColors[0] : frontColors[0];
                text.text = !isRight ? "R" : "N";
                break;
            case EInputType.Left:
                color = isFront ? backColors[0] : frontColors[0];
                text.text = isFront ? "U" : "N";
                break;
            case EInputType.Right:
                color = isFront ? frontColors[0] : backColors[0];
                text.text = isFront ? "R" : "V";
                break;
            case EInputType.LeftHard:
                color = isRight ? frontColors[0] : backColors[0];
                text.text = !isRight ? "V" : "U";
                break;
            case EInputType.RightHard:
                color = isRight ? backColors[0] : frontColors[0];
                text.text = !isRight ? "R" : "N";
                break;
        }
        
        // Store the base color and apply initial intensity
        _baseColor = color;
        Color hdrColor = _baseColor * minIntensity;
        _material.SetColor("_BaseColor", hdrColor);
    }
}
