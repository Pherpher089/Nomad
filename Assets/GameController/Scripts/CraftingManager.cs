using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public GameObject[] craftableItems;
    public Vector3[] craftingRecipesByIndex;
    Vector2 ingredientIds = new Vector2(-1, -1);
    public GameObject TryCraft(GameObject item1, GameObject item2)
    {
        for (int i = 0; i < craftableItems.Length; i++)
        {
            string objectName1 = item1.name.Replace("(Clone)", "");
            string objectName2 = item2.name.Replace("(Clone)", "");
            Debug.Log("### " + objectName1 + " ?= " + craftableItems[i].name);
            Debug.Log("### " + objectName2 + " ?= " + craftableItems[i].name);

            if (craftableItems[i].name == objectName1)
            {
                Debug.Log("true for 1");
                ingredientIds.x = i;
            }

            if (craftableItems[i].name == objectName2)
            {
                Debug.Log("true for 1");
                ingredientIds.y = i;
            }
        }
        if (ingredientIds.x == -1 || ingredientIds.y == -1)
        {
            return null;
        }

        for (int i = 0; i < craftingRecipesByIndex.Length; i++)
        {
            if (craftingRecipesByIndex[i].x == ingredientIds.x && craftingRecipesByIndex[i].y == ingredientIds.y || craftingRecipesByIndex[i].x == ingredientIds.y && craftingRecipesByIndex[i].y == ingredientIds.x)
            {
                return craftableItems[(int)craftingRecipesByIndex[i].z];
            }
        }
        return null;
    }
}
