using UnityEngine;

public static class DifficultyState
{
    public static EDifficultyLevel CurrentDifficulty { get; private set; } = EDifficultyLevel.Easy;
    public static DifficultySettings.DifficultyLevel CurrentSettings { get; private set; }
    
    private static DifficultySettings _difficultySettings;
    private static bool _isInitialized = false;
    
    public static void Initialize(DifficultySettings settings)
    {
        _difficultySettings = settings;
        _isInitialized = true;
        UpdateCurrentSettings();
        
        Debug.Log($"DifficultyState initialized with settings: {settings.name}");
    }
    
    public static void SetDifficulty(EDifficultyLevel difficulty)
    {
        CurrentDifficulty = difficulty;
        UpdateCurrentSettings();
        
        Debug.Log($"Difficulty changed to: {difficulty}");
    }
    
    private static void UpdateCurrentSettings()
    {
        if (_difficultySettings != null)
        {
            CurrentSettings = _difficultySettings.GetDifficultyLevel(CurrentDifficulty);
        }
    }
    
    // Fallback methods with safe defaults
    public static float GetFallSpeedMultiplier() => CurrentSettings?.fallSpeedMultiplier ?? 1f;
    public static float GetPerfectZoneSize() => CurrentSettings?.perfectZoneSize ?? 50f;
    public static float GetGoodZoneSize() => CurrentSettings?.goodZoneSize ?? 100f;
    public static float GetPromptInterval() => CurrentSettings?.promptInterval ?? 0.8f;
    public static bool GetShowTextIndicators() => CurrentSettings?.showTextIndicators ?? true;
    public static float GetAnimationSpeedMultiplier() => CurrentSettings?.animationSpeedMultiplier ?? 1f;
    
    // Check if properly initialized
    public static bool IsInitialized() => _isInitialized && _difficultySettings != null;
    
    // Auto-initialize if not initialized (fallback for direct level starts)
    public static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            // Try to find DifficultySettings in the scene
            var levelSelectUI = Object.FindObjectOfType<LevelSelectUI>();
            if (levelSelectUI != null && levelSelectUI.difficultySettings != null)
            {
                Initialize(levelSelectUI.difficultySettings);
                return;
            }
            
            // Try to find DifficultyManager with settings
            var difficultyManager = Object.FindObjectOfType<DifficultyManager>();
            if (difficultyManager != null && difficultyManager.difficultySettings != null)
            {
                Initialize(difficultyManager.difficultySettings);
                return;
            }
            
            // Try to load from Resources as last resort
            var defaultSettings = Resources.Load<DifficultySettings>("DifficultySettings");
            if (defaultSettings != null)
            {
                Initialize(defaultSettings);
                Debug.LogWarning("DifficultyState auto-initialized with default settings from Resources");
                return;
            }
            
            Debug.LogWarning("DifficultyState could not be initialized - using hardcoded defaults");
        }
    }
}
