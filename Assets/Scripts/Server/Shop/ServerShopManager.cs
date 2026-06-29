using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerShopManager : MonoBehaviour
{
    private Dictionary<int, RecipeData> allRecipesDict = new Dictionary<int, RecipeData>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        GameObject shopManagerGo = new GameObject("ServerShopManager");
        shopManagerGo.AddComponent<ServerShopManager>();
        DontDestroyOnLoad(shopManagerGo);
    }

    private void Awake()
    {
        RecipeData[] recipes = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
        foreach (var recipe in recipes)
        {
            if (!allRecipesDict.ContainsKey(recipe.recipeID))
            {
                allRecipesDict.Add(recipe.recipeID, recipe);
            }
        }
        Debug.Log($"[ServerShopManager] Đã load {allRecipesDict.Count} recipes vào Server.");
    }

    private void OnEnable()
    {
        NetworkPlayer.OnServerBuyRecipeRequested += HandleBuyRecipeRequested;
    }

    private void OnDisable()
    {
        NetworkPlayer.OnServerBuyRecipeRequested -= HandleBuyRecipeRequested;
    }

    private void HandleBuyRecipeRequested(NetworkPlayer player, int recipeId)
    {
        string username = player.PlayerUsername.Value.ToString();
        
        if (!allRecipesDict.TryGetValue(recipeId, out RecipeData recipeToBuy))
        {
            Debug.LogWarning($"[ServerShop] User {username} cố mua đồ không tồn tại ID: {recipeId}");
            player.RpcBuyRecipeResultClientRpc(false, recipeId, 0);
            return;
        }

        /* TÍNH NĂNG ĐÃ CHUYỂN QUA WEB API
        GameSessionData sessionData = DatabaseManager.Instance.LoadProgress(username);
        if (sessionData == null)
        {
            Debug.LogError($"[ServerShop] Lỗi không tìm thấy data của {username}");
            player.RpcBuyRecipeResultClientRpc(false, recipeId, 0);
            return;
        }

        if (sessionData.inventoryListIDs.Contains(recipeId))
        {
            Debug.LogWarning($"[ServerShop] User {username} đã có món đồ {recipeId} rồi!");
            player.RpcBuyRecipeResultClientRpc(false, recipeId, sessionData.currentCoin);
            return;
        }

        if (sessionData.currentCoin >= recipeToBuy.recipeCost)
        {
            sessionData.currentCoin -= recipeToBuy.recipeCost;
            
            sessionData.inventoryListIDs.Add(recipeId);

            if (sessionData.recipeListIDs.Count < 5)
            {
                sessionData.recipeListIDs.Add(recipeId);
            }

            DatabaseManager.Instance.SaveProgress(username, sessionData);

            Debug.Log($"[ServerShop] {username} mua thành công {recipeToBuy.recipeName}. Vàng còn: {sessionData.currentCoin}");

            player.RpcBuyRecipeResultClientRpc(true, recipeId, sessionData.currentCoin);
        }
        else
        {
            Debug.LogWarning($"[ServerShop] {username} KHÔNG ĐỦ TIỀN mua {recipeToBuy.recipeName}");
            player.RpcBuyRecipeResultClientRpc(false, recipeId, sessionData.currentCoin);
        }
        */
    }
}
