using UnityEngine;
using Photon.Pun;

public class ItemManager : MonoBehaviour
{
    //All of the items that exist in the
    public GameObject[] itemList;
    //All of the objects spawned into the env
    public GameObject[] environmentItemList;
    public static ItemManager Instance;
    PhotonView pv;
    void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }
    public void CallDropItemRPC(int itemIndex, Vector3 dropPos, bool isPacked = false)
    {
        pv.RPC("DropItemRPC", RpcTarget.AllBuffered, itemIndex, dropPos, isPacked);
    }

    [PunRPC]
    public void DropItemRPC(int itemIndex, Vector3 dropPos, bool isPacked)
    {
        GameObject newItem = Instantiate(itemList[itemIndex], dropPos + (Vector3.up * 2), Quaternion.identity);
        if (isPacked)
        {
            newItem.GetComponent<BuildingObject>().isPlaced = true;
            newItem.GetComponent<PackableItem>().PackAndSave(newItem);
        }
        newItem.GetComponent<Rigidbody>().useGravity = false;
        SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
        Item item = newItem.GetComponent<Item>();
        item.hasLanded = false;
        item.GetComponent<MeshCollider>().convex = true;
        item.GetComponent<MeshCollider>().isTrigger = true;
        spawnMotionDriver.Fall(new Vector3(0, 5f, 0));
    }

    public GameObject GetPrefabByItem(Item item)
    {
        foreach (GameObject _item in itemList)
        {
            Item newItem = _item.GetComponent<Item>();
            if (newItem.itemName == item.itemName)
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
