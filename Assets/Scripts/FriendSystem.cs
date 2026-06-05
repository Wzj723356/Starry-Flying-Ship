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

public class FriendSystem : MonoBehaviour
{
    public static FriendSystem instance;

    private List<FriendInfo> friends = new List<FriendInfo>();
    private List<string> pendingRequests = new List<string>();

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
        return true;
    }

    public bool RemoveFriend(string playerId)
    {
        FriendInfo friend = friends.Find(f => f.playerId == playerId);
        if (friend != null)
        {
            friends.Remove(friend);
            SaveFriends();
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

    [Serializable]
    private class FriendSaveData
    {
        public List<FriendInfo> friends;
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
}
