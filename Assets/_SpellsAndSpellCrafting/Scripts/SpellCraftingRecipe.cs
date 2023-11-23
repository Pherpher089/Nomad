using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SpellCraftingRecipe", menuName = "CraftingRecipe/SpellCraftingRecipe", order = 0)]
public class SpellCraftingRecipe : ScriptableObject
{
    public Item[] ingredientsList;
    public GameObject product;
    public int quantity;
}
