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
    
    public void SendGlobalMessage(string message)
    {
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
    }
    
    public void SendTeamMessage(string message)
    {
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
    }
    
    public void SendPrivateMessage(ulong targetId, string message)
    {
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
    }
    
    public string FormatMessage(ChatMessage msg)
    {
        string channelPrefix = msg.channel switch
        {
            ChatChannel.Global => "[全局]",
            ChatChannel.Team => "[团队]",
            ChatChannel.Private => $"[悄悄话]",
            _ => ""
        };
        
        return $"{channelPrefix} {msg.senderName}: {msg.content}";
    }
    
    public List<ChatMessage> GetChatHistory() => chatHistory;
    
    public Dictionary<ulong, string> GetAllPlayerNames() => playerNames;
    
    public void ClearChatHistory()
    {
        chatHistory.Clear();
    }
    
    public void SetPlayerName(ulong playerId, string name)
    {
        playerNames[playerId] = name;
        OnPlayerNameChanged?.Invoke(playerId, name);
    }
}

public enum ChatChannel
{
    Global,
    Team,
    Private
}

[Serializable]
public class ChatMessage
{
    public ulong senderId;
    public ulong targetId;
    public string senderName;
    public string content;
    public ChatChannel channel;
    public DateTime timestamp;
}