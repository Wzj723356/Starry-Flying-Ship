using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("游戏设置")]
    public int maxPlayers = 8;
    public float respawnDelay = 5f;
    public int scoreToWin = 500;
    
    [Header("玩家设置")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    
    private int playerCount = 0;
    private int[] scores;
    private bool isGameOver = false;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        scores = new int[maxPlayers];
        SpawnPlayer();
    }
    
    public void SpawnPlayer()
    {
        if (playerCount >= maxPlayers) return;
        
        Transform spawnPoint = spawnPoints[playerCount % spawnPoints.Length];
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        player.tag = "Player";
        player.name = $"Player_{playerCount}";
        
        Damageable damageable = player.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.maxHealth = 1000f;
            damageable.currentHealth = 1000f;
        }
        
        playerCount++;
    }
    
    public void OnPlayerDeath(GameObject player, GameObject killer)
    {
        if (isGameOver) return;
        
        int killerIndex = GetPlayerIndex(killer);
        int victimIndex = GetPlayerIndex(player);
        
        if (killerIndex >= 0)
        {
            scores[killerIndex] += 100;
            
            if (scores[killerIndex] >= scoreToWin)
            {
                EndGame(killerIndex);
            }
        }
        
        Invoke(nameof(SpawnPlayer), respawnDelay);
    }
    
    int GetPlayerIndex(GameObject player)
    {
        if (player == null) return -1;
        
        string name = player.name;
        if (name.StartsWith("Player_"))
        {
            string indexStr = name.Substring(7);
            if (int.TryParse(indexStr, out int index))
                return index;
        }
        
        return -1;
    }
    
    void EndGame(int winnerIndex)
    {
        isGameOver = true;
        Debug.Log($"Player {winnerIndex} wins with {scores[winnerIndex]} points!");
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public int GetScore(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < scores.Length)
            return scores[playerIndex];
        return 0;
    }
    
    public bool IsGameOver => isGameOver;
}
