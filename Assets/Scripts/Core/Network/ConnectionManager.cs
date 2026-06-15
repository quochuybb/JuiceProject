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

    // Khai báo các "Cái loa" (Events)
    public static event Action OnLoginSuccess;
    public static event Action<string> OnLoginFailed;

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
        // Lắng nghe sự kiện kết nối của Netcode
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
            Debug.Log("[ConnectionManager] Đăng nhập thành công! Đã kết nối tới Server.");
            
            // Cầm loa hét lên: "Đăng nhập thành công rồi!"
            OnLoginSuccess?.Invoke();
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Lấy lý do ngắt kết nối (do Server Auth trả về hoặc do Timeout)
            string reason = NetworkManager.Singleton.DisconnectReason;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "Máy chủ không hoạt động hoặc không thể kết nối tới IP/Port.";
            }

            Debug.LogError($"[ConnectionManager] Kết nối thất bại / Đăng nhập sai: {reason}");
            
            // Cầm loa hét lên: "Đăng nhập thất bại!" kèm theo lý do
            OnLoginFailed?.Invoke(reason);
            
            // Đảm bảo dọn dẹp sạch sẽ trạng thái mạng
            Disconnect();
        }
    }

    public void StartDedicatedServer()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort, "0.0.0.0");
        
        // Bắt buộc bật tính năng Kiểm duyệt (Connection Approval) để chặn đăng nhập sai mật khẩu
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        Debug.Log($"[ConnectionManager] Starting Dedicated Server on port {ServerPort}...");
        NetworkManager.Singleton.StartServer();
    }

    public void StartHost(string username, string password)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort);
        
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        SetClientAuthData(username, password);

        Debug.Log($"[ConnectionManager] Starting Host (Server + Client)...");
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient(string username, string password)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort);
        
        // Bắt buộc bật Kiểm duyệt ở cả Client để đồng bộ cấu hình (NetworkConfig) với Server
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        SetClientAuthData(username, password);

        Debug.Log($"[ConnectionManager] Connecting to Server {ServerIP}:{ServerPort} as {username}...");
        NetworkManager.Singleton.StartClient();
    }

    private void SetClientAuthData(string username, string password)
    {
        string payload = $"{username}:{password}";
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
