using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void BuyRecipe(RecipeData recipe)
    {
        GameSession.currentCoin -= recipe.recipeCost;
        if (GameSession.recipeList.Count < 5)
        {
            Debug.Log("Have enough slot equip");
            GameSession.recipeList.Add(recipe);
            GameSession.inventoryList.Add(recipe);
        }
        else
        {
            Debug.Log("Not have enough slot equip");
            GameSession.inventoryList.Add(recipe);
        }
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
