using System.Text;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkManager))]
public class ServerAuthManager : MonoBehaviour
{
    public static System.Collections.Generic.Dictionary<ulong, string> ClientUsernames = new System.Collections.Generic.Dictionary<ulong, string>();
    public static System.Collections.Generic.Dictionary<ulong, string> ClientRoomIds = new System.Collections.Generic.Dictionary<ulong, string>();

    public static string GetUsernameForClient(ulong clientId)
    {
        if (ClientUsernames.TryGetValue(clientId, out string username))
        {
            Debug.Log("GetUsernameForClient: " + clientId + " - " + username);
            return username;
        }
        return "";
    }

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientUsernames.ContainsKey(clientId))
            ClientUsernames.Remove(clientId);
        if (ClientRoomIds.ContainsKey(clientId))
            ClientRoomIds.Remove(clientId);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 1. Mặc định là bị từ chối
        response.Approved = false;
        response.CreatePlayerObject = false;

        // 2. Trích xuất Payload (chứa username:password) do Client gửi lên
        byte[] payload = request.Payload;
        if (payload == null || payload.Length == 0)
        {
            Debug.LogWarning("[ServerAuth] Client kết nối mà không gửi thông tin đăng nhập.");
            response.Reason = "Missing credentials.";
            return;
        }

        string rawPayload = Encoding.UTF8.GetString(payload);
        string[] parts = rawPayload.Split('|');
        
        if (parts.Length < 2)
        {
            Debug.LogWarning("[ServerAuth] Payload sai định dạng (Thiếu RoomId hoặc Token).");
            response.Reason = "Invalid Auth Payload Format.";
            return;
        }

        string jwtToken = parts[0];
        string roomId = parts[1];

        // 3. Giải mã và Xác thực JWT Token
        if (JwtUtility.VerifyToken(jwtToken, out JwtPayload decodedPayload))
        {
            string username = decodedPayload.username;

            // Đăng nhập thành công
            ClientUsernames[request.ClientNetworkId] = username;
            ClientRoomIds[request.ClientNetworkId] = roomId; // Lưu lại RoomId
            
            Debug.Log($"[ServerAuth] Xác thực JWT thành công! Client {request.ClientNetworkId} ({username}) tham gia phòng: {roomId}");
            response.Approved = true;
            response.CreatePlayerObject = true; // Tạo GameObject cho người chơi
        }
        else
        {
            Debug.LogWarning($"[ServerAuth] Từ chối kết nối từ {request.ClientNetworkId} - Token không hợp lệ.");
            response.Reason = "Invalid JWT Token.";
        }
    }
}
