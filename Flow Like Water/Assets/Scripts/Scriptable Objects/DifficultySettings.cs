using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "ScriptableObjects/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    [System.Serializable]
    public class DifficultyLevel
    {
        [Header("Visual Settings")]
        public string levelName;
        public bool showTextIndicators = true;
        
        [Header("Speed Settings")]
        public float fallSpeedMultiplier = 1f;
        public float animationSpeedMultiplier = 1f;
        
        [Header("Precision Settings")]
        public float perfectZoneSize = 50f;
        public float goodZoneSize = 100f;
        
        [Header("Timing Settings")]
        public float promptInterval = 0.8f;
    }
    
    [Header("Difficulty Levels")]
    public DifficultyLevel easy;
    public DifficultyLevel medium;
    public DifficultyLevel hard;
    
    public DifficultyLevel GetDifficultyLevel(EDifficultyLevel level)
    {
        switch (level)
        {
            case EDifficultyLevel.Easy: return easy;
            case EDifficultyLevel.Medium: return medium;
            case EDifficultyLevel.Hard: return hard;
            default: return easy;
        }
    }
}

public enum EDifficultyLevel
{
    Easy,
    Medium,
    Hard
}