using UnityEngine;
using UnityEngine.UI;
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
    
    [Header("UI配置")]
    public Canvas chatCanvas;
    public GameObject chatPanel;
    public InputField messageInput;
    public Text chatHistoryText;
    
    private ChatChannel currentChannel = ChatChannel.Global;
    private int maxMessages = 50;
    private bool isChatOpen = false;
    
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
        
        CreateChatUI();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isChatOpen)
        {
            OpenChat();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isChatOpen)
        {
            CloseChat();
        }
    }
    
    void CreateChatUI()
    {
        GameObject canvasObj = new GameObject("ChatCanvas");
        canvasObj.transform.SetParent(transform);
        chatCanvas = canvasObj.AddComponent<Canvas>();
        chatCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        chatCanvas.sortingOrder = 150;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        chatPanel = new GameObject("ChatPanel");
        chatPanel.transform.SetParent(canvasObj.transform);
        Image panelBg = chatPanel.AddComponent<Image>();
        RectTransform panelRect = panelBg.rectTransform;
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(250, 150);
        panelRect.sizeDelta = new Vector2(500, 300);
        panelBg.color = new Color(0f, 0.1f, 0.2f, 0.9f);
        
        GameObject historyObj = new GameObject("ChatHistory");
        historyObj.transform.SetParent(chatPanel.transform);
        chatHistoryText = historyObj.AddComponent<Text>();
        RectTransform histRect = chatHistoryText.rectTransform;
        histRect.anchorMin = new Vector2(0, 0.3f);
        histRect.anchorMax = new Vector2(1, 1);
        histRect.offsetMin = new Vector2(10, 0);
        histRect.offsetMax = new Vector2(-10, -10);
        chatHistoryText.fontSize = 14;
        chatHistoryText.color = Color.white;
        chatHistoryText.supportRichText = true;
        chatHistoryText.alignment = TextAnchor.UpperLeft;
        chatHistoryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        GameObject inputObj = new GameObject("MessageInput");
        inputObj.transform.SetParent(chatPanel.transform);
        messageInput = inputObj.AddComponent<InputField>();
        RectTransform inputRect = messageInput.rectTransform;
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(1, 0.3f);
        inputRect.offsetMin = new Vector2(10, 10);
        inputRect.offsetMax = new Vector2(-10, -10);
        
        GameObject inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.1f, 0.2f, 0.3f, 0.9f);
        messageInput.targetGraphic = inputBg;
        
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform);
        Text placeholderText = placeholderObj.AddComponent<Text>();
        placeholderText.rectTransform.anchorMin = Vector2.zero;
        placeholderText.rectTransform.anchorMax = Vector2.one;
        placeholderText.rectTransform.offsetMin = Vector2.zero;
        placeholderText.rectTransform.offsetMax = Vector2.one;
        placeholderText.text = "按T打开聊天...";
        placeholderText.fontSize = 14;
        placeholderText.color = Color.gray;
        placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        placeholderText.alignment = TextAnchor.MiddleLeft;
        messageInput.placeholder = placeholderText;
        
        chatPanel.SetActive(false);
    }
    
    public void OpenChat()
    {
        chatPanel.SetActive(true);
        isChatOpen = true;
        messageInput.text = "";
        messageInput.ActivateInputField();
        Time.timeScale = 0f;
    }
    
    public void CloseChat()
    {
        chatPanel.SetActive(false);
        isChatOpen = false;
        messageInput.text = "";
        Time.timeScale = 1f;
    }
    
    public void SendGlobalMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        
        ChatMessage msg = new ChatMessage
        {
            senderId = 0,
            senderName = "Player",
            content = message,
            channel = ChatChannel.Global,
            timestamp = DateTime.Now
        };
        
        chatHistory.Add(msg);
        OnMessageReceived?.Invoke(msg);
        UpdateChatDisplay();
        CloseChat();
    }
    
    public void SendTeamMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        
        ChatMessage msg = new ChatMessage
        {
            senderId = 0,
            senderName = "Player",
            content = message,
            channel = ChatChannel.Team,
            timestamp = DateTime.Now
        };
        
        chatHistory.Add(msg);
        OnMessageReceived?.Invoke(msg);
        UpdateChatDisplay();
        CloseChat();
    }
    
    public void SendPrivateMessage(ulong targetId, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        
        ChatMessage msg = new ChatMessage
        {
            senderId = 0,
            senderName = "Player",
            content = message,
            channel = ChatChannel.Private,
            targetId = targetId,
            timestamp = DateTime.Now
        };
        
        chatHistory.Add(msg);
        OnMessageReceived?.Invoke(msg);
        UpdateChatDisplay();
        CloseChat();
    }
    
    public string FormatMessage(ChatMessage msg)
    {
        string channelPrefix = msg.channel switch
        {
            ChatChannel.Global => "<color=cyan>[全局]</color>",
            ChatChannel.Team => "<color=green>[团队]</color>",
            ChatChannel.Private => "<color=yellow>[私聊]</color>",
            _ => ""
        };
        
        string timeStr = msg.timestamp.ToString("HH:mm");
        return $"{channelPrefix} [{timeStr}] <color=white>{msg.senderName}:</color> {msg.content}\n";
    }
    
    void UpdateChatDisplay()
    {
        if (chatHistoryText == null) return;
        
        string display = "";
        int start = Mathf.Max(0, chatHistory.Count - maxMessages);
        
        for (int i = start; i < chatHistory.Count; i++)
        {
            display += FormatMessage(chatHistory[i]);
        }
        
        chatHistoryText.text = display;
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
        UpdateChatDisplay();
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
