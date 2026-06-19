using System;
using UnityEngine;
using Unity.Netcode;

public partial class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalInstance { get; private set; }

    // Biến đồng bộ: Bất cứ khi nào Server đổi giá trị này, mọi Client sẽ thấy
    public NetworkVariable<int> PlayerMMR = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Tên của người chơi (dùng kiểu chuỗi đặc biệt của Netcode)
    public NetworkVariable<Unity.Collections.FixedString32Bytes> PlayerUsername = new NetworkVariable<Unity.Collections.FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsInMatchmaking { get; private set; } = false;

    // --- EVENTS CHO SERVER ---
    public static event Action<NetworkPlayer> OnServerPlayerSpawned;
    public static event Action<NetworkPlayer> OnServerPlayerDespawned;
    public static event Action<NetworkPlayer> OnServerMatchmakingRequested;
    public static event Action<NetworkPlayer> OnServerMatchmakingCanceled;
    public static event Action<NetworkPlayer, string> OnServerSaveProgressRequested;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (IsServer)
        {
            OnServerPlayerSpawned?.Invoke(this);
        }
        else if (IsOwner)
        {
            LocalInstance = this;
            Debug.Log($"[Client] Tôi đã kết nối thành công với Username: {PlayerUsername.Value}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnServerPlayerDespawned?.Invoke(this);
        }
        else if (IsOwner)
        {
            LocalInstance = null;
        }
    }

    // --- CÁC HÀM CHO NÚT BẤM UI (RANKING MODE) ---
    
    public void StartMatchmaking()
    {
        if (IsOwner)
        {
            Debug.Log("[Client] Gửi yêu cầu Tìm Trận lên Server...");
            CmdFindMatchServerRpc();
        }
    }

    public void CancelMatchmaking()
    {
        if (IsOwner)
        {
            Debug.Log("[Client] Hủy Tìm Trận.");
            CmdCancelMatchServerRpc();
        }
    }

    // ----------------------------------------------

    [ServerRpc]
    public void CmdFindMatchServerRpc()
    {
        if (IsInMatchmaking) return;
        
        Debug.Log($"[Server] Player {PlayerUsername.Value} đang tìm trận...");
        IsInMatchmaking = true;
        OnServerMatchmakingRequested?.Invoke(this);
    }
    
    [ServerRpc]
    public void CmdCancelMatchServerRpc()
    {
        if (!IsInMatchmaking) return;
        
        IsInMatchmaking = false;
        OnServerMatchmakingCanceled?.Invoke(this);
    }

    // Server gọi hàm này để đẩy lệnh xuống 1 Client duy nhất (ví dụ Client 1)
    [ClientRpc]
    public void RpcMatchFoundClientRpc(string opponentName, int opponentMMR)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] ĐÃ TÌM THẤY TRẬN! Đối thủ: {opponentName} (MMR: {opponentMMR})");
        }
    }
    
    // Server gọi để báo lưu tiến trình PvE
    [ServerRpc]
    public void CmdSaveProgressServerRpc(string sessionJson)
    {
        OnServerSaveProgressRequested?.Invoke(this, sessionJson);
    }
    
    // Server gọi để ném JSON tiến trình (Save file) về cho điện thoại ngay khi vừa đăng nhập xong
    [ClientRpc]
    public void RpcLoadSessionClientRpc(string sessionJson)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] Nhận được dữ liệu Save từ Server. Đang giải nén...");
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionJson);
            
            var allRecipesList = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
            var allRecipesDictionary = new System.Collections.Generic.Dictionary<int, RecipeData>();
            foreach (var r in allRecipesList)
            {
                if (r != null) allRecipesDictionary[r.recipeID] = r;
            }

            data.UnpackToGameSession(allRecipesDictionary);

            // Phục hồi ChapterData dựa trên CurrentChapterID
            if (!string.IsNullOrEmpty(GameSession.CurrentChapterID))
            {
                var allChapters = Resources.LoadAll<ChapterData>("ScriptObjects");
                foreach (var c in allChapters)
                {
                    if (c != null && c.chapterID == GameSession.CurrentChapterID)
                    {
                        GameSession.CurrentChapterData = c;
                        break;
                    }
                }
            }
        }
    }


    // --- CÁC HÀM XỬ LÝ LƯU GAME TỪ PHÍA CLIENT ---
    
    public void SaveProgress()
    {
        if (!IsOwner) return;

        Debug.Log("[Client] Bắt đầu gom dữ liệu GameSession để lưu lên Server...");
        
        GameSessionData data = new GameSessionData();
        data.PackFromGameSession();
        
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        
        // Bắn dữ liệu lên Server
        CmdSaveProgressServerRpc(json);
    }

    public void SaveAndQuit()
    {
        if (IsOwner)
        {
            SaveProgress();
            Debug.Log("[Client] Đã gửi lệnh Lưu Game. Đang tắt ứng dụng...");
        }
        
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnApplicationQuit()
    {
        if (IsOwner)
        {
            SaveProgress();
        }
    }


}

