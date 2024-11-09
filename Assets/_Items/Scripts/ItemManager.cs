using UnityEngine;
using Photon.Pun;

public class ItemManager : MonoBehaviour
{
    //All of the items that exist in the
    public GameObject[] itemList;
    //All of the objects spawned into the env
    public GameObject[] environmentItemList;
    public GameObject[] beastGearList;
    public static ItemManager Instance;
    PhotonView pv;
    Vector3 dropPosition;
    int dropCounter;
    void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }
    public void CallDropItemRPC(int itemIndex, Vector3 dropPos)
    {
        pv.RPC("DropItemRPC", RpcTarget.AllBuffered, itemIndex, dropPos, Random.Range(0, 1000).ToString());
    }

    [PunRPC]
    public void DropItemRPC(int itemIndex, Vector3 dropPos, string spawnIdKey)
    {
        GameObject newItem = Instantiate(itemList[itemIndex], dropPos + (Vector3.up * 2), Quaternion.identity);
        newItem.GetComponent<Rigidbody>().useGravity = false;
        SpawnMotionDriver spawnMotionDriver = newItem.GetComponent<SpawnMotionDriver>();
        Item item = newItem.GetComponent<Item>();
        Debug.Log("### spawn Id " + item.spawnId);
        if (newItem.CompareTag("Tool"))
        {
            if (newItem.TryGetComponent<ToolItem>(out var tool))
            {
                tool.spawnId = $"{spawnIdKey}_{dropCounter}_{dropPos}";
            }
            if (newItem.TryGetComponent<BuildingMaterial>(out var mat))
            {
                mat.spawnId = $"{spawnIdKey}_{dropCounter}_{dropPos}";
            }
        }
        else
        {
            item.spawnId = $"{spawnIdKey}_{dropCounter}_{dropPos}";
        }
        item.hasLanded = false;
        item.GetComponent<MeshCollider>().convex = true;
        item.GetComponent<MeshCollider>().isTrigger = true;
        float distanceMod = .1f;
        if (dropPos == dropPosition)
        {

            spawnMotionDriver.Fall(new Vector3(0 + dropCounter * distanceMod, 10f, 1 + dropCounter * distanceMod));
            dropCounter++;
        }
        else
        {
            dropPosition = dropPos;
            spawnMotionDriver.Fall(new Vector3(0 + distanceMod, 10f, 1 + distanceMod));
            dropCounter = 0;
        }
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

    public int GetBeastGearIndex(BeastGear item)
    {
        for (int i = 0; i < beastGearList.Length; i++)
        {
            if (item.gearName == beastGearList[i].GetComponent<BeastGear>().gearName)
            {
                return i;
            }
        }
        return -1;
    }

    public GameObject GetItemGameObjectByItemIndex(int index)
    {
        return itemList[index];
    }

    public GameObject GetEnvironmentItemByIndex(int index)
    {
        return environmentItemList[index];
    }
    public GameObject GetBeastGearByIndex(int index)
    {
        return beastGearList[index];
    }
}
