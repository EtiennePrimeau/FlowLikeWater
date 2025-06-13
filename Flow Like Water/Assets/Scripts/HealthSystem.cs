using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance;
    
    [Header("Health Settings")]
    [SerializeField] private float currentHealth = 0f;
    [SerializeField] private float minHealth = -100f;
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Health Changes")]
    [SerializeField] private float perfectHealthGain = 15f;
    [SerializeField] private float goodHealthGain = 10f;
    [SerializeField] private float holdHealthGain = 1f;
    [SerializeField] private float badHealthLoss = -5f;
    [SerializeField] private float wrongHealthLoss = -10f;
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnHealthPercentageChanged;
    public static Action OnHealthMinReached;
    public UnityEvent OnHealthMaxReached;
    
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => (currentHealth - minHealth) / (maxHealth - minHealth);
    public bool IsAtMinHealth => currentHealth <= minHealth;
    public bool IsAtMaxHealth => currentHealth >= maxHealth;


    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }
    
    public void OnFeedbackReceived(FeedbackType feedbackType)
    {
        float healthChange = GetHealthChangeForFeedback(feedbackType);
        ChangeHealth(healthChange);
    }
    
    private float GetHealthChangeForFeedback(FeedbackType feedbackType)
    {
        return feedbackType switch
        {
            FeedbackType.Perfect => perfectHealthGain,
            FeedbackType.Good => goodHealthGain,
            FeedbackType.Bad => badHealthLoss,
            FeedbackType.Wrong => wrongHealthLoss,
            FeedbackType.Hold => holdHealthGain,
            FeedbackType.Miss => badHealthLoss,
            _ => 0f
        };
    }
    
    public void ChangeHealth(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, minHealth, maxHealth);
        
        if (Mathf.Abs(currentHealth - previousHealth) > 0.01f)
        {
            NotifyHealthChanged();

            if (IsAtMinHealth && previousHealth > minHealth)
            {
                OnHealthMinReached?.Invoke();
                Debug.Log("Min Health Reached");
            }
                
            else if (IsAtMaxHealth && previousHealth < maxHealth)
                OnHealthMaxReached?.Invoke();
        }
    }
    
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, minHealth, maxHealth);
        NotifyHealthChanged();
    }
    
    public void ResetHealth()
    {
        SetHealth(0f);
    }
    
    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnHealthPercentageChanged?.Invoke(HealthPercentage);
    }
}
