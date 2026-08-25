using System;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("Network Settings")]
    public string ServerIP = "127.0.0.1";
    public ushort ServerPort = 7777;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[ConnectionManager] Login success! Connected to server.");
            
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            string reason = NetworkManager.Singleton.DisconnectReason;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "Server is offline or cannot connect to IP/Port.";
            }

            Debug.LogError($"[ConnectionManager] Login failed / Wrong password: {reason}");
            
            Disconnect();
        }
    }

    public void StartDedicatedServer()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort, "0.0.0.0");
        
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        Debug.Log($"[ConnectionManager] Starting Dedicated Server on port {ServerPort}...");
        NetworkManager.Singleton.StartServer();
    }


    public void StartClient(string token)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort);
        
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        SetClientAuthData(token);

        Debug.Log($"[ConnectionManager] Connecting to Server {ServerIP}:{ServerPort} with JWT Token...");
        NetworkManager.Singleton.StartClient();
    }

    public void SetClientAuthData(string token)
    {
        byte[] payloadBytes = Encoding.UTF8.GetBytes(token);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
