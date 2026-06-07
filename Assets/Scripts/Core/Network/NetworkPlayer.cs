using System;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    // Biến đồng bộ: Bất cứ khi nào Server đổi giá trị này, mọi Client sẽ thấy
    public NetworkVariable<int> PlayerMMR = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Tên của người chơi (dùng kiểu chuỗi đặc biệt của Netcode)
    public NetworkVariable<Unity.Collections.FixedString32Bytes> PlayerUsername = new NetworkVariable<Unity.Collections.FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsInMatchmaking { get; private set; } = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Khi người chơi được Spawn trên Server, nạp dữ liệu từ SQLite
            string username = ServerAuthManager.GetUsernameForClient(OwnerClientId);
            if (!string.IsNullOrEmpty(username))
            {
                PlayerUsername.Value = username;
                
                var user = DatabaseManager.Instance.LoadProgress(username);
                // Giả sử LoadProgress trả về Session, ta tạm lấy MMR từ Database. 
                // Wait, LoadProgress chỉ trả về GameSessionData.
                // Lấy điểm MMR từ DB
                var accountInfo = DatabaseManager.Instance.GetAccount(username);
                if (accountInfo != null)
                {
                    PlayerMMR.Value = accountInfo.MMR;
                    
                    // Ném nguyên cục JSON từ DB xuống cho Client để Client tự giải mã
                    if (!string.IsNullOrEmpty(accountInfo.SessionDataJSON))
                    {
                        RpcLoadSessionClientRpc(accountInfo.SessionDataJSON);
                    }
                }
            }
            
            // Đăng ký với ServerMatchmaker (Tạm thời tắt để Test Đăng Nhập trước)
            // ServerMatchmaker.Instance.RegisterPlayer(this);
        }
        else if (IsOwner)
        {
            // Báo cho UI biết là đã nạp xong
            Debug.Log($"[Client] Tôi đã kết nối thành công với Username: {PlayerUsername.Value}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            ServerMatchmaker.Instance.UnregisterPlayer(this);
        }
    }

    [ServerRpc]
    public void CmdFindMatchServerRpc()
    {
        if (IsInMatchmaking) return;
        
        Debug.Log($"[Server] Player {PlayerUsername.Value} đang tìm trận...");
        IsInMatchmaking = true;
        ServerMatchmaker.Instance.AddToQueue(this);
    }
    
    [ServerRpc]
    public void CmdCancelMatchServerRpc()
    {
        if (!IsInMatchmaking) return;
        
        IsInMatchmaking = false;
        ServerMatchmaker.Instance.RemoveFromQueue(this);
    }

    // Server gọi hàm này để đẩy lệnh xuống 1 Client duy nhất (ví dụ Client 1)
    [ClientRpc]
    public void RpcMatchFoundClientRpc(string opponentName, int opponentMMR)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] ĐÃ TÌM THẤY TRẬN! Đối thủ: {opponentName} (MMR: {opponentMMR})");
            // Ở đây bạn gọi hàm bật UI Bàn Cờ Puzzle lên
            // UIManager.Instance.ShowBattleSceneUI();
        }
    }
    
    // Server gọi để báo lưu tiến trình PvE
    [ServerRpc]
    public void CmdSaveProgressServerRpc(string sessionJson)
    {
        try
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionJson);
            DatabaseManager.Instance.SaveProgress(PlayerUsername.Value.ToString(), data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Server] Lỗi khi Client lưu tiến trình: {e.Message}");
        }
    }

    // Server gọi để ném JSON tiến trình (Save file) về cho điện thoại ngay khi vừa đăng nhập xong
    [ClientRpc]
    public void RpcLoadSessionClientRpc(string sessionJson)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] Nhận được dữ liệu Save từ Server. Đang giải nén...");
            // TODO: Ở đây bạn sẽ dùng JsonUtility hoặc Newtonsoft để giải mã chuỗi `sessionJson` 
            // và nhét nó vào GameSession, ví dụ:
            // var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionJson);
            // data.UnpackToGameSession(allRecipesDictionary);
        }
    }
}
