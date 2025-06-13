using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Scene References")]
    public string menuSceneName = "Menu";
    public string gameSceneName = "Game";
    public string gameOverSceneName = "GameOver";
    
    [Header("Game State")]
    public GameState currentState = GameState.Bootstrap;
    public bool isPaused = false;
    
    [Header("Pause Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool allowPauseInput = true;
    public GameObject pauseCanva;
    public GameObject deadCanva;
    public GameObject endCanva;
    public FinishUI finishUI;
    public Button backToMenuButton;
    public Button backToMenuButton2;
    public Button resumeButton;
    public Button playAgainButton;
    
    // Game session data
    [HideInInspector] public float gameStartTime;
    [HideInInspector] public int score = 0;
    [HideInInspector] public bool gameWon = false;
    [HideInInspector] public int promptsHit = 0;
    [HideInInspector] public int promptsMissed = 0;
    
    // Pause system events
    public Action OnGamePaused;
    public Action OnGameResumed;
    
    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameManager();
            pauseCanva.SetActive(false);
            deadCanva.SetActive(false);
            endCanva.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        backToMenuButton.onClick.AddListener(OnBackClicked);
        backToMenuButton2.onClick.AddListener(OnBackClicked);
        resumeButton.onClick.AddListener(OnResumeClicked);
        playAgainButton.onClick.AddListener(OnBackClicked);
        HealthSystem.OnHealthMinReached += OnHealthMin;
    }
    
    private void OnDisable()
    {
        HealthSystem.OnHealthMinReached -= OnHealthMin;
    }
    
    void Update()
    {
        // Handle pause input
        if (allowPauseInput && Input.GetKeyDown(pauseKey))
        {
            if (currentState == GameState.Playing)
            {
                TogglePause();
            }
        }
    }
    
    void InitializeGameManager()
    {
        // Load menu scene after bootstrap
        if (currentState == GameState.Bootstrap)
        {
            LoadMenu();
        }
    }
    
    public void LoadMenu()
    {
        currentState = GameState.Menu;
        isPaused = false;
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(menuSceneName);
    }
    
    public void StartGame()
    {
        currentState = GameState.Playing;
        gameStartTime = Time.time;
        score = 0;
        gameWon = false;
        isPaused = false;
        promptsHit = 0;
        promptsMissed = 0;
        
        // Ensure time is running normally
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene("Game");
        return;
        
        currentState = GameState.Playing;
        gameStartTime = Time.time;
        score = 0;
        gameWon = false;
        isPaused = false;
    
        Time.timeScale = 1f;
    
        Debug.Log($"Loading level: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    
    public void EndGame(bool won = false, int finalScore = 0)
    {
        currentState = GameState.GameOver;
        //gameWon = won;
        //score = finalScore;
        //isPaused = false;
        Debug.Log("End game");
        
        deadCanva.SetActive(true);
        Time.timeScale = 1f;
    }
    
    public void ReturnToMenu()
    {
        LoadMenu();
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void LevelCompleted()
    {
        currentState = GameState.GameOver;
        //Debug.Log("Level Completed!");
    
        finishUI.SetScore();
        
        endCanva.SetActive(true);
        Time.timeScale = 1f;
    }    
    
    // PAUSE SYSTEM METHODS
    
    public void TogglePause()
    {
        if (currentState != GameState.Playing) return;
        
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        
        pauseCanva.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;
        
        Debug.Log("Game Paused");
        OnGamePaused?.Invoke();
    }
    
    public void ResumeGame()
    {
        if (currentState != GameState.Playing) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanva.SetActive(false);
        
        //Debug.Log("Game Resumed");
        OnGameResumed?.Invoke();
    }
    
    void OnBackClicked()
    {
        pauseCanva.SetActive(false);
        deadCanva.SetActive(false);
        endCanva.SetActive(false);
        ReturnToMenu();
    }
    
    void OnResumeClicked()
    {
        ResumeGame();
    }

    void OnHealthMin()
    {
        EndGame();
    }
    
    public void IncrementPromptsHit()
    {
        promptsHit++;
        //Debug.Log($"Prompts Hit: {promptsHit}");
        GuiDebug.Instance.PrintFloat("Prompts hit", promptsHit);
    }

    public void IncrementPromptsMissed()
    {
        promptsMissed++;
        //Debug.Log($"Prompts Missed: {promptsMissed}");
        GuiDebug.Instance.PrintFloat("Prompts missed", promptsMissed);
    }
}

public enum GameState
{
    Bootstrap,
    Menu,
    Playing,
    GameOver
}
