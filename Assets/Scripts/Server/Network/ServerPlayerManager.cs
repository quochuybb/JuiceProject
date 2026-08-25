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
        NetworkPlayer.OnServerSaveProgressRequested += HandleSaveProgress;
    }



    private static void HandlePlayerSpawned(NetworkPlayer player)
    {
        string username = ServerAuthManager.GetUsernameForClient(player.OwnerClientId);
        if (!string.IsNullOrEmpty(username))
        {
            player.PlayerUsername.Value = username;
        }
        
    }
    private static void HandlePlayerDespawned(NetworkPlayer player)
    {

    }

    private static void HandleSaveProgress(NetworkPlayer player, string sessionJson)
    {
        try
        {
            Debug.LogWarning("[Server] Tính năng Save trên Server đã bị tắt! Client tự gọi Web API để save.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Server] Lỗi khi Client lưu tiến trình: {e.Message}");
        }
    }
}
