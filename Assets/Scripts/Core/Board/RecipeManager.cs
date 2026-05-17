using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [SerializeField] private GameObject recipePrefab;
    [SerializeField] private Transform spawnPoint;
    private void Start()
    {
        SpawnRecipe();
    }

    public void SpawnRecipe()
    {
        RecipeData[] recipeDatas = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
        foreach (RecipeData recipeData in recipeDatas)
        {
            Debug.Log(recipeData.name);
            GameObject recipe = Instantiate(recipePrefab, spawnPoint);
            Debug.Log(recipe.name);
            recipe.GetComponent<RecipeUI>().recipeData = recipeData;
            recipe.GetComponent<RecipeUI>().SetUp();
        }
        
    }
}
