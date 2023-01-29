using UnityEngine;

public class ItemManager : MonoBehaviour
{
    //All of the items that exist in the
    public GameObject[] itemList;
    //All of the objects spawned into the env
    public GameObject[] environmentItemList;

    public GameObject GetPrefabByItem(Item item)
    {
        foreach (GameObject _item in itemList)
        {
            if (_item.GetComponent<Item>().name == item.name)
            {
                return _item;
            }
        }
        return null;
    }
}
