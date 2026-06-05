using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[System.Serializable]
public class ChatMessage
{
    public ulong SenderId;
    public string SenderName;
    public string Message;
    public System.DateTime Timestamp;
    public MessageType Type;

    public enum MessageType
    {
        Global,
        Team,
        Whisper,
        System
    }
}

public class ChatSystem : NetworkBehaviour
{
    public static ChatSystem instance;

    [Header("聊天设置")]
    public int maxMessages = 100;
    public string defaultPlayerName = "Player";

    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    // 事件
    public System.Action<ChatMessage> OnMessageReceived;
    public System.Action<ulong, string> OnPlayerNameChanged;

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

    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
    }

    void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
    }

    void OnPlayerConnected(ulong clientId)
    {
        // 设置默认玩家名称
        string playerName = $"{defaultPlayerName} {clientId}";
        playerNames[clientId] = playerName;
        
        SendSystemMessage($"{playerName} 加入了游戏");
        
        if (OnPlayerNameChanged != null)
            OnPlayerNameChanged(clientId, playerName);
    }

    void OnPlayerDisconnected(ulong clientId)
    {
        if (playerNames.ContainsKey(clientId))
        {
            SendSystemMessage($"{playerNames[clientId]} 离开了游戏");
            playerNames.Remove(clientId);
        }
    }

    // ===== 发送消息 =====
    public void SendGlobalMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        if (IsServer)
        {
            BroadcastMessageServer(message, ChatMessage.MessageType.Global);
        }
        else
        {
            SendMessageToServerServerRpc(message, ChatMessage.MessageType.Global);
        }
    }

    public void SendTeamMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        if (IsServer)
        {
            BroadcastMessageServer(message, ChatMessage.MessageType.Team);
        }
        else
        {
            SendMessageToServerServerRpc(message, ChatMessage.MessageType.Team);
        }
    }

    public void SendWhisperMessage(ulong targetPlayerId, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        if (IsServer)
        {
            SendWhisperMessageServer(targetPlayerId, message);
        }
        else
        {
            SendWhisperToServerServerRpc(targetPlayerId, message);
        }
    }

    public void SendFriendMessage(string friendId, string message)
    {
        // 好友消息可以通过好友系统发送
        Debug.Log($"发送好友消息给 {friendId}: {message}");
    }

    // ===== 服务器RPC =====
    [ServerRpc(RequireOwnership = false)]
    void SendMessageToServerServerRpc(string message, ChatMessage.MessageType type)
    {
        if (IsServer)
        {
            BroadcastMessageServer(message, type);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendWhisperToServerServerRpc(ulong targetPlayerId, string message)
    {
        if (IsServer)
        {
            SendWhisperMessageServer(targetPlayerId, message);
        }
    }

    // ===== 服务器端处理 =====
    void BroadcastMessageServer(string message, ChatMessage.MessageType type)
    {
        ulong senderId = NetworkManager.Singleton.LocalClientId;
        string senderName = playerNames.ContainsKey(senderId) ? playerNames[senderId] : "Unknown";

        ChatMessage chatMsg = new ChatMessage
        {
            SenderId = senderId,
            SenderName = senderName,
            Message = message,
            Timestamp = System.DateTime.Now,
            Type = type
        };

        AddMessage(chatMsg);
        BroadcastMessageClientRpc(chatMsg);
    }

    void SendWhisperMessageServer(ulong targetPlayerId, string message)
    {
        ulong senderId = NetworkManager.Singleton.LocalClientId;
        string senderName = playerNames.ContainsKey(senderId) ? playerNames[senderId] : "Unknown";

        ChatMessage chatMsg = new ChatMessage
        {
            SenderId = senderId,
            SenderName = senderName,
            Message = message,
            Timestamp = System.DateTime.Now,
            Type = ChatMessage.MessageType.Whisper
        };

        AddMessage(chatMsg);
        SendMessageToClientClientRpc(targetPlayerId, chatMsg);
    }

    void SendSystemMessage(string message)
    {
        ChatMessage chatMsg = new ChatMessage
        {
            SenderId = 0,
            SenderName = "系统",
            Message = message,
            Timestamp = System.DateTime.Now,
            Type = ChatMessage.MessageType.System
        };

        AddMessage(chatMsg);
        BroadcastMessageClientRpc(chatMsg);
    }

    // ===== 客户端RPC =====
    [ClientRpc]
    void BroadcastMessageClientRpc(ChatMessage message)
    {
        AddMessage(message);
    }

    [ClientRpc]
    void SendMessageToClientClientRpc(ulong targetClientId, ChatMessage message)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId ||
            NetworkManager.Singleton.LocalClientId == message.SenderId)
        {
            AddMessage(message);
        }
    }

    // ===== 消息管理 =====
    void AddMessage(ChatMessage message)
    {
        chatHistory.Add(message);
        
        // 保持消息数量限制
        while (chatHistory.Count > maxMessages)
        {
            chatHistory.RemoveAt(0);
        }

        // 触发消息接收事件
        if (OnMessageReceived != null)
        {
            OnMessageReceived(message);
        }
    }

    // ===== 玩家名称管理 =====
    public void SetPlayerName(string newName)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        
        if (IsServer)
        {
            string oldName = playerNames.ContainsKey(clientId) ? playerNames[clientId] : "Unknown";
            playerNames[clientId] = newName;
            
            SendSystemMessage($"{oldName} 改名为 {newName}");
            
            if (OnPlayerNameChanged != null)
                OnPlayerNameChanged(clientId, newName);
        }
        else
        {
            SetPlayerNameServerRpc(newName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerNameServerRpc(string newName)
    {
        SetPlayerName(newName);
    }

    public string GetPlayerName(ulong clientId)
    {
        return playerNames.ContainsKey(clientId) ? playerNames[clientId] : "Unknown";
    }

    public Dictionary<ulong, string> GetAllPlayerNames()
    {
        return new Dictionary<ulong, string>(playerNames);
    }

    // ===== 公共方法 =====
    public List<ChatMessage> GetChatHistory()
    {
        return new List<ChatMessage>(chatHistory);
    }

    public int GetMessageCount()
    {
        return chatHistory.Count;
    }

    public void ClearChatHistory()
    {
        chatHistory.Clear();
    }

    // ===== 格式化消息 =====
    public string FormatMessage(ChatMessage message)
    {
        string time = message.Timestamp.ToString("HH:mm:ss");
        
        switch (message.Type)
        {
            case ChatMessage.MessageType.Global:
                return $"[{time}] <color=#00ffff>{message.SenderName}</color>: {message.Message}";
            
            case ChatMessage.MessageType.Team:
                return $"[{time}] <color=#00ff00>[团队]</color> <color=#00ffff>{message.SenderName}</color>: {message.Message}";
            
            case ChatMessage.MessageType.Whisper:
                return $"[{time}] <color=#ffff00>[悄悄话]</color> <color=#00ffff>{message.SenderName}</color>: {message.Message}";
            
            case ChatMessage.MessageType.System:
                return $"[{time}] <color=#ff6600>[系统]</color> {message.Message}";
            
            default:
                return $"[{time}] {message.SenderName}: {message.Message}";
        }
    }
}
