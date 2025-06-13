using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    
    [Header("Scene Names")]
    public string level1Scene = "Level1";
    public string level2Scene = "Level2";
    public string level3Scene = "Level3";
    
    void Start()
    {
        level1Button.onClick.AddListener(() => LoadLevel(level1Scene));
        level2Button.onClick.AddListener(() => LoadLevel(level2Scene));
        level3Button.onClick.AddListener(() => LoadLevel(level3Scene));
        
        // Hide this canvas initially
        gameObject.SetActive(false);
    }
    
    void LoadLevel(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(sceneName);
        }
    }
}