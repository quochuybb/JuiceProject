using UnityEngine;
using Unity.Netcode;

public class GameRoom : NetworkBehaviour
{
    private NetworkPlayer playerA;
    private NetworkPlayer playerB;

    public void Initialize(NetworkPlayer a, NetworkPlayer b)
    {
        playerA = a;
        playerB = b;

        // Báo cho 2 người chơi biết đối thủ của họ
        playerA.RpcMatchFoundClientRpc(playerB.PlayerUsername.Value.ToString(), playerB.PlayerMMR.Value);
        playerB.RpcMatchFoundClientRpc(playerA.PlayerUsername.Value.ToString(), playerA.PlayerMMR.Value);

        Debug.Log($"[GameRoom] Phòng đã khởi tạo cho {playerA.PlayerUsername.Value} và {playerB.PlayerUsername.Value}");
        
        // TODO: Sinh ra bàn cờ Puzzle trên Server (Server-Authoritative Board)
    }

    // Giả sử hàm này được gọi khi trò chơi kết thúc
    public void EndMatch(NetworkPlayer winner, NetworkPlayer loser)
    {
        Debug.Log($"[GameRoom] Trận đấu kết thúc. {winner.PlayerUsername.Value} thắng!");

        // Tính toán MMR mới (Ví dụ đơn giản: +25 thắng, -25 thua)
        int newWinnerMMR = winner.PlayerMMR.Value + 25;
        int newLoserMMR = Mathf.Max(0, loser.PlayerMMR.Value - 25);

        // Lưu vào Database (Đã chuyển sang Web API - Sẽ xử lý ở Phase 4)
        // DatabaseManager.Instance.UpdateMMR(winner.PlayerUsername.Value.ToString(), newWinnerMMR);
        // DatabaseManager.Instance.UpdateMMR(loser.PlayerUsername.Value.ToString(), newLoserMMR);

        // Cập nhật lại NetworkVariable cho 2 người chơi
        winner.PlayerMMR.Value = newWinnerMMR;
        loser.PlayerMMR.Value = newLoserMMR;

        // Xóa phòng
        NetworkObject.Despawn();
    }

    public override void OnNetworkDespawn()
    {
        // Đặt lại trạng thái Matchmaking của 2 người chơi
        if (playerA != null) playerA.CmdCancelMatchServerRpc();
        if (playerB != null) playerB.CmdCancelMatchServerRpc();
        
        Debug.Log("[GameRoom] Phòng đã giải tán.");
    }
}
