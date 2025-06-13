using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [Header("UI References")]
    public LevelSelectUI levelSelectUI;
    public Canvas mainCanvas;
    public Canvas levelCanvas;
    public Button playButton;
    public Button howToPlayButton;
    public Button quitButton;
    public Button backButton;
    public GameObject howToPlayPanel;
    
    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        backButton.onClick.AddListener(OnBackClicked);
        
            
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }
    
    public void OnPlayClicked()
    {
        mainCanvas.gameObject.SetActive(false);
        levelCanvas.gameObject.SetActive(true);
    }

    
    public void OnHowToPlayClicked()
    {
        if (howToPlayPanel != null)
        {
            bool isActive = howToPlayPanel.activeSelf;
            howToPlayPanel.SetActive(!isActive);
        }
    }
    
    public void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
    
    void OnBackClicked()
    {
        levelCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);

    }
}