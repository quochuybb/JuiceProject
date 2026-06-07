using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerMatchmaker : MonoBehaviour
{
    public static ServerMatchmaker Instance { get; private set; }

    [Header("Settings")]
    public GameObject GameRoomPrefab; // Gán Prefab GameRoom ở đây trên Inspector

    private List<NetworkPlayer> onlinePlayers = new List<NetworkPlayer>();
    private List<NetworkPlayer> waitingQueue = new List<NetworkPlayer>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Logic ghép trận rất đơn giản: Cứ có 2 người trong hàng chờ là ghép
        if (waitingQueue.Count >= 2)
        {
            NetworkPlayer playerA = waitingQueue[0];
            NetworkPlayer playerB = waitingQueue[1];

            waitingQueue.RemoveAt(0);
            waitingQueue.RemoveAt(0); // Vì A đã bị xóa, B nhảy lên index 0

            CreateGameRoom(playerA, playerB);
        }
    }

    public void RegisterPlayer(NetworkPlayer player)
    {
        if (!onlinePlayers.Contains(player))
        {
            onlinePlayers.Add(player);
            Debug.Log($"[Matchmaker] Player {player.PlayerUsername.Value} online.");
        }
    }

    public void UnregisterPlayer(NetworkPlayer player)
    {
        onlinePlayers.Remove(player);
        RemoveFromQueue(player);
        Debug.Log($"[Matchmaker] Player offline.");
    }

    public void AddToQueue(NetworkPlayer player)
    {
        if (!waitingQueue.Contains(player))
        {
            waitingQueue.Add(player);
            Debug.Log($"[Matchmaker] {player.PlayerUsername.Value} vào hàng chờ (Queue size: {waitingQueue.Count})");
        }
    }

    public void RemoveFromQueue(NetworkPlayer player)
    {
        if (waitingQueue.Contains(player))
        {
            waitingQueue.Remove(player);
            Debug.Log($"[Matchmaker] {player.PlayerUsername.Value} thoát hàng chờ.");
        }
    }

    private void CreateGameRoom(NetworkPlayer playerA, NetworkPlayer playerB)
    {
        Debug.Log($"[Matchmaker] Ghép cặp thành công: {playerA.PlayerUsername.Value} vs {playerB.PlayerUsername.Value}");

        // Tạo phòng chơi
        if (GameRoomPrefab != null)
        {
            GameObject roomObj = Instantiate(GameRoomPrefab);
            NetworkObject netObj = roomObj.GetComponent<NetworkObject>();
            netObj.Spawn(); // Spawn phòng trên Server

            GameRoom room = roomObj.GetComponent<GameRoom>();
            room.Initialize(playerA, playerB);
        }
        else
        {
            Debug.LogWarning("[Matchmaker] Lỗi: Chưa gán GameRoomPrefab vào ServerMatchmaker!");
            // Vẫn gọi hàm RPC cho 2 người (để test UI) dù không có phòng vật lý
            playerA.RpcMatchFoundClientRpc(playerB.PlayerUsername.Value.ToString(), playerB.PlayerMMR.Value);
            playerB.RpcMatchFoundClientRpc(playerA.PlayerUsername.Value.ToString(), playerA.PlayerMMR.Value);
        }
    }
}
