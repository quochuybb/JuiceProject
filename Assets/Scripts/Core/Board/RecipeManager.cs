using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager instance { get; private set; }
    [SerializeField] private GameObject recipePrefab;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SpawnRecipe();
    }

    public void SpawnRecipe()
    {
        RecipeData[] recipeDatas = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
        foreach (RecipeData recipeData in recipeDatas)
        {
            GameObject recipe = Instantiate(recipePrefab, spawnPoint);
            recipe.GetComponent<RecipeUI>().recipeData = recipeData;
            recipe.GetComponent<RecipeUI>().SetUp();
        }
    }

    public void RefreshAllRecipes()
    {
        foreach (Transform child in spawnPoint)
        {
            RecipeUI recipeUI = child.GetComponent<RecipeUI>();
            if (recipeUI != null)
            {
                recipeUI.SetUp();
            }
        }
    }
    public RecipeData GetMatchingRecipe(int val1, int val2)
    {
        if (GameSession.recipeList == null || GameSession.recipeList.Count == 0)
            return null;

        foreach (RecipeData recipe in GameSession.recipeList)
        {
            if (recipe == null) continue;

            if ((recipe.foodFirst == val1 && recipe.foodSecond == val2) ||
                (recipe.foodFirst == val2 && recipe.foodSecond == val1))
            {
                return recipe; 
            }
        }

        return null;
    }
}
