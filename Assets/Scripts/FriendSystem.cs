using UnityEngine;

public class FriendSystem : MonoBehaviour
{
    public static FriendSystem instance;
    
    [System.Serializable]
    public class FriendInfo
    {
        public string playerId;
        public string playerName;
        public bool isOnline;
        public System.DateTime lastSeen;
    }
    
    [Header("好友列表")]
    public System.Collections.Generic.List<FriendInfo> friends = new System.Collections.Generic.List<FriendInfo>();
    
    [Header("好友请求")]
    public System.Collections.Generic.List<FriendInfo> pendingRequests = new System.Collections.Generic.List<FriendInfo>();
    
    public System.Action<FriendInfo> OnFriendAdded;
    public System.Action<FriendInfo> OnFriendRemoved;
    public System.Action<FriendInfo> OnFriendOnline;
    
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
    
    public void AddFriend(string playerId, string playerName)
    {
        FriendInfo friend = new FriendInfo
        {
            playerId = playerId,
            playerName = playerName,
            isOnline = false,
            lastSeen = System.DateTime.Now
        };
        
        friends.Add(friend);
        OnFriendAdded?.Invoke(friend);
        
        Debug.Log($"好友添加: {playerName}");
    }
    
    public void RemoveFriend(string playerId)
    {
        FriendInfo friend = friends.Find(f => f.playerId == playerId);
        if (friend != null)
        {
            friends.Remove(friend);
            OnFriendRemoved?.Invoke(friend);
            Debug.Log($"好友移除: {friend.playerName}");
        }
    }
    
    public void SendFriendRequest(string playerId)
    {
        Debug.Log($"好友请求发送: {playerId}");
    }
    
    public void AcceptFriendRequest(string playerId)
    {
        FriendInfo request = pendingRequests.Find(r => r.playerId == playerId);
        if (request != null)
        {
            pendingRequests.Remove(request);
            friends.Add(request);
            OnFriendAdded?.Invoke(request);
            Debug.Log($"好友请求接受: {request.playerName}");
        }
    }
    
    public void SetFriendOnline(string playerId, bool online)
    {
        FriendInfo friend = friends.Find(f => f.playerId == playerId);
        if (friend != null)
        {
            friend.isOnline = online;
            friend.lastSeen = System.DateTime.Now;
            
            if (online)
                OnFriendOnline?.Invoke(friend);
        }
    }
    
    public System.Collections.Generic.List<FriendInfo> GetOnlineFriends()
    {
        return friends.FindAll(f => f.isOnline);
    }
    
    public System.Collections.Generic.List<FriendInfo> GetAllFriends() => friends;
}