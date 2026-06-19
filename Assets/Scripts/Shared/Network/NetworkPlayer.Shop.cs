using System;
using Unity.Netcode;
using UnityEngine;

public partial class NetworkPlayer : NetworkBehaviour
{
    public static event Action<NetworkPlayer, int> OnServerBuyRecipeRequested;
    
    public static event Action<bool, int, float> OnClientBuyRecipeResult;

    [ServerRpc]
    public void CmdBuyRecipeServerRpc(int recipeId)
    {
        OnServerBuyRecipeRequested?.Invoke(this, recipeId);
    }

    [ClientRpc]
    public void RpcBuyRecipeResultClientRpc(bool success, int recipeId, float newCoins)
    {
        if (IsOwner)
        {
            OnClientBuyRecipeResult?.Invoke(success, recipeId, newCoins);
        }
    }
}
