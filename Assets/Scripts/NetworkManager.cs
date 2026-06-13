using UnityEngine;
using UnityEngine.UI;
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
    
    [Header("UI组件")]
    public GameObject matchmakingPanel;
    public Text statusText;
    public Button quickMatchButton;
    public Button hostGameButton;
    public Button cancelButton;
    public Text playerCountText;
    
    private bool isSearching = false;
    private float searchStartTime = 0f;
    private float lastHeartbeat = 0f;
    
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
        
        InitializeUI();
    }
    
    void Start()
    {
        LoadPlayerName();
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
    }
    
    void InitializeUI()
    {
        // 创建匹配面板
        CreateMatchmakingPanel();
    }
    
    void CreateMatchmakingPanel()
    {
        // Canvas
        GameObject canvasObj = new GameObject("NetworkCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 面板背景
        GameObject panelObj = new GameObject("MatchmakingPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        matchmakingPanel = panelObj;
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.rectTransform.anchoredPosition = new Vector2(0, 0);
        panelBg.rectTransform.sizeDelta = new Vector2(400, 300);
        panelBg.color = new Color(0f, 0.1f, 0.2f, 0.95f);
        
        // 标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform);
        Text title = titleObj.AddComponent<Text>();
        title.rectTransform.anchoredPosition = new Vector2(0, 120);
        title.rectTransform.sizeDelta = new Vector2(300, 40);
        title.fontSize = 28;
        title.color = Color.cyan;
        title.text = "联机匹配";
        title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        title.alignment = TextAnchor.MiddleCenter;
        
        // 状态文字
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(panelObj.transform);
        statusText = statusObj.AddComponent<Text>();
        statusText.rectTransform.anchoredPosition = new Vector2(0, 60);
        statusText.rectTransform.sizeDelta = new Vector2(300, 30);
        statusText.fontSize = 18;
        statusText.color = Color.white;
        statusText.text = "当前状态: 离线";
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.alignment = TextAnchor.MiddleCenter;
        
        // 玩家数量
        GameObject countObj = new GameObject("PlayerCount");
        countObj.transform.SetParent(panelObj.transform);
        playerCountText = countObj.AddComponent<Text>();
        playerCountText.rectTransform.anchoredPosition = new Vector2(0, 30);
        playerCountText.rectTransform.sizeDelta = new Vector2(300, 25);
        playerCountText.fontSize = 16;
        playerCountText.color = new Color(0.7f, 0.9f, 1f);
        playerCountText.text = "在线玩家: 0/8";
        playerCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        playerCountText.alignment = TextAnchor.MiddleCenter;
        
        // 快速匹配按钮
        GameObject quickBtnObj = new GameObject("QuickMatchButton");
        quickBtnObj.transform.SetParent(panelObj.transform);
        Button quickBtn = quickBtnObj.AddComponent<Button>();
        quickBtnObj.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.8f, 1f);
        quickBtn.rectTransform.anchoredPosition = new Vector2(0, -10);
        quickBtn.rectTransform.sizeDelta = new Vector2(200, 45);
        quickMatchButton = quickBtn;
        
        GameObject quickTextObj = new GameObject("QuickText");
        quickTextObj.transform.SetParent(quickBtnObj.transform);
        Text quickText = quickTextObj.AddComponent<Text>();
        quickText.rectTransform.anchoredPosition = Vector2.zero;
        quickText.rectTransform.sizeDelta = new Vector2(200, 45);
        quickText.fontSize = 18;
        quickText.color = Color.white;
        quickText.text = "快速匹配";
        quickText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        quickText.alignment = TextAnchor.MiddleCenter;
        
        quickBtn.onClick.AddListener(() => StartQuickMatch());
        
        // 创建房间按钮
        GameObject hostBtnObj = new GameObject("HostGameButton");
        hostBtnObj.transform.SetParent(panelObj.transform);
        Button hostBtn = hostBtnObj.AddComponent<Button>();
        hostBtnObj.AddComponent<Image>().color = new Color(0.3f, 0.7f, 0.3f, 1f);
        hostBtn.rectTransform.anchoredPosition = new Vector2(0, -65);
        hostBtn.rectTransform.sizeDelta = new Vector2(200, 45);
        hostGameButton = hostBtn;
        
        GameObject hostTextObj = new GameObject("HostText");
        hostTextObj.transform.SetParent(hostBtnObj.transform);
        Text hostText = hostTextObj.AddComponent<Text>();
        hostText.rectTransform.anchoredPosition = Vector2.zero;
        hostText.rectTransform.sizeDelta = new Vector2(200, 45);
        hostText.fontSize = 18;
        hostText.color = Color.white;
        hostText.text = "创建房间";
        hostText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hostText.alignment = TextAnchor.MiddleCenter;
        
        hostBtn.onClick.AddListener(() => CreateRoom());
        
        // 取消按钮
        GameObject cancelBtnObj = new GameObject("CancelButton");
        cancelBtnObj.transform.SetParent(panelObj.transform);
        Button cancelBtn = cancelBtnObj.AddComponent<Button>();
        cancelBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 1f);
        cancelBtn.rectTransform.anchoredPosition = new Vector2(0, -120);
        cancelBtn.rectTransform.sizeDelta = new Vector2(200, 40);
        cancelButton = cancelBtn;
        cancelButton.gameObject.SetActive(false);
        
        GameObject cancelTextObj = new GameObject("CancelText");
        cancelTextObj.transform.SetParent(cancelBtnObj.transform);
        Text cancelText = cancelTextObj.AddComponent<Text>();
        cancelText.rectTransform.anchoredPosition = Vector2.zero;
        cancelText.rectTransform.sizeDelta = new Vector2(200, 40);
        cancelText.fontSize = 16;
        cancelText.color = Color.white;
        cancelText.text = "取消";
        cancelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cancelText.alignment = TextAnchor.MiddleCenter;
        
        cancelBtn.onClick.AddListener(() => CancelMatchmaking());
        
        // 关闭按钮
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panelObj.transform);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtnObj.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
        closeBtn.rectTransform.anchoredPosition = new Vector2(180, 130);
        closeBtn.rectTransform.sizeDelta = new Vector2(30, 30);
        
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeBtnObj.transform);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.rectTransform.anchoredPosition = Vector2.zero;
        closeText.rectTransform.sizeDelta = new Vector2(30, 30);
        closeText.fontSize = 20;
        closeText.color = Color.white;
        closeText.text = "X";
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeText.alignment = TextAnchor.MiddleCenter;
        
        closeBtn.onClick.AddListener(() => TogglePanel());
        
        matchmakingPanel.SetActive(false);
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
            statusText.text = "搜索超时，请重试";
            isSearching = false;
            quickMatchButton.interactable = true;
            hostGameButton.interactable = true;
            cancelButton.gameObject.SetActive(false);
        }
    }
    
    void SendHeartbeat()
    {
        if (Time.time - lastHeartbeat > heartbeatInterval)
        {
            // 发送心跳包
            lastHeartbeat = Time.time;
        }
    }
    
    public void StartQuickMatch()
    {
        isSearching = true;
        searchStartTime = Time.time;
        statusText.text = "正在搜索房间...";
        quickMatchButton.interactable = false;
        hostGameButton.interactable = false;
        cancelButton.gameObject.SetActive(true);
        
        // 模拟搜索过程
        StartCoroutine(SimulateMatchmaking());
    }
    
    public void CreateRoom()
    {
        isHost = true;
        isOnline = true;
        statusText.text = "房间已创建，等待玩家加入...";
        quickMatchButton.interactable = false;
        hostGameButton.interactable = false;
        cancelButton.gameObject.SetActive(true);
        
        // 添加主机到玩家列表
        PlayerInfo hostInfo = new PlayerInfo();
        hostInfo.playerId = 1;
        hostInfo.playerName = playerName;
        hostInfo.isHost = true;
        hostInfo.isReady = true;
        connectedPlayers.Add(hostInfo);
        
        UpdatePlayerCount();
        
        // 开始作为主机运行
        StartCoroutine(HostGame());
    }
    
    public void CancelMatchmaking()
    {
        isSearching = false;
        isOnline = false;
        isHost = false;
        connectedPlayers.Clear();
        
        statusText.text = "已取消";
        quickMatchButton.interactable = true;
        hostGameButton.interactable = true;
        cancelButton.gameObject.SetActive(false);
        
        UpdatePlayerCount();
    }
    
    public void JoinRoom(string roomCode)
    {
        // 加入指定房间
        isOnline = true;
        statusText.text = $"正在加入房间 {roomCode}...";
    }
    
    public void LeaveRoom()
    {
        isOnline = false;
        isHost = false;
        connectedPlayers.Clear();
        statusText.text = "已离开房间";
        quickMatchButton.interactable = true;
        hostGameButton.interactable = true;
        cancelButton.gameObject.SetActive(false);
        UpdatePlayerCount();
    }
    
    public void TogglePanel()
    {
        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(!matchmakingPanel.activeSelf);
        }
    }
    
    void UpdatePlayerCount()
    {
        playerCountText.text = $"在线玩家: {connectedPlayers.Count}/{maxPlayers}";
        
        // 更新HUD
        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null)
        {
            hud.SetOnlinePlayers(connectedPlayers.Count);
        }
    }
    
    IEnumerator SimulateMatchmaking()
    {
        yield return new WaitForSeconds(2f);
        
        // 模拟找到房间
        if (isSearching)
        {
            statusText.text = "找到房间，正在连接...";
            yield return new WaitForSeconds(1f);
            
            // 随机决定是否成功
            if (Random.Range(0, 3) > 0) // 66%成功率
            {
                isSearching = false;
                isOnline = true;
                statusText.text = "连接成功！";
                
                // 添加模拟玩家
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
                statusText.text = "未找到可用房间，创建新房间...";
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
            
            // 检查所有玩家是否准备
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
                statusText.text = "所有玩家已准备，开始游戏！";
                yield return new WaitForSeconds(1f);
                
                // 启动游戏
                StartGame();
                break;
            }
        }
    }
    
    void StartGame()
    {
        // 关闭匹配面板
        if (matchmakingPanel != null)
        {
            matchmakingPanel.SetActive(false);
        }
        
        // 更新战斗状态
        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null)
        {
            hud.SetBattleStatus("战斗中");
        }
        
        Debug.Log("游戏开始！");
    }
    
    // 公开方法供其他脚本调用
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
