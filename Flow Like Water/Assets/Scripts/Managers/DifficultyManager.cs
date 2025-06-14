using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;
    
    [Header("Settings")]
    public DifficultySettings difficultySettings; // Add this field
    public float baseFallSpeed = 15f;
    
    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;
        
        // Ensure DifficultyState is initialized
        if (!DifficultyState.IsInitialized() && difficultySettings != null)
        {
            DifficultyState.Initialize(difficultySettings);
            Debug.Log("DifficultyManager initialized DifficultyState for direct level start");
        }
        else
        {
            DifficultyState.EnsureInitialized();
        }
    }

    void Start()
    {
        ApplyDifficultySettings();
    }
    
    void ApplyDifficultySettings()
    {
        // Apply settings from DifficultyState
        PromptObject.FallSpeed = baseFallSpeed * DifficultyState.GetFallSpeedMultiplier();
        PromptObject.ShowTextIndicators = DifficultyState.GetShowTextIndicators();
        
        if (RhythmInputSystem.Instance != null)
        {
            RhythmInputSystem.Instance.perfectZoneSize = DifficultyState.GetPerfectZoneSize();
            RhythmInputSystem.Instance.goodZoneSize = DifficultyState.GetGoodZoneSize();
            RhythmInputSystem.Instance.promptInterval = DifficultyState.GetPromptInterval();
        }
        
        ApplyAnimationSpeeds();
        
        Debug.Log($"Applied difficulty: {DifficultyState.CurrentDifficulty}");
        Debug.Log($"- Fall Speed: {PromptObject.FallSpeed}");
        Debug.Log($"- Perfect Zone: {DifficultyState.GetPerfectZoneSize()}");
        Debug.Log($"- Show Text: {DifficultyState.GetShowTextIndicators()}");
    }
    
    void ApplyAnimationSpeeds()
    {
        // Find all CharacterAnimator instances and apply difficulty speeds
        CharacterAnimator[] characterAnimators = FindObjectsOfType<CharacterAnimator>();
        
        foreach (var characterAnimator in characterAnimators)
        {
            characterAnimator.RefreshDifficultySpeed();
        }
        
        Debug.Log($"Applied difficulty animation speed to {characterAnimators.Length} CharacterAnimators");
    }
}