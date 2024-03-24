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

    public void TrySpellCraft()
    {
        Debug.Log("### TrySellCraft");
        Item[] currentIngredients = new Item[6];
        SpellCirclePedestalInteraction[] pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        for (int i = 0; i < pedestals.Length; i++)
        {
            currentIngredients[i] = pedestals[i].currentItem;
        }
        foreach (SpellCraftingRecipe recipe in m_Recipes)
        {
            Debug.Log("### checking");

            if (currentIngredients.SequenceEqual(recipe.ingredientsList))
            {
                Debug.Log("### confirmed");

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
