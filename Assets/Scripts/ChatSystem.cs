using UnityEngine;
using System.Collections.Generic;
using System;

public class ChatSystem : MonoBehaviour
{
    public static ChatSystem instance;
    
    [Header("聊天历史")]
    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    
    public event Action<ChatMessage> OnMessageReceived;
    public event Action<ulong, string> OnPlayerNameChanged;
    
    [Header("聊天设置")]
    public bool enableChat = true;
    public int maxMessages = 50;
    
    private ChatChannel currentChannel = ChatChannel.Global;
    private bool isChatOpen = false;
    private string inputText = "";
    private Vector2 scrollPos = Vector2.zero;
    
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
        
        // 添加初始消息
        AddSystemMessage("欢迎来到星辰飞舰！");
        AddSystemMessage("按 T 打开聊天，按 ESC 关闭");
    }
    
    void Update()
    {
        if (!enableChat) return;
        
        if (Input.GetKeyDown(KeyCode.T) && !isChatOpen)
        {
            OpenChat();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isChatOpen)
        {
            CloseChat();
        }
        
        if (isChatOpen && Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(inputText);
        }
    }
    
    void OnGUI()
    {
        if (!enableChat) return;
        
        // 聊天面板背景 - 增大尺寸
        GUI.Box(new Rect(10, Screen.height - 250, 500, 240), "聊天");
        
        // 聊天历史
        scrollPos = GUI.BeginScrollView(new Rect(20, Screen.height - 220, 480, 150), scrollPos, new Rect(0, 0, 460, Mathf.Max(150, chatHistory.Count * 25)));
        
        string display = "";
        int start = Mathf.Max(0, chatHistory.Count - maxMessages);
        
        for (int i = start; i < chatHistory.Count; i++)
        {
            display += FormatMessage(chatHistory[i]);
        }
        
        GUI.Label(new Rect(0, 0, 460, chatHistory.Count * 25), display);
        GUI.EndScrollView();
        
        // 输入框 - 增大尺寸
        if (isChatOpen)
        {
            GUI.SetNextControlName("ChatInput");
            inputText = GUI.TextField(new Rect(20, Screen.height - 55, 480, 30), inputText, 100);
            GUI.FocusControl("ChatInput");
            
            // 发送按钮
            if (GUI.Button(new Rect(510, Screen.height - 55, 60, 30), "发送"))
            {
                SendChatMessage(inputText);
            }
        }
        else
        {
            GUI.Label(new Rect(20, Screen.height - 55, 480, 30), "按 T 打开聊天...");
        }
        
        // 频道选择
        GUI.Label(new Rect(520, Screen.height - 240, 80, 20), "频道:");
        if (GUI.Button(new Rect(520, Screen.height - 215, 80, 25), "全局"))
        {
            currentChannel = ChatChannel.Global;
        }
        if (GUI.Button(new Rect(520, Screen.height - 185, 80, 25), "团队"))
        {
            currentChannel = ChatChannel.Team;
        }
        if (GUI.Button(new Rect(520, Screen.height - 155, 80, 25), "私聊"))
        {
            currentChannel = ChatChannel.Private;
        }
    }
    
    void OpenChat()
    {
        isChatOpen = true;
        inputText = "";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void CloseChat()
    {
        isChatOpen = false;
        inputText = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        
        ChatMessage msg = new ChatMessage
        {
            senderId = 0,
            senderName = "玩家",
            content = message,
            channel = currentChannel,
            timestamp = DateTime.Now
        };
        
        chatHistory.Add(msg);
        OnMessageReceived?.Invoke(msg);
        inputText = "";
    }
    
    void AddSystemMessage(string message)
    {
        ChatMessage msg = new ChatMessage
        {
            senderId = ulong.MaxValue,
            senderName = "系统",
            content = message,
            channel = ChatChannel.Global,
            timestamp = DateTime.Now
        };
        
        chatHistory.Add(msg);
    }
    
    public string FormatMessage(ChatMessage msg)
    {
        string channelPrefix = msg.channel switch
        {
            ChatChannel.Global => "[全局]",
            ChatChannel.Team => "[团队]",
            ChatChannel.Private => "[私聊]",
            _ => ""
        };
        
        string timeStr = msg.timestamp.ToString("HH:mm");
        
        if (msg.senderId == ulong.MaxValue)
        {
            return $"[{timeStr}] <color=yellow>{msg.senderName}:</color> {msg.content}\n";
        }
        
        return $"{channelPrefix} [{timeStr}] {msg.senderName}: {msg.content}\n";
    }
    
    public void SetChannel(ChatChannel channel)
    {
        currentChannel = channel;
    }
    
    public List<ChatMessage> GetHistory()
    {
        return new List<ChatMessage>(chatHistory);
    }
    
    public void ClearHistory()
    {
        chatHistory.Clear();
    }
}

[System.Serializable]
public class ChatMessage
{
    public ulong senderId;
    public string senderName;
    public string content;
    public ChatChannel channel;
    public ulong targetId;
    public DateTime timestamp;
}

public enum ChatChannel
{
    Global,
    Team,
    Private
}