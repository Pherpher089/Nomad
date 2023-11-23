using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;


//TODO investigate and remove this class is it is not used anywhere. Looks like 0 scene usage.
public class BuilderManager : MonoBehaviour
{
    public bool isBuilding = false;
    public GameObject m_buildObject;
    private Dictionary<string, Vector2> materialIndices = new Dictionary<string, Vector2>();
    private GameObject[] m_buildPieces;
    private ThirdPersonCharacter playerCharacterController;
    private int childCount;
    private GameObject currentBuildObject;
    private float buildDistance = 3.5f;
    // Start is called before the first frame update
    void Awake()
    {
        playerCharacterController = GetComponent<ThirdPersonCharacter>();
        childCount = m_buildObject.transform.childCount;
        m_buildPieces = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            m_buildPieces[i] = m_buildObject.transform.GetChild(i).gameObject;
        }
        SelectBuildObject(0);
    }


    void Start()
    {
        m_buildObject = (GameObject)Resources.Load("Prefabs/BuilderObject");
        //This appears to be the range of items to cycle through for a given material
        materialIndices.Add("Chopped Logs", new Vector2(0, 5));
        materialIndices.Add("Basic Crafting Bench", new Vector2(5, 6));
        materialIndices.Add("Torch", new Vector2(6, 7));
        materialIndices.Add("Stone", new Vector2(7, 10));
        materialIndices.Add("Fire Pit", new Vector2(10, 11));
        materialIndices.Add("Chest", new Vector2(11, 12));
        materialIndices.Add("Spell Circle", new Vector2(12, 13));

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

    public void Build(ThirdPersonUserControl player, Item material)
    {
        if (materialIndices.TryGetValue(material.itemName, out Vector2 value))
        {
            // Key exists, value is stored in the "value" variable
            isBuilding = true;
            int index = player.lastBuildIndex > value.x && player.lastBuildIndex < value.y ? player.lastBuildIndex : (int)value.x;
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
            buildController.itemIndexRange = value;
            buildController.player = player;
            PackableItem packable = material.GetComponent<PackableItem>();
            if (packable != null && packable.packed)
            {
                //buildController.transform.GetChild(index).GetComponent<PackableItem>().PackAndSave(buildController.transform.GetChild(index).gameObject);
                buildController.transform.GetChild(index).GetComponent<PackableItem>().JustPack();
            }
            buildController.CallInitializeBuildPicePRC(index, value);
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
        else
        {
            Debug.Log("**This is not a building material**");
        }

    }

    public void CancelBuild(ThirdPersonUserControl user)
    {
        isBuilding = false;
        if (currentBuildObject.transform.GetChild(currentBuildObject.GetComponent<ObjectBuildController>().itemIndex).TryGetComponent<Item>(out var buildItem))
        {
            CraftingRecipe returnObjectInfo = CraftingManager.Instance.CancelBuildCraft(buildItem.itemIndex);
            ActorEquipment ae = user.GetComponent<ActorEquipment>();
            foreach (int index in returnObjectInfo.ingredients)
            {
                ae.AddItemToInventory(ItemManager.Instance.GetItemByIndex(index).GetComponent<Item>());
            }
        }
        PhotonNetwork.Destroy(currentBuildObject.GetPhotonView());
    }

}
