using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    
    [Header("=== UI引用 ===")]
    public GameObject mainMenuCanvas;
    public GameObject multiplayerMenuCanvas;
    public GameObject loadingCanvas;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerCountText;
    public Button quickMatchButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public TMP_InputField roomCodeInput;
    
    [Header("=== 游戏设置 ===")]
    public int maxPlayers = 8;
    public bool isMatchmaking = false;
    public string defaultPlayerName = "Player";
    
    private string currentRoomCode = "";
    private List<ulong> connectedPlayers = new List<ulong>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    
    // 事件
    public System.Action<ulong> OnPlayerConnected;
    public System.Action<ulong> OnPlayerDisconnected;
    public System.Action OnMatchStarted;
    
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
            return;
        }
        
        // 初始化网络日志
        Unity.Netcode.NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    void Start()
    {
        SetupUI();
        UpdateConnectionStatus("离线 - 请先登录");
    }
    
    void SetupUI()
    {
        if (quickMatchButton != null)
            quickMatchButton.onClick.AddListener(StartQuickMatch);
        
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(CreatePrivateRoom);
        
        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(JoinPrivateRoom);
    }
    
    // ===== 快速匹配 =====
    public async void StartQuickMatch()
    {
        UpdateConnectionStatus("正在搜索房间...");
        
        isMatchmaking = true;
        
        // 尝试以客户端身份启动并连接到默认房间
        bool started = await TryConnectAsync();
        
        if (!started)
        {
            // 没有可用房间，创建新房间
            UpdateConnectionStatus("未找到房间，正在创建...");
            CreateDefaultRoom();
        }
        else
        {
            UpdateConnectionStatus("已连接到房间！");
        }
        
        isMatchmaking = false;
    }
    
    System.Threading.Tasks.Task<bool> TryConnectAsync()
    {
        // 由于Unity Netcode不直接支持大厅，我们模拟快速匹配
        // 实际项目中应使用:
        // - Unity Relay + Lobby
        // - Photon PUN
        // - Mirror + Master Server
        // - Nakama
        
        return System.Threading.Tasks.Task.FromResult(false);
    }
    
    // ===== 创建私人房间 =====
    public void CreatePrivateRoom()
    {
        currentRoomCode = GenerateRoomCode();
        CreateDefaultRoom();
        
        if (statusText != null)
            statusText.text = $"房间码: {currentRoomCode}";
        
        Debug.Log($"创建房间，房间码: {currentRoomCode}");
    }
    
    // ===== 加入私人房间 =====
    public void JoinPrivateRoom()
    {
        if (roomCodeInput != null)
        {
            currentRoomCode = roomCodeInput.text.ToUpper();
            UpdateConnectionStatus($"正在加入房间: {currentRoomCode}...");
            
            // 实际项目中验证房间码后连接
            CreateDefaultRoom();
        }
    }
    
    void CreateDefaultRoom()
    {
        Unity.Netcode.NetworkManager.Singleton.StartHost();
    }
    
    // ===== 网络回调 =====
    void OnServerStarted()
    {
        UpdateConnectionStatus("房间已创建！等待玩家加入...");
        connectedPlayers.Clear();
        playerNames.Clear();
        
        // 添加主机自己
        ulong localClientId = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
        connectedPlayers.Add(localClientId);
        playerNames[localClientId] = $"{defaultPlayerName} {localClientId}";
        UpdatePlayerCount();
        
        if (OnMatchStarted != null)
            OnMatchStarted();
    }
    
    void OnClientConnected(ulong clientId)
    {
        if (!connectedPlayers.Contains(clientId))
        {
            connectedPlayers.Add(clientId);
            playerNames[clientId] = $"{defaultPlayerName} {clientId}";
            UpdatePlayerCount();
            
            Debug.Log($"玩家 {clientId} 已连接");
            
            if (OnPlayerConnected != null)
                OnPlayerConnected(clientId);
        }
        
        if (clientId == Unity.Netcode.NetworkManager.Singleton.LocalClientId)
        {
            UpdateConnectionStatus("已加入房间！");
            
            if (OnMatchStarted != null)
                OnMatchStarted();
        }
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.Remove(clientId);
        playerNames.Remove(clientId);
        UpdatePlayerCount();
        
        Debug.Log($"玩家 {clientId} 已断开");
        
        if (OnPlayerDisconnected != null)
            OnPlayerDisconnected(clientId);
    }
    
    // ===== 断开连接 =====
    public void Disconnect()
    {
        Unity.Netcode.NetworkManager.Singleton.Shutdown();
        connectedPlayers.Clear();
        playerNames.Clear();
        currentRoomCode = "";
        UpdateConnectionStatus("已断开连接");
    }
    
    // ===== UI更新 =====
    void UpdateConnectionStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
        
        Debug.Log($"[网络] {status}");
    }
    
    void UpdatePlayerCount()
    {
        if (playerCountText != null)
            playerCountText.text = $"玩家: {connectedPlayers.Count}/{maxPlayers}";
    }
    
    // ===== 玩家名称管理 =====
    public void SetPlayerName(ulong playerId, string playerName)
    {
        playerNames[playerId] = playerName;
    }
    
    public string GetPlayerName(ulong playerId)
    {
        return playerNames.ContainsKey(playerId) ? playerNames[playerId] : $"Player {playerId}";
    }
    
    public Dictionary<ulong, string> GetAllPlayerNames()
    {
        return new Dictionary<ulong, string>(playerNames);
    }
    
    // ===== 工具方法 =====
    string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";
        for (int i = 0; i < 6; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }
        return code;
    }
    
    // ===== 公共方法 =====
    public bool IsConnected()
    {
        return Unity.Netcode.NetworkManager.Singleton.IsClient;
    }
    
    public bool IsHost()
    {
        return Unity.Netcode.NetworkManager.Singleton.IsHost;
    }
    
    public string GetRoomCode()
    {
        return currentRoomCode;
    }
    
    public int GetPlayerCount()
    {
        return connectedPlayers.Count;
    }
    
    public List<ulong> GetConnectedPlayers()
    {
        return new List<ulong>(connectedPlayers);
    }
    
    // ===== 场景加载 =====
    public void LoadGameScene()
    {
        Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}

// ===== 网络玩家数据 =====
public struct PlayerData : INetworkSerializable
{
    public ulong ClientId;
    public string PlayerName;
    public Vector3 Position;
    public Quaternion Rotation;
    public float Health;
    public bool IsReady;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref IsReady);
    }
}
