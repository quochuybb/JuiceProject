using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameRoom
{
    private const int COLUMNS = 9;

    public string RoomId { get; private set; }
    public ulong Player1Id { get; private set; }
    public ulong Player2Id { get; private set; }
    
    public int Player1HP { get; set; } = 1000;
    public int Player2HP { get; set; } = 1000;
    
    public int BoardSeed { get; private set; }
    public List<CellData> Player1Board { get; private set; }
    public List<CellData> Player2Board { get; private set; }

    public GameRoom(string roomId, ulong player1Id, ulong player2Id)
    {
        RoomId = roomId;
        Player1Id = player1Id;
        Player2Id = player2Id;

        BoardSeed = Random.Range(1000, 999999);
        Random.InitState(BoardSeed);
        Player1Board = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        Random.InitState(BoardSeed);
        Player2Board = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        Debug.Log($"[GameRoom] Created Room {RoomId}. Seed: {BoardSeed}");
    }

    public void HandleAttack(ulong attackerId, int damage)
    {
        if (attackerId == Player1Id)
        {
            Player2HP -= damage;
            if (Player2HP < 0) Player2HP = 0;
        }
        else if (attackerId == Player2Id)
        {
            Player1HP -= damage;
            if (Player1HP < 0) Player1HP = 0;
        }
        
        Debug.Log($"[GameRoom] {RoomId} - HP: P1({Player1HP}) vs P2({Player2HP})");

        var player1Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player1Id);
        if (player1Obj != null)
        {
            var p1 = player1Obj.GetComponent<NetworkPlayer>();
            ClientRpcParams p1Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player1Id } } };
            p1.RpcUpdateHPClientRpc(Player1HP, Player2HP, p1Params);
        }

        var player2Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player2Id);
        if (player2Obj != null)
        {
            var p2 = player2Obj.GetComponent<NetworkPlayer>();
            ClientRpcParams p2Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player2Id } } };
            p2.RpcUpdateHPClientRpc(Player2HP, Player1HP, p2Params); 
        }

        CheckWinCondition();
    }
    
    private void CheckWinCondition()
    {
        if (Player1HP <= 0 || Player2HP <= 0)
        {
            Debug.Log($"[GameRoom] Trận đấu kết thúc ở phòng {RoomId}! Đang gửi kết quả lên Node.js...");

            ulong winnerClientId = Player1HP > 0 ? Player1Id : Player2Id;
            ulong loserClientId = Player1HP <= 0 ? Player1Id : Player2Id;

            string winnerName = ServerAuthManager.GetUsernameForClient(winnerClientId);
            string loserName = ServerAuthManager.GetUsernameForClient(loserClientId);

            ServerMatchManager.Instance.SubmitMatchResult(winnerName, loserName, (winnerMmr, loserMmr) =>
            {
                var player1Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player1Id);
                if (player1Obj != null)
                {
                    var p1 = player1Obj.GetComponent<NetworkPlayer>();
                    ClientRpcParams p1Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player1Id } } };
                    int myMmr = (Player1Id == winnerClientId) ? winnerMmr : loserMmr;
                    p1.RpcEndMatchClientRpc(Player1HP > 0, myMmr, p1Params);
                }

                var player2Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player2Id);
                if (player2Obj != null)
                {
                    var p2 = player2Obj.GetComponent<NetworkPlayer>();
                    ClientRpcParams p2Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player2Id } } };
                    int myMmr = (Player2Id == winnerClientId) ? winnerMmr : loserMmr;
                    p2.RpcEndMatchClientRpc(Player2HP > 0, myMmr, p2Params);
                }
            });
        }
    }
}
