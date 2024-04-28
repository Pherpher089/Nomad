using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    private ItemManager itemManager;
    public HandCraftingRecipe[] recipes;
    public static CraftingManager Instance;
    void Awake()
    {
        Instance = this;
        itemManager = FindObjectOfType<ItemManager>();
    }
    public HandCraftingRecipe CancelBuildCraft(int itemIndex)
    {
        foreach (HandCraftingRecipe rec in recipes)
        {
            if (rec.product.GetComponent<Item>().itemListIndex == itemIndex)
            {
                return rec;
            }
        }
        return null;
    }
    public GameObject[] TryCraft(int[] ingredients)
    {
        foreach (HandCraftingRecipe recipe in recipes)
        {
            if (ingredients.Length != recipe.ingredientsList.Length)
                continue; // Skip if the ingredient lists don't have the same number of items

            bool match = true;
            for (int i = 0; i < ingredients.Length; i++)
            {
                if (ingredients[i] != recipe.ingredientsList[i].itemListIndex)
                {
                    match = false; // If any ingredient doesn't match, mark it as a mismatch and break the loop
                    break;
                }
            }

            // If we have a match, return the corresponding item
            if (match)
            {
                GameObject[] returnProduct = new GameObject[recipe.quantity];
                returnProduct[0] = recipe.product;
                return returnProduct;
            }
        }

        // If no matching recipe is found, return null
        return null;
    }
}
