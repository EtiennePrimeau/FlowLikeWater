using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Selection")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    
    [Header("Scene Names")]
    public string level1Scene = "Level1";
    public string level2Scene = "Level2";
    public string level3Scene = "Level3";
    
    [Header("Difficulty Selection")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    
    [Header("Difficulty Visual Feedback")]
    public GameObject easySelectedIndicator;
    public GameObject mediumSelectedIndicator;
    public GameObject hardSelectedIndicator;
    
    [Header("Settings")]
    public DifficultySettings difficultySettings;
    
    private EDifficultyLevel selectedDifficulty = EDifficultyLevel.Easy;
    private string selectedLevel = "";
    
    void Start()
    {
        // Initialize difficulty state
        DifficultyState.Initialize(difficultySettings);
        
        SetupLevelButtons();
        SetupDifficultyButtons();
        UpdateDifficultyVisuals();
        
        gameObject.SetActive(false);
    }
    
    void SetupLevelButtons()
    {
        level1Button.onClick.AddListener(() => SelectLevel(level1Scene));
        level2Button.onClick.AddListener(() => SelectLevel(level2Scene));
        level3Button.onClick.AddListener(() => SelectLevel(level3Scene));
    }
    
    void SetupDifficultyButtons()
    {
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SelectDifficulty(EDifficultyLevel.Easy));
            
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => SelectDifficulty(EDifficultyLevel.Medium));
            
        if (hardButton != null)
            hardButton.onClick.AddListener(() => SelectDifficulty(EDifficultyLevel.Hard));
    }
    
    void SelectLevel(string sceneName)
    {
        selectedLevel = sceneName;
        LoadLevel();
    }
    
    void SelectDifficulty(EDifficultyLevel difficulty)
    {
        selectedDifficulty = difficulty;
        DifficultyState.SetDifficulty(difficulty);
        UpdateDifficultyVisuals();
    }
    
    void LoadLevel()
    {
        if (string.IsNullOrEmpty(selectedLevel))
        {
            Debug.LogError("No level selected!");
            return;
        }
        
        // Difficulty is already set in DifficultyState
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(selectedLevel);
        }
    }
    
    void UpdateDifficultyVisuals()
    {
        // Hide all indicators
        if (easySelectedIndicator != null) easySelectedIndicator.SetActive(false);
        if (mediumSelectedIndicator != null) mediumSelectedIndicator.SetActive(false);
        if (hardSelectedIndicator != null) hardSelectedIndicator.SetActive(false);
        
        // Show selected difficulty indicator
        switch (selectedDifficulty)
        {
            case EDifficultyLevel.Easy:
                if (easySelectedIndicator != null) easySelectedIndicator.SetActive(true);
                break;
            case EDifficultyLevel.Medium:
                if (mediumSelectedIndicator != null) mediumSelectedIndicator.SetActive(true);
                break;
            case EDifficultyLevel.Hard:
                if (hardSelectedIndicator != null) hardSelectedIndicator.SetActive(true);
                break;
        }
    }
}
