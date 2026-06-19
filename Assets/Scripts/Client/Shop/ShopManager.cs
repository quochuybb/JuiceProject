using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    

    private Dictionary<int, RecipeData> allRecipesDict = new Dictionary<int, RecipeData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RecipeData[] recipes = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
        foreach (var recipe in recipes)
        {
            if (!allRecipesDict.ContainsKey(recipe.recipeID))
            {
                allRecipesDict.Add(recipe.recipeID, recipe);
            }
        }
    }

    private void OnEnable()
    {
        NetworkPlayer.OnClientBuyRecipeResult += HandleBuyResult;
    }

    private void OnDisable()
    {
        NetworkPlayer.OnClientBuyRecipeResult -= HandleBuyResult;
    }

    private void HandleBuyResult(bool isSuccess, int recipeId, float newCoin)
    {
        if (isSuccess)
        {
            GameSession.currentCoin = newCoin;

            if (allRecipesDict.TryGetValue(recipeId, out RecipeData purchasedRecipe))
            {
                GameSession.inventoryList.Add(purchasedRecipe);
                if (GameSession.recipeList.Count < 5)
                {
                    GameSession.recipeList.Add(purchasedRecipe);
                }
                Debug.Log($"[ShopClient] Mua thành công {purchasedRecipe.recipeName}. Vàng còn: {newCoin}");
            }
            if (RecipeManager.instance != null)
            {
                RecipeManager.instance.RefreshAllRecipes();
            }

        }
        else
        {
            Debug.LogWarning("[ShopClient] Mua thất bại! Không đủ vàng hoặc lỗi server.");
            if (RecipeManager.instance != null)
            {
                RecipeManager.instance.RefreshAllRecipes();
            }
        }
    }

    public void BuyRecipe(RecipeData recipe)
    {
        // GameSession.currentCoin -= recipe.recipeCost;
        // if (GameSession.recipeList.Count < 5)
        // {
        //     Debug.Log("Have enough slot equip");
        //     GameSession.recipeList.Add(recipe);
        //     GameSession.inventoryList.Add(recipe);
        // }
        // else
        // {
        //     Debug.Log("Not have enough slot equip");
        //     GameSession.inventoryList.Add(recipe);
        // }
        NetworkPlayer.LocalInstance.CmdBuyRecipeServerRpc(recipe.recipeID);

    }
    public void UnequipRecipe(RecipeData recipe)
    {
        GameSession.recipeList.Remove(recipe);
    }
    public void EquipRecipe(RecipeData recipe)
    {
        GameSession.recipeList.Add(recipe);
    }
}
