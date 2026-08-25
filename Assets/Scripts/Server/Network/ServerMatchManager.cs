using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ServerMatchManager : MonoBehaviour
{
    public static ServerMatchManager Instance { get; private set; }
    private Dictionary<string, List<ulong>> pendingRooms = new Dictionary<string, List<ulong>>();
    public static Dictionary<string, GameRoom> ActiveRooms = new Dictionary<string, GameRoom>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkPlayer.OnServerAttackReceived += HandlePlayerAttack;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        NetworkPlayer.OnServerAttackReceived -= HandlePlayerAttack;
    }

    private void HandlePlayerAttack(ulong clientId, int damage)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (ServerAuthManager.ClientRoomIds.TryGetValue(clientId, out string roomId))
        {
            if (ActiveRooms.TryGetValue(roomId, out GameRoom room))
            {
                room.HandleAttack(clientId, damage);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        Debug.Log($"[ServerMatchManager] Client {clientId} connected.");
        if (ServerAuthManager.ClientRoomIds.TryGetValue(clientId, out string roomId))
        {
            Debug.Log($"[ServerMatchManager] Client {clientId} joined room {roomId}.");
            if (!pendingRooms.ContainsKey(roomId))
            {
                pendingRooms[roomId] = new List<ulong>();
            }
            
            pendingRooms[roomId].Add(clientId);

            if (pendingRooms[roomId].Count == 2)
            {
                ulong player1 = pendingRooms[roomId][0];
                ulong player2 = pendingRooms[roomId][1];
                StartMatch(roomId, player1, player2);
            }
        }
    }

    private void StartMatch(string roomId, ulong player1Id, ulong player2Id)
    {
        GameRoom newRoom = new GameRoom(roomId, player1Id, player2Id);
        ActiveRooms[roomId] = newRoom;
        pendingRooms.Remove(roomId);

        Debug.Log($"[ServerMatchManager] START GAME IN ROOM {roomId}!");

        var p1Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player1Id);
        var p2Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player2Id);

        if (p1Obj != null && p2Obj != null)
        {
            var p1 = p1Obj.GetComponent<NetworkPlayer>();
            var p2 = p2Obj.GetComponent<NetworkPlayer>();

            ClientRpcParams p1Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { player1Id } } };
            p1.RpcStartGameClientRpc(newRoom.BoardSeed, p1Params);

            ClientRpcParams p2Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { player2Id } } };
            p2.RpcStartGameClientRpc(newRoom.BoardSeed, p2Params);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (ServerAuthManager.ClientRoomIds.TryGetValue(clientId, out string roomId))
        {
            if (pendingRooms.ContainsKey(roomId))
            {
                pendingRooms[roomId].Remove(clientId);
                if (pendingRooms[roomId].Count == 0) pendingRooms.Remove(roomId);
            }
            
            if (ActiveRooms.ContainsKey(roomId))
            {
                ActiveRooms.Remove(roomId);
            }
        }
    }

    public void SubmitMatchResult(string winnerUsername, string loserUsername, System.Action<int, int> onSuccess)
    {
        StartCoroutine(SubmitMatchResultRoutine(winnerUsername, loserUsername, onSuccess));
    }

    private IEnumerator SubmitMatchResultRoutine(string winner, string loser, System.Action<int, int> onSuccess)
    {
        string url = "http://localhost:3000/api/match/internal/result";
        string json = $"{{\"winnerId\":\"{winner}\",\"loserId\":\"{loser}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerMatchManager] Đã lưu kết quả thành công: {request.downloadHandler.text}");
                try
                {
                    // Lấy ra MMR mới trả về từ Backend
                    var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(request.downloadHandler.text);
                    int winnerMmr = (int)jsonResponse["data"]["winnerMmr"];
                    int loserMmr = (int)jsonResponse["data"]["loserMmr"];
                    
                    onSuccess?.Invoke(winnerMmr, loserMmr);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[ServerMatchManager] Lỗi đọc JSON MMR: " + e.Message);
                    onSuccess?.Invoke(0, 0);
                }
            }
            else
            {
                Debug.LogError($"[ServerMatchManager] Lỗi lưu kết quả: {request.error}");
                onSuccess?.Invoke(0, 0);
            }
        }
    }
}
