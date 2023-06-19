using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public ItemManager itemManager;
    public List<CraftingRecipe> craftingRecipesByIndex = new List<CraftingRecipe>();

    void Awake()
    {
        itemManager = FindObjectOfType<ItemManager>();
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 3 }, 4, 1)); //Primitive Stone Axe Head
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 4, 2 }, 5, 1)); // Primitive Stone Axe
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 2, 7, 10 }, 6, 1)); //Primitive Torch
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 1, 1, 1, 1 }, 9, 1)); // Basic Crafting Bench
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 7, 7, 7, 7 }, 10, 1)); // Hemp Rope
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 3, 3, 3 }, 11, 1)); // Primitive Stone Sword Blade
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 11, 10, 2, 3 }, 12, 1)); // Primitive Stone Sword
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 1, 1, 3 }, 13, 1)); // Fire Pit


    }

    public GameObject[] TryCraft(int[] ingredients)
    {
        Debug.Log($"### trying to craft {ingredients}");
        foreach (CraftingRecipe recipe in craftingRecipesByIndex)
        {
            if (ingredients.Length != recipe.ingredients.Length)
                continue; // Skip if the ingredient lists don't have the same number of items

            bool match = true;
            for (int i = 0; i < ingredients.Length; i++)
            {
                if (ingredients[i] != recipe.ingredients[i])
                {
                    match = false; // If any ingredient doesn't match, mark it as a mismatch and break the loop
                    break;
                }
            }

            // If we have a match, return the corresponding item
            if (match)
            {
                GameObject[] returnProduct = new GameObject[recipe.quantity];
                returnProduct[0] = itemManager.itemList[recipe.producedItemIndex];
                return returnProduct;
            }
        }

        // If no matching recipe is found, return null
        return null;
    }
}

public class CraftingRecipe
{
    public int[] ingredients;
    public int producedItemIndex;
    public int quantity;

    public CraftingRecipe(int[] ingredients, int producedItemIndex, int quantity)
    {
        this.ingredients = ingredients;
        this.producedItemIndex = producedItemIndex;
        this.quantity = quantity;
    }
}
