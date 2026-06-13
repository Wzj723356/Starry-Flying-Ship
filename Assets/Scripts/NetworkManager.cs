using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    [Header("网络配置")]
    public string serverAddress = "localhost";
    public int serverPort = 7777;
    public float heartbeatInterval = 5f;
    
    [Header("匹配系统")]
    public bool isOnline = false;
    public bool isHost = false;
    public int maxPlayers = 8;
    public float searchTimeout = 30f;
    
    [Header("玩家信息")]
    public string playerName = "Player";
    public int playerId;
    public List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();
    
    [Header("UI设置")]
    public bool showMatchmakingUI = true;
    
    private bool isSearching = false;
    private bool isPanelOpen = false;
    private float searchStartTime = 0f;
    private float lastHeartbeat = 0f;
    private string statusText = "当前状态: 离线";
    private string playerCountText = "在线玩家: 0/8";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        LoadPlayerName();
    }
    
    void Start()
    {
        // 初始化玩家名称
        UpdatePlayerCount();
    }
    
    void Update()
    {
        if (isSearching)
        {
            UpdateMatchmaking();
        }
        
        if (isOnline)
        {
            SendHeartbeat();
        }
        
        // 按M键打开/关闭匹配面板
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePanel();
        }
    }
    
    void OnGUI()
    {
        if (!showMatchmakingUI || !isPanelOpen) return;
        
        // 面板背景
        GUI.Box(new Rect(Screen.width/2 - 200, Screen.height/2 - 150, 400, 300), "联机匹配");
        
        // 标题
        GUI.color = Color.cyan;
        GUI.Label(new Rect(Screen.width/2 - 75, Screen.height/2 - 110, 150, 30), "星辰飞舰 - 联机");
        GUI.color = Color.white;
        
        // 状态文字
        GUI.Label(new Rect(Screen.width/2 - 150, Screen.height/2 - 60, 300, 25), statusText);
        
        // 玩家数量
        GUI.Label(new Rect(Screen.width/2 - 150, Screen.height/2 - 30, 300, 25), playerCountText);
        
        // 快速匹配按钮
        if (GUI.Button(new Rect(Screen.width/2 - 100, Screen.height/2 + 10, 200, 40), "快速匹配"))
        {
            if (!isSearching && !isOnline)
            {
                StartQuickMatch();
            }
        }
        
        // 创建房间按钮
        if (GUI.Button(new Rect(Screen.width/2 - 100, Screen.height/2 + 60, 200, 40), "创建房间"))
        {
            if (!isSearching && !isOnline)
            {
                CreateRoom();
            }
        }
        
        // 取消按钮
        if (isSearching || isOnline)
        {
            if (GUI.Button(new Rect(Screen.width/2 - 100, Screen.height/2 + 110, 200, 40), "取消"))
            {
                CancelMatchmaking();
            }
        }
        
        // 关闭按钮
        if (GUI.Button(new Rect(Screen.width/2 + 160, Screen.height/2 - 130, 30, 30), "X"))
        {
            TogglePanel();
        }
    }
    
    void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            playerName = "Pilot" + Random.Range(1000, 9999);
            PlayerPrefs.SetString("PlayerName", playerName);
        }
    }
    
    void UpdateMatchmaking()
    {
        if (Time.time - searchStartTime > searchTimeout)
        {
            statusText = "搜索超时，请重试";
            isSearching = false;
        }
    }
    
    void SendHeartbeat()
    {
        if (Time.time - lastHeartbeat > heartbeatInterval)
        {
            lastHeartbeat = Time.time;
        }
    }
    
    public void StartQuickMatch()
    {
        isSearching = true;
        searchStartTime = Time.time;
        statusText = "正在搜索房间...";
        
        StartCoroutine(SimulateMatchmaking());
    }
    
    public void CreateRoom()
    {
        isHost = true;
        isOnline = true;
        statusText = "房间已创建，等待玩家加入...";
        
        PlayerInfo hostInfo = new PlayerInfo();
        hostInfo.playerId = 1;
        hostInfo.playerName = playerName;
        hostInfo.isHost = true;
        hostInfo.isReady = true;
        connectedPlayers.Add(hostInfo);
        
        UpdatePlayerCount();
        StartCoroutine(HostGame());
    }
    
    public void CancelMatchmaking()
    {
        isSearching = false;
        isOnline = false;
        isHost = false;
        connectedPlayers.Clear();
        
        statusText = "已取消";
        UpdatePlayerCount();
    }
    
    public void JoinRoom(string roomCode)
    {
        isOnline = true;
        statusText = $"正在加入房间 {roomCode}...";
    }
    
    public void LeaveRoom()
    {
        isOnline = false;
        isHost = false;
        connectedPlayers.Clear();
        statusText = "已离开房间";
        UpdatePlayerCount();
    }
    
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        if (!isPanelOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void UpdatePlayerCount()
    {
        playerCountText = $"在线玩家: {connectedPlayers.Count}/{maxPlayers}";
        
        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null)
        {
            // 更新在线人数显示
        }
    }
    
    IEnumerator SimulateMatchmaking()
    {
        yield return new WaitForSeconds(2f);
        
        if (isSearching)
        {
            statusText = "找到房间，正在连接...";
            yield return new WaitForSeconds(1f);
            
            if (Random.Range(0, 3) > 0)
            {
                isSearching = false;
                isOnline = true;
                statusText = "连接成功！";
                
                for (int i = 0; i < Random.Range(1, 4); i++)
                {
                    PlayerInfo info = new PlayerInfo();
                    info.playerId = i + 2;
                    info.playerName = "Enemy" + Random.Range(100, 999);
                    info.isHost = false;
                    info.isReady = true;
                    connectedPlayers.Add(info);
                }
                
                UpdatePlayerCount();
            }
            else
            {
                statusText = "未找到可用房间，创建新房间...";
                yield return new WaitForSeconds(1f);
                CreateRoom();
            }
        }
    }
    
    IEnumerator HostGame()
    {
        while (isOnline && isHost)
        {
            yield return new WaitForSeconds(1f);
            
            bool allReady = true;
            foreach (var player in connectedPlayers)
            {
                if (!player.isReady)
                {
                    allReady = false;
                    break;
                }
            }
            
            if (allReady && connectedPlayers.Count >= 1)
            {
                statusText = "所有玩家已准备，开始游戏！";
                yield return new WaitForSeconds(1f);
                
                StartGame();
                break;
            }
        }
    }
    
    void StartGame()
    {
        isPanelOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null)
        {
            // 更新战斗状态
        }
        
        Debug.Log("游戏开始！");
    }
    
    public bool IsConnected()
    {
        return isOnline;
    }
    
    public bool IsHost()
    {
        return isHost;
    }
    
    public int GetPlayerCount()
    {
        return connectedPlayers.Count;
    }
    
    public List<PlayerInfo> GetConnectedPlayers()
    {
        return connectedPlayers;
    }
}

[System.Serializable]
public class PlayerInfo
{
    public int playerId;
    public string playerName;
    public bool isHost;
    public bool isReady;
    public Vector3 position;
    public Quaternion rotation;
}