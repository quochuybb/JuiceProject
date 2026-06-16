using System;
using UnityEngine;
using Unity.Netcode;

public static class ServerPlayerManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        NetworkPlayer.OnServerPlayerSpawned += HandlePlayerSpawned;
        NetworkPlayer.OnServerPlayerDespawned += HandlePlayerDespawned;
        NetworkPlayer.OnServerMatchmakingRequested += HandleMatchmakingRequested;
        NetworkPlayer.OnServerMatchmakingCanceled += HandleMatchmakingCanceled;
        NetworkPlayer.OnServerSaveProgressRequested += HandleSaveProgress;
    }



    private static void HandlePlayerSpawned(NetworkPlayer player)
    {
        string username = ServerAuthManager.GetUsernameForClient(player.OwnerClientId);
        if (!string.IsNullOrEmpty(username))
        {
            player.PlayerUsername.Value = username;
            
            var accountInfo = DatabaseManager.Instance.GetAccount(username);
            if (accountInfo != null)
            {
                player.PlayerMMR.Value = accountInfo.MMR;
                
                if (!string.IsNullOrEmpty(accountInfo.SessionDataJSON))
                {
                    player.RpcLoadSessionClientRpc(accountInfo.SessionDataJSON);
                }
            }
        }
        
        ServerMatchmaker.Instance.RegisterPlayer(player);
    }

    private static void HandlePlayerDespawned(NetworkPlayer player)
    {
        ServerMatchmaker.Instance.UnregisterPlayer(player);
    }

    private static void HandleMatchmakingRequested(NetworkPlayer player)
    {
        ServerMatchmaker.Instance.AddToQueue(player);
    }

    private static void HandleMatchmakingCanceled(NetworkPlayer player)
    {
        ServerMatchmaker.Instance.RemoveFromQueue(player);
    }

    private static void HandleSaveProgress(NetworkPlayer player, string sessionJson)
    {
        try
        {
            Debug.Log("Save");
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionJson);
            DatabaseManager.Instance.SaveProgress(player.PlayerUsername.Value.ToString(), data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Server] Lỗi khi Client lưu tiến trình: {e.Message}");
        }
    }
}
