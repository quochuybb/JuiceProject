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

    public async void BuyRecipe(RecipeData recipe)
    {
        // 1. Kiểm tra có đủ vàng không
        if (GameSession.currentCoin >= recipe.recipeCost)
        {
            // Trừ vàng
            GameSession.currentCoin -= recipe.recipeCost;
            
            // 2. Thêm vào túi đồ (Inventory)
            GameSession.inventoryList.Add(recipe);

            // 3. Nếu khay trang bị (Equip) chưa đầy 5 món thì tự động mặc luôn
            if (GameSession.recipeList.Count < 5)
            {
                Debug.Log("[ShopClient] Còn chỗ trống! Tự động trang bị món đồ này.");
                GameSession.recipeList.Add(recipe);
            }
            else
            {
                Debug.Log("[ShopClient] Hết chỗ trang bị! Chỉ cất vào túi.");
            }

            // 4. Lưu dữ liệu lên Web API
            GameSessionData data = new GameSessionData();
            data.PackFromGameSession();
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            
            bool saveSuccess = await WebClientManager.Instance.SaveProgressAsync(jsonPayload);
            
            if (saveSuccess)
            {
                Debug.Log($"[ShopClient] Đã mua thành công {recipe.recipeName}!");
                // Cập nhật lại UI tiền vàng ở Main Menu
                if (MainMenuManager.Instance != null)
                {
                    MainMenuManager.Instance.UpdateCoinDisplay();
                }
                RecipeManager.instance.RefreshAllRecipes();
            }
            else
            {
                Debug.LogError("[ShopClient] Lỗi lưu game khi mua đồ. Hoàn tác...");
                // Hoàn tác nếu Server lỗi
                GameSession.currentCoin += recipe.recipeCost;
                GameSession.inventoryList.Remove(recipe);
                GameSession.recipeList.Remove(recipe);
                RecipeManager.instance.RefreshAllRecipes();
            }
        }
        else
        {
            Debug.LogWarning("[ShopClient] Mua thất bại! Không đủ vàng.");
            // Cập nhật lại UI đề phòng lỗi hiển thị
            RecipeManager.instance.RefreshAllRecipes();
        }
    }

    public async void UnequipRecipe(RecipeData recipe)
    {
        GameSession.recipeList.Remove(recipe);
        await SaveEquipState();
    }
    
    public async void EquipRecipe(RecipeData recipe)
    {
        GameSession.recipeList.Add(recipe);
        await SaveEquipState();
    }

    private async System.Threading.Tasks.Task SaveEquipState()
    {
        GameSessionData data = new GameSessionData();
        data.PackFromGameSession();
        string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        await WebClientManager.Instance.SaveProgressAsync(jsonPayload);
    }
}
