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
        Debug.Log("### here 1");
        SpellCirclePedestalInteraction[] pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        Item[] currentIngredients = new Item[pedestals.Length];
        Debug.Log("### here 2");
        for (int i = 0; i < pedestals.Length; i++)
        {
            currentIngredients[i] = pedestals[i].currentItem;
        }
        Debug.Log("### here 3");

        foreach (SpellCraftingRecipe recipe in m_Recipes)
        {
            Debug.Log("### here 3.5");

            if (currentIngredients.SequenceEqual(recipe.ingredientsList))
            {
                Debug.Log("### here 3.7");

                //clearPedestals
                for (int i = 1; i < transform.childCount; i++)
                {
                    Debug.Log("### here 4");
                    LevelManager.Instance.CallSpellCirclePedestalPRC(GetComponent<BuildingMaterial>().id, -1, i, true);
                }
                Debug.Log("### here 5");
                SpawnCraftingProduct(recipe.product);
                //spawn object
            }
        }
    }

    void SpawnCraftingProduct(GameObject product)
    {
        LevelManager.Instance.CallSpellCircleProducePRC(GetComponent<BuildingMaterial>().id, product.GetComponent<Item>().itemListIndex);
    }
}
