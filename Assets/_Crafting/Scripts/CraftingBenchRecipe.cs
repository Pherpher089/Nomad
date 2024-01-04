using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CraftingBenchRecipe", menuName = "CraftingRecipe/CraftingBenchRecipe", order = 3)]
public class CraftingBenchRecipe : ScriptableObject
{
    public Item[] ingredientsList = new Item[9];
    public GameObject product;
    public int quantity;
}
