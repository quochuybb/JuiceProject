using UnityEngine;

public class DebugLoginUI : MonoBehaviour
{
    private string username = "Player1";
    private string password = "123";

    private void OnGUI()
    {
        // Nếu đã kết nối (Làm Server hoặc Client), thì ẩn màn hình đăng nhập đi
        if (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("ĐÃ KẾT NỐI!");
            if (GUILayout.Button("Ngắt Kết Nối (Logout)"))
            {
                ConnectionManager.Instance.Disconnect();
            }
            
            // Hiện nút test tìm trận nếu là Client
            if (Unity.Netcode.NetworkManager.Singleton.IsClient && !Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                if (GUILayout.Button("Tìm Trận 1v1 (Matchmaking)"))
                {
                    var player = Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
                    if (player != null) player.CmdFindMatchServerRpc();
                }
            }
            return;
        }

        // --- GIAO DIỆN CHƯA KẾT NỐI ---
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUILayout.Box("MÀN HÌNH TEST ĐĂNG NHẬP (LOCALHOST)");

        GUILayout.Label("Username:");
        username = GUILayout.TextField(username);

        GUILayout.Label("Password:");
        password = GUILayout.TextField(password);

        GUILayout.Space(10);

        if (GUILayout.Button("1. Chạy MÁY CHỦ (Start Dedicated Server)", GUILayout.Height(40)))
        {
            ConnectionManager.Instance.StartDedicatedServer();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("2. Đăng Nhập (Start Client)", GUILayout.Height(40)))
        {
            ConnectionManager.Instance.StartClient(username, password);
        }

        GUILayout.EndArea();
    }
}
