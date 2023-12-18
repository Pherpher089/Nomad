using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BeastSaddleCraftingRecipe", menuName = "CraftingRecipe/BeastSaddleCraftingRecipe", order = 2)]

public class BeastSaddleCraftingRecipe : ScriptableObject
{

    public Item[] ingredientsList;
    public GameObject product;
    public int quantity;
}
