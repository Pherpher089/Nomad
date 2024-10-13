using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class BuilderManager : MonoBehaviour
{
    [HideInInspector]
    public bool isBuilding = false;
    private GameObject m_buildObject;
    public BuildableItemIndexRange[] materialIndices;
    private GameObject[] m_buildPieces;
    private int childCount;
    private GameObject currentBuildObject;
    // Start is called before the first frame update
    void Awake()
    {
        m_buildObject = (GameObject)Resources.Load("PhotonPrefabs/BuilderObject");
        childCount = m_buildObject.transform.childCount;
        m_buildPieces = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            m_buildPieces[i] = m_buildObject.transform.GetChild(i).gameObject;
        }
        SelectBuildObject(0);
    }


    void SelectBuildObject(int index)
    {

        for (int i = 0; i < childCount; i++)
        {
            if (i == index)
            {
                m_buildPieces[i].SetActive(true);
            }
            else
            {
                m_buildPieces[i].SetActive(false);
            }
        }
    }

    public void Build(ThirdPersonUserControl player, BuildingMaterial material)
    {
        foreach (BuildableItemIndexRange buildRange in materialIndices)
        {
            if (buildRange.buildingMaterial.itemListIndex == material.itemListIndex)
            {
                // Key exists, value is stored in the "value" variable
                isBuilding = true;
                int index = player.lastBuildIndex > buildRange.buildableItemIndexRange.x && player.lastBuildIndex < buildRange.buildableItemIndexRange.y ? player.lastBuildIndex : (int)buildRange.buildableItemIndexRange.x;
                SelectBuildObject(index);
                Vector3 deltaPosition = player.lastBuildPosition + (player.lastBuildPosition - player.lastLastBuildPosition).normalized * 4;
                player.lastLastBuildPosition = player.lastBuildPosition;
                player.lastBuildPosition = Vector3.Distance(player.transform.position, deltaPosition) > 10 ? player.transform.position + (player.transform.forward * 2) : deltaPosition;
                // Instantiate the prefab at the calculated position with the same rotation as the player.
                if (player.lastBuildPosition.y < player.transform.position.y)
                {
                    player.lastBuildPosition = new Vector3((int)player.lastBuildPosition.x, (int)player.transform.position.y + 1f, (int)player.lastBuildPosition.z);
                }
                //currentBuildObject = Instantiate(m_buildObject, player.lastBuildPosition, player.lastBuildRotation);
                currentBuildObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BuilderObject"), player.lastBuildPosition, player.lastBuildRotation);
                ObjectBuildController buildController = currentBuildObject.GetComponent<ObjectBuildController>();
                buildController.itemIndex = index;
                buildController.itemIndexRange = buildRange.buildableItemIndexRange;
                buildController.player = player;
                buildController.CallInitializeBuildPicePRC(index, buildRange.buildableItemIndexRange);
                if (currentBuildObject.GetComponent<Outline>() != null)
                {
                    currentBuildObject.GetComponent<Outline>().enabled = false;
                }
                if (currentBuildObject.GetComponent<Item>() != null)
                {
                    currentBuildObject.GetComponent<Item>().isEquipable = false;
                }
                if (currentBuildObject.GetComponent<BuildingMaterial>() != null)
                {
                    currentBuildObject.GetComponent<BuildingMaterial>().isEquipable = false;
                }
            }

        }
    }

    public void CancelBuild(ThirdPersonUserControl user)
    {
        isBuilding = false;
        if (currentBuildObject.transform.GetChild(currentBuildObject.GetComponent<ObjectBuildController>().itemIndex).TryGetComponent<Item>(out var buildItem))
        {
            HandCraftingRecipe returnObjectInfo = CraftingManager.Instance.CancelBuildCraft(buildItem.itemListIndex);
            ActorEquipment ae = user.GetComponent<ActorEquipment>();
            foreach (Item item in returnObjectInfo.ingredientsList)
            {
                ae.AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(item.itemListIndex).GetComponent<Item>());
            }
        }
        if (GameStateManager.Instance.currentTent != null && FindObjectsOfType<ObjectBuildController>().Length == 1)
        {
            GameStateManager.Instance.currentTent.TurnOffBoundsVisuals();
        }
        PhotonNetwork.Destroy(currentBuildObject.GetPhotonView());
    }

}
