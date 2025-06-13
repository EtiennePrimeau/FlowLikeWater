using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform indicatorTransform;
    [SerializeField] private RectTransform canvaTransform;
    [SerializeField] private Image indicatorImage;
    [SerializeField] private TextMeshProUGUI healthValueText;
    
    [Header("Visual Settings")]
    [SerializeField] private Gradient healthColorGradient;
    [SerializeField] private bool showNumericValue = true;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    
    [Header("Movement Settings")]
    [SerializeField] private float panelHeight = 5f; // Should match your panel's height
    
    private float targetHealthPercentage;
    private Vector2 targetIndicatorPosition;
    private HealthSystem healthSystem;
    
    private void Awake()
    {
        SetupDefaultGradient();
    }

    private void Start()
    {
        Initialize(HealthSystem.Instance);
    }

    public void Initialize(HealthSystem health)
    {
        healthSystem = health;
        
        healthSystem.OnHealthChanged.AddListener(OnHealthChanged);
        healthSystem.OnHealthPercentageChanged.AddListener(OnHealthPercentageChanged);
        
        // Set initial values
        OnHealthChanged(healthSystem.CurrentHealth);
        OnHealthPercentageChanged(healthSystem.HealthPercentage);
        
        panelHeight = canvaTransform.sizeDelta.y;
        Debug.Log(panelHeight);
    }
    
    private void Update()
    {
        if (smoothTransition && indicatorTransform != null)
        {
            Vector2 currentPosition = indicatorTransform.anchoredPosition;
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetIndicatorPosition, transitionSpeed * panelHeight * Time.deltaTime);
            
            if (Vector2.Distance(newPosition, currentPosition) > 0.001f)
            {
                indicatorTransform.anchoredPosition = newPosition;
                
                // Update visual based on current position
                float currentPercentage = (newPosition.y + panelHeight * 0.5f) / panelHeight;
                UpdateVisuals(currentPercentage);
            }
        }
    }
    
    private void OnHealthChanged(float health)
    {
        if (healthValueText != null && showNumericValue && healthSystem != null)
        {
            healthValueText.text = $"{health:F0}";
        }
    }
    
    private void OnHealthPercentageChanged(float percentage)
    {
        targetHealthPercentage = percentage;
        
        // Calculate target position for indicator
        // percentage 0 = bottom of panel (-panelHeight/2)
        // percentage 0.5 = middle of panel (0) - starting position
        // percentage 1 = top of panel (+panelHeight/2)
        float targetY = (percentage - 0.5f) * panelHeight;
        targetIndicatorPosition = new Vector2(0, targetY);
        
        if (!smoothTransition && indicatorTransform != null)
        {
            indicatorTransform.anchoredPosition = targetIndicatorPosition;
            UpdateVisuals(percentage);
        }
    }
    
    private void UpdateVisuals(float percentage)
    {
        if (indicatorImage != null)
        {
            indicatorImage.color = healthColorGradient.Evaluate(percentage);
        }
    }
    
    private void SetupDefaultGradient()
    {
        if (healthColorGradient.colorKeys.Length == 0)
        {
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = Color.red;     // Min health (-100)
            colorKeys[0].time = 0f;
            colorKeys[1].color = Color.yellow;  // Neutral health (0)
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = Color.green;   // Max health (100)
            colorKeys[2].time = 1f;
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;
            
            healthColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(OnHealthChanged);
            healthSystem.OnHealthPercentageChanged.RemoveListener(OnHealthPercentageChanged);
        }
    }
}
