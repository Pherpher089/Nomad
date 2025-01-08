using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StationCraftingRecipe", menuName = "CraftingRecipe/StationCraftingRecipe", order = 0)]
public class StationCraftingRecipe : ScriptableObject
{
    public Item[] ingredientsList;
    public GameObject product;
    public int quantity;
}
