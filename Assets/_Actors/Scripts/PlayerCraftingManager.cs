using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
    0 = Stone Axe Head
*/
public class PlayerCraftingManager : MonoBehaviour
{
    public GameObject[] craftabelItems;
    Item craft(Item item1, Item item2)
    {
        switch (item1.itemName)
        {
            case "Stone":
                if (item2.itemName == "Stone")
                {
                    return craftabelItems[0].GetComponent<Item>(); //Stone Axe head
                }
                break;
        }
        return null;
    }
}
