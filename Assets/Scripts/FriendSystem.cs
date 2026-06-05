using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FriendInfo
{
    public string playerId;
    public string playerName;
    public bool isOnline;
    public DateTime lastSeen;
}

[Serializable]
public class FriendChatMessage
{
    public string senderId;
    public string receiverId;
    public string message;
    public DateTime timestamp;
    public bool isRead;
}

public class FriendSystem : MonoBehaviour
{
    public static FriendSystem instance;

    private List<FriendInfo> friends = new List<FriendInfo>();
    private List<string> pendingRequests = new List<string>();
    private Dictionary<string, List<FriendChatMessage>> friendChatHistory = new Dictionary<string, List<FriendChatMessage>>();

    public event Action<FriendChatMessage> OnFriendMessageReceived;
    public event Action<FriendInfo> OnFriendOnlineStatusChanged;
    public event Action<string, string> OnFriendRequestReceived;

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
        LoadFriendsFromSave();
        LoadChatHistory();
    }

    // ===== 好友管理 =====
    public bool AddFriend(string playerId, string playerName)
    {
        if (IsFriend(playerId)) return false;

        FriendInfo newFriend = new FriendInfo
        {
            playerId = playerId,
            playerName = playerName,
            isOnline = false,
            lastSeen = DateTime.Now
        };

        friends.Add(newFriend);
        SaveFriends();
        
        if (!friendChatHistory.ContainsKey(playerId))
        {
            friendChatHistory[playerId] = new List<FriendChatMessage>();
        }

        return true;
    }

    public bool RemoveFriend(string playerId)
    {
        FriendInfo friend = friends.Find(f => f.playerId == playerId);
        if (friend != null)
        {
            friends.Remove(friend);
            friendChatHistory.Remove(playerId);
            SaveFriends();
            SaveChatHistory();
            return true;
        }
        return false;
    }

    public bool IsFriend(string playerId)
    {
        return friends.Exists(f => f.playerId == playerId);
    }

    public List<FriendInfo> GetFriends()
    {
        return new List<FriendInfo>(friends);
    }

    public List<FriendInfo> GetOnlineFriends()
    {
        return friends.FindAll(f => f.isOnline);
    }

    public FriendInfo GetFriend(string playerId)
    {
        return friends.Find(f => f.playerId == playerId);
    }

    // ===== 好友请求 =====
    public void SendFriendRequest(string targetPlayerId)
    {
        if (IsFriend(targetPlayerId)) return;
        if (pendingRequests.Contains(targetPlayerId)) return;

        pendingRequests.Add(targetPlayerId);
        Debug.Log($"已向玩家 {targetPlayerId} 发送好友请求");
    }

    public void AcceptFriendRequest(string requesterId, string requesterName)
    {
        if (pendingRequests.Contains(requesterId))
        {
            AddFriend(requesterId, requesterName);
            pendingRequests.Remove(requesterId);
        }
    }

    public void DeclineFriendRequest(string requesterId)
    {
        pendingRequests.Remove(requesterId);
    }

    public List<string> GetPendingRequests()
    {
        return new List<string>(pendingRequests);
    }

    // ===== 好友聊天 =====
    public void SendMessageToFriend(string friendId, string message)
    {
        if (!IsFriend(friendId)) return;
        if (string.IsNullOrEmpty(message)) return;

        FriendChatMessage chatMessage = new FriendChatMessage
        {
            senderId = "Me",
            receiverId = friendId,
            message = message,
            timestamp = DateTime.Now,
            isRead = true
        };

        if (!friendChatHistory.ContainsKey(friendId))
        {
            friendChatHistory[friendId] = new List<FriendChatMessage>();
        }

        friendChatHistory[friendId].Add(chatMessage);
        SaveChatHistory();

        if (OnFriendMessageReceived != null)
        {
            OnFriendMessageReceived(chatMessage);
        }

        Debug.Log($"发送消息给 {GetFriend(friendId)?.playerName}: {message}");
    }

    public void ReceiveMessageFromFriend(string friendId, string senderName, string message)
    {
        FriendChatMessage chatMessage = new FriendChatMessage
        {
            senderId = friendId,
            receiverId = "Me",
            message = message,
            timestamp = DateTime.Now,
            isRead = false
        };

        if (!friendChatHistory.ContainsKey(friendId))
        {
            friendChatHistory[friendId] = new List<FriendChatMessage>();
        }

        friendChatHistory[friendId].Add(chatMessage);
        SaveChatHistory();

        if (OnFriendMessageReceived != null)
        {
            OnFriendMessageReceived(chatMessage);
        }
    }

    public List<FriendChatMessage> GetChatHistoryWithFriend(string friendId)
    {
        if (friendChatHistory.ContainsKey(friendId))
        {
            return new List<FriendChatMessage>(friendChatHistory[friendId]);
        }
        return new List<FriendChatMessage>();
    }

    public void MarkMessagesAsRead(string friendId)
    {
        if (friendChatHistory.ContainsKey(friendId))
        {
            foreach (var msg in friendChatHistory[friendId])
            {
                if (msg.receiverId == "Me")
                {
                    msg.isRead = true;
                }
            }
            SaveChatHistory();
        }
    }

    public int GetUnreadMessageCount(string friendId)
    {
        if (friendChatHistory.ContainsKey(friendId))
        {
            return friendChatHistory[friendId].FindAll(m => !m.isRead && m.receiverId == "Me").Count;
        }
        return 0;
    }

    public int GetTotalUnreadCount()
    {
        int total = 0;
        foreach (var kvp in friendChatHistory)
        {
            total += GetUnreadMessageCount(kvp.Key);
        }
        return total;
    }

    public void ClearChatHistoryWithFriend(string friendId)
    {
        if (friendChatHistory.ContainsKey(friendId))
        {
            friendChatHistory[friendId].Clear();
            SaveChatHistory();
        }
    }

    // ===== 在线状态 =====
    public void UpdateFriendOnlineStatus(string playerId, bool isOnline)
    {
        FriendInfo friend = friends.Find(f => f.playerId == playerId);
        if (friend != null)
        {
            friend.isOnline = isOnline;
            if (!isOnline)
            {
                friend.lastSeen = DateTime.Now;
            }

            if (OnFriendOnlineStatusChanged != null)
            {
                OnFriendOnlineStatusChanged(friend);
            }

            SaveFriends();
        }
    }

    // ===== 存档系统 =====
    private void SaveFriends()
    {
        string json = JsonUtility.ToJson(new FriendSaveData { friends = friends });
        PlayerPrefs.SetString("FriendsData", json);
        PlayerPrefs.Save();
    }

    private void LoadFriendsFromSave()
    {
        string json = PlayerPrefs.GetString("FriendsData", "");
        if (!string.IsNullOrEmpty(json))
        {
            FriendSaveData data = JsonUtility.FromJson<FriendSaveData>(json);
            if (data != null && data.friends != null)
            {
                friends = data.friends;
            }
        }
    }

    private void SaveChatHistory()
    {
        // 转换为可序列化的字典
        Dictionary<string, string> serializedHistory = new Dictionary<string, string>();
        foreach (var kvp in friendChatHistory)
        {
            string json = JsonUtility.ToJson(new ChatHistorySaveData { messages = kvp.Value });
            serializedHistory[kvp.Key] = json;
        }

        // 保存到PlayerPrefs
        foreach (var kvp in serializedHistory)
        {
            PlayerPrefs.SetString($"ChatHistory_{kvp.Key}", kvp.Value);
        }

        PlayerPrefs.Save();
    }

    private void LoadChatHistory()
    {
        friendChatHistory.Clear();

        foreach (var friend in friends)
        {
            string key = $"ChatHistory_{friend.playerId}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                ChatHistorySaveData data = JsonUtility.FromJson<ChatHistorySaveData>(json);
                if (data != null && data.messages != null)
                {
                    friendChatHistory[friend.playerId] = data.messages;
                }
            }
        }
    }

    [Serializable]
    private class FriendSaveData
    {
        public List<FriendInfo> friends;
    }

    [Serializable]
    private class ChatHistorySaveData
    {
        public List<FriendChatMessage> messages;
    }

    // ===== 工具方法 =====
    public int GetFriendCount()
    {
        return friends.Count;
    }

    public int GetOnlineFriendCount()
    {
        return friends.FindAll(f => f.isOnline).Count;
    }

    public string FormatFriendChatMessage(FriendChatMessage message)
    {
        string time = message.timestamp.ToString("HH:mm:ss");
        string senderName = message.senderId == "Me" ? "我" : GetFriend(message.senderId)?.playerName ?? "未知";
        
        return $"[{time}] {senderName}: {message.message}";
    }
}
