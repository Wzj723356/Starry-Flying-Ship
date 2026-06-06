using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    
    [Header("连接状态")]
    public Text connectionStatusText;
    public GameObject connectingPanel;
    
    [Header("房间设置")]
    public string roomName = "DefaultRoom";
    public int maxPlayers = 8;
    
    private bool isConnected = false;
    private bool isMatchmaking = false;
    
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
    
    void Start()
    {
        UpdateConnectionStatus("离线");
    }
    
    public void StartQuickMatch()
    {
        if (isMatchmaking) return;
        
        UpdateConnectionStatus("正在搜索房间...");
        isMatchmaking = true;
        
        // 模拟匹配
        Invoke(nameof(SimulateMatchFound), 2f);
    }
    
    void SimulateMatchFound()
    {
        isConnected = true;
        isMatchmaking = false;
        UpdateConnectionStatus("已连接到房间！");
        
        if (connectingPanel != null)
            connectingPanel.SetActive(false);
    }
    
    public void CreateRoom(string name)
    {
        roomName = name;
        UpdateConnectionStatus($"创建房间: {name}");
        
        // 模拟创建房间
        Invoke(nameof(SimulateRoomCreated), 1f);
    }
    
    void SimulateRoomCreated()
    {
        isConnected = true;
        UpdateConnectionStatus($"房间 {roomName} 已创建");
    }
    
    public void JoinRoom(string name)
    {
        UpdateConnectionStatus($"正在加入房间: {name}...");
        
        // 模拟加入房间
        Invoke(nameof(SimulateJoined), 1.5f);
    }
    
    void SimulateJoined()
    {
        isConnected = true;
        UpdateConnectionStatus("已加入房间！");
    }
    
    public void LeaveRoom()
    {
        isConnected = false;
        UpdateConnectionStatus("已离开房间");
    }
    
    void UpdateConnectionStatus(string status)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = status;
        
        Debug.Log($"[Network] {status}");
    }
    
    public bool IsConnected() => isConnected;
    public bool IsMatchmaking() => isMatchmaking;
}