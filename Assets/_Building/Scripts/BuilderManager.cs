using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using Unity.Mathematics;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
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
    private Stack<BuildAction> buildActions = new();
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

    public void Build(ThirdPersonUserControl player, BuildingMaterial material, bool fromCraft = false)
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
                player.lastBuildPosition = Vector3.Distance(player.transform.position, deltaPosition) > 15 ? player.transform.position + (player.transform.forward * 2) : deltaPosition;
                // Instantiate the prefab at the calculated position with the same rotation as the player.
                if (player.lastBuildPosition.y < player.transform.position.y)
                {
                    player.lastBuildPosition = new Vector3((int)player.lastBuildPosition.x, (int)player.transform.position.y + 1f, (int)player.lastBuildPosition.z);
                }
                //currentBuildObject = Instantiate(m_buildObject, player.lastBuildPosition, player.lastBuildRotation);
                currentBuildObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BuilderObject"), player.lastBuildPosition, player.lastBuildRotation);
                ObjectBuildController buildController = currentBuildObject.GetComponent<ObjectBuildController>();
                if (fromCraft)
                {
                    buildController.buildCooldown = true;
                }
                buildController.currentBuildPieceIndex = index;
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

    public void AddBuildAction(BuildActionType actionType, Vector3 lastPosition, float lastRotation, int itemIndex, string id)
    {
        // Check if the stack has reached the max limit
        if (buildActions.Count >= 10)
        {
            // Step 1: Transfer elements from Stack to Queue to reverse the order
            Queue<BuildAction> tempQueue = new Queue<BuildAction>();
            while (buildActions.Count > 0)
            {
                tempQueue.Enqueue(buildActions.Pop());
            }

            // Step 2: Remove the oldest item (front of the queue)
            tempQueue.Dequeue();

            // Step 3: Move elements back from Queue to Stack in correct order
            while (tempQueue.Count > 0)
            {
                buildActions.Push(tempQueue.Dequeue());
            }
        }

        // Add the new item to the top of the stack
        buildActions.Push(new BuildAction(actionType, lastPosition, lastRotation, id, itemIndex));
    }
    public void UndoBuildAction()
    {
        if (!isBuilding && buildActions.Count > 0) return;
        BuildAction buildAction = null;

        while (buildAction == null && buildActions.Count > 0)
        {
            BuildAction newBuildAction = buildActions.Pop();
            if (newBuildAction != null)
            {
                buildAction = newBuildAction;
            }
            else
            {
                if (buildActions.Count <= 0)
                {
                    return;
                }
            }
        }
        SourceObject[] allSourceObjects;
        switch (buildAction.buildActionType)
        {
            case BuildActionType.Add:
                allSourceObjects = FindObjectsOfType<SourceObject>();
                foreach (SourceObject srcObj in allSourceObjects)
                {
                    if (srcObj.id == buildAction.objectId)
                    {
                        srcObj.TakeDamage(srcObj.hitPoints, srcObj.properTool, srcObj.transform.position, this.gameObject);
                    }
                }
                break;
            case BuildActionType.Remove:
                LevelManager.Instance.CallPlaceObjectPRC(buildAction.itemIndex, buildAction.lastPosition, new Vector3(0, buildAction.lastRotation, 0), buildAction.objectId, false);
                break;
            case BuildActionType.Move:
                allSourceObjects = FindObjectsOfType<SourceObject>();
                foreach (SourceObject srcObj in allSourceObjects)
                {
                    if (srcObj.id == buildAction.objectId)
                    {
                        srcObj.transform.position = buildAction.lastPosition;
                        srcObj.transform.rotation = Quaternion.Euler(0, buildAction.lastRotation, 0);
                        srcObj.id = GenerateObjectId.GenerateSourceObjectId(srcObj);
                    }
                }
                break;
        }
    }

    public void CancelBuild(ThirdPersonUserControl user)
    {
        isBuilding = false;
        ObjectBuildController obc = currentBuildObject.GetComponent<ObjectBuildController>();
        if (obc.currentlySelectedBuildPiece.id != "")
        {
            if (obc.currentlySelectedBuildPiece.isSourceObject)
            {
                LevelManager.Instance.CallShutOffObjectRPC(obc.currentlySelectedBuildPiece.id);
            }
            else
            {
                LevelManager.Instance.CallShutOffBuildingMaterialRPC(obc.currentlySelectedBuildPiece.id);
            }
            LevelManager.Instance.CallPlaceObjectPRC(obc.currentlySelectedBuildPiece.itemIndex, obc.currentlySelectedBuildPiece.curPos, obc.currentlySelectedBuildPiece.curRotEuler, obc.currentlySelectedBuildPiece.id, false);
            obc.currentlySelectedBuildPiece = new();
        }
        else
        {
            if (currentBuildObject.transform.GetChild(obc.currentBuildPieceIndex).TryGetComponent<Item>(out var buildItem))
            {
                HandCraftingRecipe returnObjectInfo = CraftingManager.Instance.CancelBuildCraft(buildItem.itemListIndex);
                ActorEquipment ae = user.GetComponent<ActorEquipment>();
                foreach (Item item in returnObjectInfo.ingredientsList)
                {
                    ae.AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(item.itemListIndex).GetComponent<Item>());
                }
            }
        }
        GameStateManager.Instance.enableBuildSnapping = obc.previousSnappingState;
        if (GameStateManager.Instance.currentTent != null && FindObjectsOfType<ObjectBuildController>().Length == 1)
        {
            GameStateManager.Instance.currentTent.TurnOffBoundsVisuals();
        }
        PhotonNetwork.Destroy(currentBuildObject.GetPhotonView());
    }

}

public enum BuildActionType { Add = 0, Remove = 1, Move = 2 }
public class BuildAction
{
    //for adds
    public string objectId;
    public Vector3 lastPosition;
    public float lastRotation;
    public int itemIndex;
    public BuildActionType buildActionType;

    public BuildAction(BuildActionType buildActionType, Vector3 lastPosition, float lastRotation, string objectId, int itemIndex)
    {
        this.buildActionType = buildActionType;
        this.objectId = objectId;
        this.lastPosition = lastPosition;
        this.lastRotation = lastRotation;
        this.itemIndex = itemIndex;
        this.objectId = objectId;

    }

}
