using System.Linq;
using UnityEngine;

public class SpellCraftingManager : MonoBehaviour
{
    public SpellCirclePedestalInteraction[] m_Pedestals;
    public SpellCraftingRecipe[] m_Recipes;
    public SpellCircleAlterInteraction m_Alter;
    public void Start()
    {
        m_Pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        m_Alter = GetComponentInChildren<SpellCircleAlterInteraction>();
    }

    public void TrySellCraft()
    {
        Item[] currentIngredients = new Item[6];
        SpellCirclePedestalInteraction[] pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        Debug.Log("### trying to spell craft 2");
        for (int i = 0; i < pedestals.Length; i++)
        {
            Debug.Log("### currentIngredient " + i + " " + pedestals[i].currentItem);
            currentIngredients[i] = pedestals[i].currentItem;
        }
        foreach (SpellCraftingRecipe recipe in m_Recipes)
        {
            for (int i = 0; i < recipe.ingredientsList.Length; i++)
            {
                Debug.Log("### currentIngredient " + i + " " + recipe.ingredientsList[i]);
            }
            if (currentIngredients.SequenceEqual(recipe.ingredientsList))
            {
                //clearPedestals
                for (int i = 1; i < transform.childCount; i++)
                {
                    LevelManager.Instance.CallSpellCirclePedestalPRC(GetComponent<BuildingMaterial>().id, -1, i, true);
                }
                SpawnCraftingProduct(recipe.product);
                //spawn object
            }
        }
    }

    void SpawnCraftingProduct(GameObject product)
    {
        LevelManager.Instance.CallSpellCircleProducePRC(GetComponent<BuildingMaterial>().id, product.GetComponent<Item>().itemIndex);
    }
}
