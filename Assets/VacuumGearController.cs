using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

public class VacuumGearController : MonoBehaviour
{
    private List<Item> grabableItems = new List<Item>();
    private BeastStorageContainerController storageContainer;
    private GameObject vacuumHead;
    private PhotonView pv;
    public List<Item> objectsInVauumRange = new List<Item>();
    // Start is called before the first frame update
    void Start()
    {
        vacuumHead = transform.GetChild(transform.childCount - 1).gameObject;
        pv = GetComponent<PhotonView>();
        storageContainer = GetComponent<BeastStorageContainerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vacuum();
        }
    }

    void Vacuum()
    {
        Debug.Log("### Vacuuming");
        if (objectsInVauumRange != null)
        {
            foreach (Item item in objectsInVauumRange)
            {
                Debug.Log("### calling vacuum RPC");
                pv.RPC("SuckUpItem_RPC", RpcTarget.All, item.spawnId);
            }
        }
    }

    [PunRPC]
    void SuckUpItem_RPC(string spawnId)
    {
        Debug.Log("### in vacuum RPC");

        Item _itemToVacuum = LevelManager.Instance.allItems.FindAll(x => x.spawnId == spawnId)[0];
        StartCoroutine(SuckUpItem(_itemToVacuum));
    }

    IEnumerator SuckUpItem(Item item)
    {
        float time = 0;
        float duration = 1;
        Vector3 startPos = item.transform.position;
        Vector3 endPos = vacuumHead.transform.position;
        while (time < duration)
        {
            item.transform.position = Vector3.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        item.transform.position = endPos;
        if (PhotonNetwork.IsMasterClient)
        {
            storageContainer.AddItem(item);
            LevelManager.Instance.CallUpdateItemsRPC(item.spawnId);
        }
    }
    List<Item> GatherAllItemsInScene(float range = 7)
    {
        Item[] allItems = GameObject.FindObjectsOfType<Item>();
        Item closestItem;
        float closestDist = range;
        foreach (Item item in allItems)
        {

            if (!item.isEquipped && item.isEquipable)
            {
                float currentItemDist = Vector3.Distance(vacuumHead.transform.position + Vector3.up, item.gameObject.transform.position);
                if (currentItemDist < 3)
                {
                    if (currentItemDist < closestDist)
                    {
                        //TODO check for player direction as well to stop players from picking up unintended items

                        closestDist = currentItemDist;
                        closestItem = item;
                        Outline outline = closestItem.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = true;
                        }
                    }
                    else
                    {
                        if (item.GetComponent<Outline>() != null)
                        {
                            item.GetComponent<Outline>().enabled = false;
                        }
                    }
                    grabableItems.Add(item);//TODO a list?
                }
            }
        }

        if (grabableItems.Count <= 0)
            return null;
        else
            return grabableItems;
    }
}
