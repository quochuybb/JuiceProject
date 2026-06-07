using System.Text;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkManager))]
public class ServerAuthManager : MonoBehaviour
{
    public static System.Collections.Generic.Dictionary<ulong, string> ClientUsernames = new System.Collections.Generic.Dictionary<ulong, string>();

    public static string GetUsernameForClient(ulong clientId)
    {
        if (ClientUsernames.TryGetValue(clientId, out string username))
            return username;
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

        string credentials = Encoding.UTF8.GetString(payload);
        string[] parts = credentials.Split(':');
        
        if (parts.Length != 2)
        {
            Debug.LogWarning("[ServerAuth] Format đăng nhập không hợp lệ.");
            response.Reason = "Invalid credential format.";
            return;
        }

        string username = parts[0];
        string password = parts[1];

        // 3. Nếu Server muốn tạo tài khoản mới ngay lúc đăng nhập (Auto-register nếu chưa có)
        // Hoặc kiểm tra mật khẩu nếu đã có
        if (DatabaseManager.Instance.VerifyAccount(username, password, out AccountUser user))
        {
            // Đăng nhập thành công
            ClientUsernames[request.ClientNetworkId] = username;
            Debug.Log($"[ServerAuth] Cho phép Client {request.ClientNetworkId} ({username}) tham gia.");
            response.Approved = true;
            response.CreatePlayerObject = true; // Tạo GameObject cho người chơi
        }
        else
        {
            // Thử đăng ký tài khoản mới nếu chưa tồn tại
            if (DatabaseManager.Instance.CreateAccount(username, password))
            {
                ClientUsernames[request.ClientNetworkId] = username;
                Debug.Log($"[ServerAuth] Tự động đăng ký và cho phép {username} tham gia.");
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
            else
            {
                Debug.LogWarning($"[ServerAuth] Từ chối kết nối từ {request.ClientNetworkId} - Sai mật khẩu.");
                response.Reason = "Invalid password.";
            }
        }
    }
}
