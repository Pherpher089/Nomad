using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public ItemManager itemManager;
    public List<CraftingRecipe> craftingRecipesByIndex = new List<CraftingRecipe>();
    public static CraftingManager Instance;
    void Awake()
    {
        Instance = this;
        itemManager = FindObjectOfType<ItemManager>();
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 3 }, 4, 1)); //Primitive Stone Axe Head
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 4, 10, 2 }, 5, 1)); // Primitive Stone Axe
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 2, 7, 10 }, 6, 1)); //Primitive Torch
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 1, 1, 1, 1 }, 9, 1)); // Basic Crafting Bench
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 7, 7, 7, 7 }, 10, 1)); // Hemp Rope
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 3, 3, 3 }, 11, 1)); // Primitive Stone Sword Blade
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 11, 10, 2, 3 }, 12, 1)); // Primitive Stone Sword
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 1, 1, 3 }, 14, 1)); // Fire Pit
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 2, 2 }, 13, 1)); // Magic Stick
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 2, 3 }, 18, 3)); // Arrow
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 20, 10, 2 }, 19, 1)); // stone pick axe
        craftingRecipesByIndex.Add(new CraftingRecipe(new int[] { 3, 3, 3 }, 20, 1)); // stone pickaxe head 
    }
    public CraftingRecipe CancelBuildCraft(int itemIndex)
    {
        foreach (CraftingRecipe rec in craftingRecipesByIndex)
        {
            if (rec.producedItemIndex == itemIndex)
            {
                return rec;
            }
        }
        return null;
    }
    public GameObject[] TryCraft(int[] ingredients)
    {
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
