using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RecipeData", menuName = "JuiceProject/RecipeData", order = 1)]
public class RecipeData : ScriptableObject
{
    public int recipeID;
    public string recipeName;
    public int foodFirst;
    public int foodSecond;
    public float recipeCost;
    public string description;
}
