using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HandCraftingRecipe", menuName = "CraftingRecipe/HandCraftingRecipe", order = 4)]
public class HandCraftingRecipe : ScriptableObject
{
    public Item[] ingredientsList = new Item[4];
    public GameObject product;
    public int quantity;
}

