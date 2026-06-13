using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("游戏状态")]
    public GameState currentState = GameState.Menu;
    
    [Header("玩家数据")]
    public int score = 0;
    public int kills = 0;
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
    }
    
    public void StartGame()
    {
        currentState = GameState.Playing;
        score = 0;
        kills = 0;
        Time.timeScale = 1f;
        
        Debug.Log("游戏开始！");
    }
    
    public void PauseGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        
        Debug.Log("游戏暂停");
    }
    
    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        Debug.Log("游戏继续");
    }
    
    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        
        Debug.Log($"游戏结束！得分: {score} 击杀: {kills}");
    }
    
    public void AddScore(int points)
    {
        score += points;
    }
    
    public void AddKill()
    {
        kills++;
        AddScore(100);
    }
    
    public bool IsPlaying() => currentState == GameState.Playing;
}