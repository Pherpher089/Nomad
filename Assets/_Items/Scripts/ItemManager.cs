using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    //All of the items that exist in the
    public GameObject[] itemList;
    //All of the objects spawned into the env
    public GameObject[] environmentItemList;
    public static ItemManager Instance;
    void Awake()
    {
        Instance = this;
    }


    public GameObject GetPrefabByItem(Item item)
    {
        foreach (GameObject _item in itemList)
        {
            if (_item.GetComponent<Item>().itemName == item.itemName)
            {
                return _item;
            }
        }
        return null;
    }

    public int GetEnvItemIndex(GameObject obj)
    {
        for (int i = 0; i < environmentItemList.Length; i++)
        {
            if (obj.name.Replace("(Clone)", "") == environmentItemList[i].name)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetItemIndex(GameObject obj)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (obj.name.Replace("(Clone)", "") == itemList[i].name)
            {
                return i;
            }
        }
        return -1;
    }
    public int GetItemIndex(Item item)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (item.itemName == itemList[i].GetComponent<Item>().itemName)
            {
                return i;
            }
        }
        return -1;
    }

    public GameObject GetItemByIndex(int index)
    {
        return itemList[index];
    }

    public GameObject GetEnvironmentItemByIndex(int index)
    {
        return environmentItemList[index];
    }
}
