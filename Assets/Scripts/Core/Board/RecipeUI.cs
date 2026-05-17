using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipeDescription;
    [SerializeField] private TextMeshProUGUI recipeCost;
    public RecipeData recipeData;

    public void SetUp()
    {
        recipeName.text = recipeData.name;
        recipeDescription.text = recipeData.description;
        recipeCost.text = recipeData.recipeCost.ToString();
    }
}
