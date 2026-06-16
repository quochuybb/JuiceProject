using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RecipeStatus
{
    BUY,
    EQUIPMENT,
    UNEQUIPMENT,
}
public class RecipeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI recipeDescription;
    [SerializeField] private TextMeshProUGUI recipeCost;
    public RecipeData recipeData;
    public RecipeStatus status;
    [SerializeField] private Button buyButton;
    private void Start()
    {
        buyButton.onClick.AddListener(OnBuyBtnClick);
        // Xóa dòng gán status = BUY ở đây vì SetUp đã xử lý
    }
    public void SetUp()
    {
        recipeName.text = recipeData.name;
        recipeDescription.text = recipeData.description;
        recipeCost.text = recipeData.recipeCost.ToString();

        // Kiểm tra xem món đồ này đã nằm trong túi đồ hoặc đang được trang bị chưa
        bool isEquipped = GameSession.recipeList.Contains(recipeData);
        bool inInventory = GameSession.inventoryList.Contains(recipeData);

        if (isEquipped)
        {
            status = RecipeStatus.EQUIPMENT; // Trong code cũ, EQUIPMENT có nghĩa là ĐANG trang bị (click để gỡ)
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unequip";
        }
        else if (inInventory)
        {
            status = RecipeStatus.UNEQUIPMENT; // Trong code cũ, UNEQUIPMENT có nghĩa là CHƯA trang bị (click để lắp)
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
        }
        else
        {
            status = RecipeStatus.BUY;
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
        }
    }

    private void OnBuyBtnClick()
    {
        if (GameSession.recipeList.Count == 5)
        {
            GameSession.isFull = true;
        }
        else
        {
            GameSession.isFull = false;
        }
        if (status == RecipeStatus.BUY)
        {
            if (GameSession.currentCoin >= recipeData.recipeCost)
            {
                ShopManager.Instance.BuyRecipe(recipeData);
                if (!GameSession.isFull)
                {
                    buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unequip";
                    status = RecipeStatus.EQUIPMENT;
                }
                else 
                {
                    buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
                    status = RecipeStatus.UNEQUIPMENT;
                }
            }
            else
            {
                Debug.Log("Not enough coins to buy " + recipeData.recipeName);    
            }
        }
        else if (status == RecipeStatus.EQUIPMENT)
        {
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
            status = RecipeStatus.UNEQUIPMENT;
            ShopManager.Instance.UnequipRecipe(recipeData);
        }
        else if (status == RecipeStatus.UNEQUIPMENT && !GameSession.isFull)
        {
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unequip";
            status = RecipeStatus.EQUIPMENT;
            ShopManager.Instance.EquipRecipe(recipeData);
        }
    }
}
