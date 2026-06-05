using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    
    [Header("网络设置")]
    public int maxPlayers = 8;
    public int port = 7777;
    public string serverIP = "127.0.0.1";
    public int tickRate = 64;
    
    [Header("网络状态")]
    public bool isServer;
    public bool isClient;
    public int localPlayerId = 0;
    public int connectedPlayers = 0;
    
    private Dictionary<int, NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();
    private float lastTickTime = 0;
    private float tickInterval = 0;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        tickInterval = 1.0f / tickRate;
    }
    
    void Update()
    {
        if (isServer || isClient)
        {
            if (Time.time - lastTickTime >= tickInterval)
            {
                NetworkTick();
                lastTickTime = Time.time;
            }
        }
    }
    
    public void StartServer()
    {
        isServer = true;
        Debug.Log("服务器已启动");
        connectedPlayers = 0;
        
        NetworkPlayer serverPlayer = new NetworkPlayer();
        serverPlayer.id = 0;
        serverPlayer.isLocal = true;
        serverPlayer.isServer = true;
        players.Add(0, serverPlayer);
    }
    
    public void ConnectToServer(string ip, int port)
    {
        serverIP = ip;
        this.port = port;
        isClient = true;
        Debug.Log($"正在连接到服务器: {ip}:{port}");
    }
    
    public void Disconnect()
    {
        isServer = false;
        isClient = false;
        players.Clear();
        connectedPlayers = 0;
        Debug.Log("已断开连接");
    }
    
    void NetworkTick()
    {
        if (isServer)
        {
            BroadcastPlayerStates();
        }
        
        if (isClient)
        {
            SendPlayerState();
        }
    }
    
    void BroadcastPlayerStates()
    {
        foreach (var player in players.Values)
        {
            if (player.isLocal && player.shipController != null)
            {
                NetworkPlayerState state = new NetworkPlayerState();
                state.playerId = player.id;
                state.position = player.shipController.transform.position;
                state.rotation = player.shipController.transform.rotation;
                state.velocity = player.shipController.GetVelocity();
                state.thrust = player.shipController.GetThrustPercentage();
                state.afterburner = player.shipController.IsAfterburnerActive();
                
                SendStateToClients(state);
            }
        }
    }
    
    void SendPlayerState()
    {
        if (players.ContainsKey(localPlayerId))
        {
            NetworkPlayer player = players[localPlayerId];
            if (player.shipController != null)
            {
                NetworkPlayerState state = new NetworkPlayerState();
                state.playerId = localPlayerId;
                state.position = player.shipController.transform.position;
                state.rotation = player.shipController.transform.rotation;
                state.velocity = player.shipController.GetVelocity();
                state.thrust = player.shipController.GetThrustPercentage();
                state.afterburner = player.shipController.IsAfterburnerActive();
                
                SendStateToServer(state);
            }
        }
    }
    
    void SendStateToClients(NetworkPlayerState state)
    {
        foreach (var player in players.Values)
        {
            if (!player.isServer)
            {
                ApplyStateToPlayer(state, player);
            }
        }
    }
    
    void SendStateToServer(NetworkPlayerState state)
    {
        if (isServer)
        {
            ApplyStateToPlayer(state, players[state.playerId]);
        }
    }
    
    void ApplyStateToPlayer(NetworkPlayerState state, NetworkPlayer player)
    {
        if (player.shipController != null)
        {
            player.shipController.transform.position = state.position;
            player.shipController.transform.rotation = state.rotation;
            
            Rigidbody rb = player.shipController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = state.velocity;
            }
        }
    }
    
    public void AddPlayer(int playerId, GameObject shipObject = null)
    {
        if (!players.ContainsKey(playerId))
        {
            NetworkPlayer player = new NetworkPlayer();
            player.id = playerId;
            player.isLocal = playerId == localPlayerId;
            
            if (shipObject != null)
            {
                StarShipFlightController controller = shipObject.GetComponent<StarShipFlightController>();
                player.shipController = controller;
            }
            
            players.Add(playerId, player);
            connectedPlayers++;
            
            Debug.Log($"玩家 {playerId} 已加入");
        }
    }
    
    public void RemovePlayer(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            connectedPlayers--;
            Debug.Log($"玩家 {playerId} 已离开");
        }
    }
    
    public NetworkPlayer GetPlayer(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            return players[playerId];
        }
        return null;
    }
    
    public List<NetworkPlayer> GetAllPlayers()
    {
        return new List<NetworkPlayer>(players.Values);
    }
    
    public bool IsConnected()
    {
        return isServer || isClient;
    }
}

public class NetworkPlayer
{
    public int id;
    public bool isLocal;
    public bool isServer;
    public StarShipFlightController shipController;
    public int score;
    public int ping;
}

public struct NetworkPlayerState
{
    public int playerId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public float thrust;
    public bool afterburner;
    public float health;
    public int ammo;
}
