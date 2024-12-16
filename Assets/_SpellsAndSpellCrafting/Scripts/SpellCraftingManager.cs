using System.Linq;
using TMPro;
using UnityEngine;

public class SpellCraftingManager : MonoBehaviour
{
    public SpellCirclePedestalInteraction[] m_Pedestals;
    public SpellCraftingRecipe[] m_Recipes;
    public SpellCircleAlterInteraction m_Alter;

    // For the beast stable
    SaddleStationUIController saddleStation;
    public TMP_Text uiMessage;

    public void Start()
    {
        m_Pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        m_Alter = GetComponentInChildren<SpellCircleAlterInteraction>();
        //For beast stable
        saddleStation = transform.GetComponentInChildren<SaddleStationUIController>();
        if (name.ToLower().Contains("beaststable"))
        {
            uiMessage = transform.GetChild(11).GetChild(0).GetComponent<TMP_Text>();
            uiMessage.text = "";
        }
    }

    public void TrySpellCraft()
    {
        SpellCirclePedestalInteraction[] pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        Item[] currentIngredients = new Item[pedestals.Length];
        for (int i = 0; i < pedestals.Length; i++)
        {
            currentIngredients[i] = pedestals[i].currentItem;
        }

        foreach (SpellCraftingRecipe recipe in m_Recipes)
        {

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
        if (name.Contains("BeastStable"))
        {
            string message = saddleStation.AddItem(product.GetComponent<BeastGear>());
            uiMessage.text = message;
        }
        else
        {
            LevelManager.Instance.CallSpellCircleProducePRC(GetComponent<BuildingMaterial>().id, product.GetComponent<Item>().itemListIndex);
        }
    }
}
