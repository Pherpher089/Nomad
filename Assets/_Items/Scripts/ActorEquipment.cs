using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActorEquipment : MonoBehaviour
{
    //Item the player has equipped
    public GameObject equippedItem;
    //The armor the player has equipped
    public GameObject[] equippedArmor = new GameObject[3];
    public GameObject[] equippedSpecialItems = new GameObject[4];// a list of the game objects of the special items equipped. Pipe, utility, special and cape;
    [Description("Assign armor maps for what the character would be wearing if they had no armor.")]
    public HeadArmorCharacterIndexMap m_HeadArmorMap;
    public ChestArmorCharacterIndexMap m_ChestArmorMap;
    public LegsArmorCharacterIndexMap m_LegArmorMap;
    [HideInInspector] public bool hasItem;
    public Transform[] m_HandSockets = new Transform[4];
    [HideInInspector] public Transform[] m_ArmorSockets = new Transform[3];
    [HideInInspector] public Transform[] m_OtherSockets = new Transform[1]; // So far just for the earth bulwark but will probably also work with a shield 
    [HideInInspector] public Transform[] m_specialItemSockets = new Transform[4];
    [HideInInspector] public PlayerInventoryManager inventoryManager;
    [HideInInspector] public Animator m_Animator;//public for debug
    [HideInInspector] public TheseHands[] m_TheseHandsArray = new TheseHands[2];
    [HideInInspector] public TheseFeet[] m_TheseFeetArray = new TheseFeet[2];
    [HideInInspector] public Vector3 earthMinePath;
    private HeadArmorCharacterIndexMap m_DefaultHeadArmorMap;
    private ChestArmorCharacterIndexMap m_DefaultChestArmorMap;
    private LegsArmorCharacterIndexMap m_DefaultLegArmorMap;
    private List<Item> grabableItems = new List<Item>();
    private Item newItem;
    private CharacterManager characterManager;
    public bool isPlayer = false;
    private ItemManager m_ItemManager;
    private PhotonView pv;
    private ThirdPersonCharacter m_ThirdPersonCharacter;
    public CharacterStats m_Stats;
    private GameObject mine;
    private ActorAudioManager audioManager;

    //ArmorLists
    Transform m_HeadArmor1;
    Transform m_HeadArmor2;
    Transform m_HeadHair;
    Transform m_ChestArmor;
    Transform m_UpperRightArmArmor;
    Transform m_UpperLeftArmArmor;
    Transform m_LowerRightArmArmor;
    Transform m_LowerLeftArmArmor;
    Transform m_WaistArmor;
    Transform m_RightLegArmor;
    Transform m_LeftLegArmor;
    Transform m_CapeArmor;

    //Pipe List
    Transform m_Pipe;

    public void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("LoadingScene")) return;
        characterManager = GetComponent<CharacterManager>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        pv = GetComponent<PhotonView>();
        hasItem = false;
        m_Animator = GetComponentInChildren<Animator>();
        m_TheseHandsArray = GetComponentsInChildren<TheseHands>();
        m_TheseFeetArray = GetComponentsInChildren<TheseFeet>();
        m_HandSockets = new Transform[4];
        equippedArmor = new GameObject[3];
        equippedSpecialItems = new GameObject[4];
        audioManager = GetComponent<ActorAudioManager>();
        GetSockets(transform);
        if (tag == "Player")
        {
            m_Stats = GetComponent<CharacterStats>();
            GetArmorTransforms();
            isPlayer = true;
            m_DefaultHeadArmorMap = m_HeadArmorMap;
            m_DefaultChestArmorMap = m_ChestArmorMap;
            m_DefaultLegArmorMap = m_LegArmorMap;
        }
        else
        {
            if (equippedItem != null)
            {
                EquipItem(equippedItem);
            }
        }
    }

    void Start()
    {
        ToggleTheseHands(true);
    }

    void GetArmorTransforms()
    {
        m_HeadArmor1 = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2);
        m_HeadArmor2 = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(1);
        m_HeadHair = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1);
        m_ChestArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(3);
        m_UpperRightArmArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(4);
        m_UpperLeftArmArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(5);
        m_LowerRightArmArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(6);
        m_LowerLeftArmArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(7);
        m_WaistArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(10);
        m_RightLegArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(11);
        m_LeftLegArmor = transform.GetChild(0).GetChild(0).GetChild(2).GetChild(12);
        m_CapeArmor = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(4);
    }

    void GetSockets(Transform _transform)
    {
        foreach (Transform t in _transform.GetComponentInChildren<Transform>())
        {
            if (t.gameObject.tag == "HandSocket")
            {
                if (t.gameObject.name == "LeftHandSocket")
                {
                    m_HandSockets[0] = t;
                }
                else if (t.gameObject.name == "RightHandSocket")
                {
                    m_HandSockets[1] = t;
                }
                else if (t.gameObject.name == "ChestWeaponGearSocket")
                {
                    m_HandSockets[2] = t;
                }
                else
                {
                    m_HandSockets[3] = t;
                }
            }
            else if (t.gameObject.tag == "ArmorSocket")
            {
                switch (t.gameObject.name)
                {
                    case "HeadSocket":
                        m_ArmorSockets[0] = t;
                        break;
                    case "ChestSocket":
                        m_ArmorSockets[1] = t;
                        break;
                    case "LegsSocket":
                        m_ArmorSockets[2] = t;
                        break;
                }
            }
            else if (t.gameObject.tag == "OtherSocket")
            {
                m_OtherSockets[0] = t;
            }
            else if (t.gameObject.tag == "CapeSocket")
            {
                m_specialItemSockets[0] = t;
            }
            else if (t.gameObject.tag == "UtilitySocket")
            {
                m_specialItemSockets[1] = t;
            }
            else if (t.gameObject.tag == "PipeSocket")
            {
                m_specialItemSockets[2] = t;
            }
            else if (t.gameObject.tag == "SpecialSocket")
            {
                m_specialItemSockets[3] = t;
            }
            else
            {
                if (t.childCount > 0)
                {
                    GetSockets(t);
                }
            }


        }
    }

    void RemoveHeadArmorOnCharacter()
    {
        for (int i = 0; i < m_HeadArmor1.childCount; i++)
        {
            m_HeadArmor1.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_HeadArmor2.childCount; i++)
        {
            m_HeadArmor2.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_HeadHair.childCount; i++)
        {
            m_HeadHair.GetChild(i).gameObject.SetActive(false);
        }
    }
    void RemoveChestArmorOnCharacter()
    {
        for (int i = 0; i < m_ChestArmor.childCount; i++)
        {
            m_ChestArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_UpperRightArmArmor.childCount; i++)
        {
            m_UpperRightArmArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_UpperLeftArmArmor.childCount; i++)
        {
            m_UpperLeftArmArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_LowerRightArmArmor.childCount; i++)
        {
            m_LowerRightArmArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_LowerLeftArmArmor.childCount; i++)
        {
            m_LowerLeftArmArmor.GetChild(i).gameObject.SetActive(false);
        }
    }
    void RemoveLegArmorOnCharacter()
    {
        for (int i = 0; i < m_WaistArmor.childCount; i++)
        {
            m_WaistArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_RightLegArmor.childCount; i++)
        {
            m_RightLegArmor.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < m_LeftLegArmor.childCount; i++)
        {
            m_LeftLegArmor.GetChild(i).gameObject.SetActive(false);
        }
    }
    void RemoveCapeOnCharacter()
    {
        for (int i = 0; i < m_CapeArmor.childCount; i++)
        {
            m_CapeArmor.GetChild(i).gameObject.SetActive(false);
        }
    }

    void EquipHeadArmorOnCharacter(HeadArmorCharacterIndexMap headArmorMap)
    {
        RemoveHeadArmorOnCharacter();
        switch (headArmorMap.headListIndex)
        {
            case 0:
                m_HeadArmor1.GetChild(headArmorMap.headIndex).gameObject.SetActive(true);
                break;
            case 1:
                m_HeadArmor2.GetChild(headArmorMap.headIndex).gameObject.SetActive(true);
                break;
            case 2:
                m_HeadHair.GetChild(headArmorMap.headIndex).gameObject.SetActive(true);
                break;
        }
    }
    void EquipHeadArmorOnCharacter()
    {
        RemoveHeadArmorOnCharacter();
        switch (m_DefaultHeadArmorMap.headListIndex)
        {
            case 0:
                m_HeadArmor1.GetChild(m_DefaultHeadArmorMap.headIndex).gameObject.SetActive(true);
                break;
            case 1:
                m_HeadArmor2.GetChild(m_DefaultHeadArmorMap.headIndex).gameObject.SetActive(true);
                break;
            case 2:
                m_HeadHair.GetChild(m_DefaultHeadArmorMap.headIndex).gameObject.SetActive(true);
                break;
        }
    }

    void EquipChestArmorOnCharacter(ChestArmorCharacterIndexMap chestArmorMap)
    {
        RemoveChestArmorOnCharacter();
        m_ChestArmor.GetChild(chestArmorMap.chestIndex).gameObject.SetActive(true);
        m_UpperRightArmArmor.GetChild(chestArmorMap.upperRightArmIndex).gameObject.SetActive(true);
        m_UpperLeftArmArmor.GetChild(chestArmorMap.upperLeftArmIndex).gameObject.SetActive(true);
        m_LowerRightArmArmor.GetChild(chestArmorMap.lowerRightArmIndex).gameObject.SetActive(true);
        m_LowerLeftArmArmor.GetChild(chestArmorMap.lowerLeftArmIndex).gameObject.SetActive(true);
    }
    void EquipChestArmorOnCharacter()
    {
        RemoveChestArmorOnCharacter();
        m_ChestArmor.GetChild(m_DefaultChestArmorMap.chestIndex).gameObject.SetActive(true);
        m_UpperRightArmArmor.GetChild(m_DefaultChestArmorMap.upperRightArmIndex).gameObject.SetActive(true);
        m_UpperLeftArmArmor.GetChild(m_DefaultChestArmorMap.upperLeftArmIndex).gameObject.SetActive(true);
        m_LowerRightArmArmor.GetChild(m_DefaultChestArmorMap.lowerRightArmIndex).gameObject.SetActive(true);
        m_LowerLeftArmArmor.GetChild(m_DefaultChestArmorMap.lowerLeftArmIndex).gameObject.SetActive(true);
    }

    void EquipLegArmorOnCharacter(LegsArmorCharacterIndexMap legArmorMap)
    {
        RemoveLegArmorOnCharacter();
        m_WaistArmor.GetChild(legArmorMap.waistIndex).gameObject.SetActive(true);
        m_RightLegArmor.GetChild(legArmorMap.rightLegIndex).gameObject.SetActive(true);
        m_LeftLegArmor.GetChild(legArmorMap.leftLegIndex).gameObject.SetActive(true);
    }
    void EquipLegArmorOnCharacter()
    {
        RemoveLegArmorOnCharacter();
        m_WaistArmor.GetChild(m_DefaultLegArmorMap.waistIndex).gameObject.SetActive(true);
        m_RightLegArmor.GetChild(m_DefaultLegArmorMap.rightLegIndex).gameObject.SetActive(true);
        m_LeftLegArmor.GetChild(m_DefaultLegArmorMap.leftLegIndex).gameObject.SetActive(true);
    }
    void EquipCapeOnCharacter(int capeIndex)
    {
        RemoveCapeOnCharacter();
        m_CapeArmor.GetChild(capeIndex).gameObject.SetActive(true);
    }
    void EquipCapeOnCharacter()
    {
        RemoveCapeOnCharacter();
    }

    public bool AddItemToInventory(Item item)
    {
        bool wasAdded = false;
        if (item.fitsInBackpack)
        {
            wasAdded = inventoryManager.AddItem(ItemManager.Instance.GetItemGameObjectByItemIndex(item.itemListIndex).GetComponent<Item>(), 1);
        }
        if (isPlayer) characterManager.SaveCharacter();
        return wasAdded;
    }
    void ToggleTheseHands(bool toggle)
    {
        foreach (TheseHands th in m_TheseHandsArray)
        {
            th.gameObject.GetComponent<Collider>().enabled = toggle;
        }
    }

    public void EquipItem(GameObject item)
    {
        Item _item = item.GetComponent<Item>();
        int socketIndex;
        GameObject _newItem;
        if (_item.isEquipable)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    // Destroy(m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                    m_ArmorSockets[socketIndex] = null;
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_ArmorSockets[socketIndex].position, m_ArmorSockets[socketIndex].rotation, m_ArmorSockets[socketIndex]);
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;

                equippedArmor[socketIndex] = _newItem;
                if (armor.headMap != null)
                {
                    EquipHeadArmorOnCharacter(armor.headMap);
                }
                else if (armor.chestMap != null)
                {
                    EquipChestArmorOnCharacter(armor.chestMap);
                }
                else if (armor.legsMap != null)
                {
                    EquipLegArmorOnCharacter(armor.legsMap);
                }
            }
            else if (item.TryGetComponent<Pipe>(out var pipe))
            {
                socketIndex = 0;
                if (m_specialItemSockets[2].childCount > 0)
                {
                    Destroy(m_specialItemSockets[2].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_specialItemSockets[2].position, m_specialItemSockets[2].rotation, m_specialItemSockets[2]);
                equippedSpecialItems[2] = _newItem;
            }
            else if (item.TryGetComponent<Jewelry>(out var jewelry))
            {

                socketIndex = 0;
                if (m_specialItemSockets[3].childCount > 0)
                {
                    Destroy(m_specialItemSockets[3].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item));
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;
                equippedSpecialItems[3] = _newItem;
            }
            else if (item.TryGetComponent<Cape>(out var cape))
            {

                socketIndex = 0;
                if (m_specialItemSockets[0].childCount > 0)
                {
                    Destroy(m_specialItemSockets[0].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item));
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;
                equippedSpecialItems[0] = _newItem;
                EquipCapeOnCharacter(cape.m_CapeIndex);
            }
            else if (item.TryGetComponent<UtilityItem>(out var utilItem))
            {
                socketIndex = 0;
                if (m_specialItemSockets[1].childCount > 0)
                {
                    Destroy(m_specialItemSockets[1].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_specialItemSockets[1].position, m_specialItemSockets[1].rotation, m_specialItemSockets[1]);
                equippedSpecialItems[1] = _newItem;
            }
            else
            { // If item is not armor, which means, is held in the hands
                hasItem = true;
                switch (_item.itemAnimationState)
                {
                    case 2:
                    case 3:
                    case 5:
                        socketIndex = 1;
                        break;
                    case 1:
                    case 4:
                        socketIndex = 0;
                        break;
                    case 6:
                        socketIndex = 2;
                        break;
                    case 7:
                        socketIndex = 3;
                        break;
                    default:
                        socketIndex = 0;
                        break;

                }
                if (m_HandSockets[socketIndex].childCount > 0)
                {
                    Destroy(m_HandSockets[socketIndex].GetChild(0).gameObject);
                }
                _newItem = Instantiate(item, m_HandSockets[socketIndex].position, m_HandSockets[socketIndex].rotation, m_HandSockets[socketIndex]);
                equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                m_Animator.SetInteger("ItemAnimationState", _item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, _newItem.GetComponent<Item>().itemListIndex, socketIndex != 0, pv.ViewID);

            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }
        }
    }
    public void EquipItem(Item item)
    {
        int socketIndex;
        GameObject _newItem;
        if (item.isEquipable)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    Destroy(m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                    m_ArmorSockets[socketIndex] = null;
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_ArmorSockets[socketIndex].position, m_ArmorSockets[socketIndex].rotation, m_ArmorSockets[socketIndex]);
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;

                //_newItem = m_ItemManager.GetPrefabByItem(item);
                equippedArmor[socketIndex] = _newItem;
                if (armor.headMap != null)
                {
                    EquipHeadArmorOnCharacter(armor.headMap);
                }
                else if (armor.chestMap != null)
                {
                    EquipChestArmorOnCharacter(armor.chestMap);
                }
                else if (armor.legsMap != null)
                {
                    EquipLegArmorOnCharacter(armor.legsMap);
                }
            }
            else if (item.TryGetComponent<Pipe>(out var pipe))
            {
                socketIndex = 0;
                if (m_specialItemSockets[2].childCount > 0)
                {
                    Destroy(m_specialItemSockets[2].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_specialItemSockets[2].position, m_specialItemSockets[2].rotation, m_specialItemSockets[2]);
                equippedSpecialItems[2] = _newItem;
            }
            else if (item.TryGetComponent<Jewelry>(out var jewelry))
            {
                socketIndex = 0;
                if (m_specialItemSockets[3].childCount > 0)
                {
                    Destroy(m_specialItemSockets[3].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item));
                _newItem.SetActive(false);
                equippedSpecialItems[3] = _newItem;
            }
            else if (item.TryGetComponent<Cape>(out var cape))
            {

                socketIndex = 0;
                if (m_specialItemSockets[0].childCount > 0)
                {
                    Destroy(m_specialItemSockets[0].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item));
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;
                equippedSpecialItems[0] = _newItem;
                EquipCapeOnCharacter(cape.m_CapeIndex);

            }
            else if (item.TryGetComponent<UtilityItem>(out var utilItem))
            {
                socketIndex = 0;
                if (m_specialItemSockets[1].childCount > 0)
                {
                    Destroy(m_specialItemSockets[1].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_specialItemSockets[1].position, m_specialItemSockets[1].rotation, m_specialItemSockets[1]);
                equippedSpecialItems[1] = _newItem;
            }
            else
            { // If item is not armor, which means, is held in the hands
                hasItem = true;
                socketIndex = item.itemAnimationState == 1 || item.itemAnimationState == 4 ? 0 : item.itemAnimationState == 6 ? 2 : 1;

                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_HandSockets[socketIndex].position, m_HandSockets[socketIndex].rotation, m_HandSockets[socketIndex]);
                equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                m_Animator.SetInteger("ItemAnimationState", item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, _newItem.GetComponent<Item>().itemListIndex, socketIndex != 0, pv.ViewID);
            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }
        }
    }

    [PunRPC]
    public void EquipItemClient(int itemIndex, bool offHand, int viewId)
    {
        ActorEquipment targetView = PhotonView.Find(viewId).GetComponent<ActorEquipment>();
        // Fetch the item from the manager using the ID
        GameObject item = targetView.m_ItemManager.GetItemGameObjectByItemIndex(itemIndex);
        int socketIndex;
        Item _item = item.GetComponent<Item>();
        GameObject _newItem;
        // Make sure the item is equipable
        if (item != null && _item.isEquipable == true)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (targetView.m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    // Destroy(targetView.m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                    targetView.m_ArmorSockets[socketIndex] = null;
                }
                _newItem = Instantiate(targetView.m_ItemManager.GetPrefabByItem(_item), targetView.m_ArmorSockets[socketIndex].position, targetView.m_ArmorSockets[socketIndex].rotation, targetView.m_ArmorSockets[socketIndex]);
                _newItem.GetComponent<MeshRenderer>().enabled = false;
                _newItem.GetComponent<Collider>().enabled = false;

                targetView.equippedArmor[socketIndex] = _newItem;
                if (armor.headMap != null)
                {
                    EquipHeadArmorOnCharacter(armor.headMap);
                }
                else if (armor.chestMap != null)
                {
                    EquipChestArmorOnCharacter(armor.chestMap);
                }
                else if (armor.legsMap != null)
                {
                    EquipLegArmorOnCharacter(armor.legsMap);
                }
            }
            else if (item.TryGetComponent<Pipe>(out var pipe))
            {
                if (m_specialItemSockets[3].childCount > 0)
                {
                    Destroy(m_specialItemSockets[3].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_specialItemSockets[3].position, m_specialItemSockets[3].rotation, m_specialItemSockets[3]);
            }
            else if (item.TryGetComponent<Jewelry>(out var jewelry))
            {
                return;
            }
            else if (item.TryGetComponent<Cape>(out var cape))
            {
                EquipCapeOnCharacter(cape.m_CapeIndex);
                return;
            }
            else if (item.TryGetComponent<UtilityItem>(out var utilItem))
            {
                if (m_specialItemSockets[1].childCount > 0)
                {
                    Destroy(m_specialItemSockets[1].GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_specialItemSockets[1].position, m_specialItemSockets[1].rotation, m_specialItemSockets[1]);
            }
            else
            { // If item is not armor, which means, is held in the hands
                targetView.hasItem = true;
                socketIndex = _item.itemAnimationState == 1 || _item.itemAnimationState == 4 ? 0 : _item.itemAnimationState == 6 ? 2 : 1;
                _newItem = Instantiate(targetView.m_ItemManager.GetPrefabByItem(_item), targetView.m_HandSockets[socketIndex].position, targetView.m_HandSockets[socketIndex].rotation, targetView.m_HandSockets[socketIndex]);
                targetView.equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                targetView.m_Animator.SetInteger("ItemAnimationState", _item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
        }
    }
    public float GetArmorBonus()
    {
        float bonus = 0;
        for (int i = 0; i < 3; i++)
        {
            if (equippedArmor[i] != null)
            {
                bonus += equippedArmor[i].GetComponent<Armor>().m_DefenseValue;
            }
        }
        if (equippedSpecialItems[3] != null)
        {
            bonus += equippedSpecialItems[3].GetComponent<Jewelry>().m_DefenseValue;
        }
        if (equippedSpecialItems[0] != null)
        {
            bonus += equippedSpecialItems[0].GetComponent<Cape>().m_DefenseValue;
        }
        return bonus;
    }
    public EquipmentStatBonus GetStatBonus()
    {
        int _dexBonus = 0;
        int _strBonus = 0;
        int _intBonus = 0;
        int _conBonus = 0;
        for (int i = 0; i < 3; i++)
        {
            if (equippedArmor[i] != null)
            {
                Armor armor = equippedArmor[i].GetComponent<Armor>();
                _dexBonus += armor.dexBonus;
                _strBonus += armor.strBonus;
                _conBonus += armor.conBonus;
                _intBonus += armor.intBonus;
            }
        }
        if (equippedItem != null && equippedItem.TryGetComponent<ToolItem>(out var tool))
        {
            _dexBonus += tool.dexBonus;
            _strBonus += tool.strBonus;
            _conBonus += tool.conBonus;
            _intBonus += tool.intBonus;
        }
        if (equippedSpecialItems[3] != null && equippedSpecialItems[3].TryGetComponent<Jewelry>(out var jewelry))
        {
            _dexBonus += jewelry.dexBonus;
            _strBonus += jewelry.strBonus;
            _conBonus += jewelry.conBonus;
            _intBonus += jewelry.intBonus;
        }
        if (equippedSpecialItems[0] != null && equippedSpecialItems[0].TryGetComponent<Cape>(out var cape))
        {
            _dexBonus += cape.dexBonus;
            _strBonus += cape.strBonus;
            _conBonus += cape.conBonus;
            _intBonus += cape.intBonus;
        }
        return new EquipmentStatBonus(_dexBonus, _strBonus, _intBonus, _conBonus);
    }
    public void UnequippedCurrentSpecialItem(int specialItemIndex)
    {
        Item item = equippedSpecialItems[specialItemIndex].GetComponent<Item>();
        item.inventoryIndex = -1;
        item.OnUnequipped();
        equippedSpecialItems[specialItemIndex].transform.parent = null;
        equippedSpecialItems[specialItemIndex].SetActive(false);
        equippedSpecialItems[specialItemIndex] = null;
        if (specialItemIndex == 0)
        {
            RemoveCapeOnCharacter();
        }

        pv.RPC("UnequippedCurrentSpecialItemClient", RpcTarget.OthersBuffered, specialItemIndex);
        if (isPlayer && characterManager.isLoaded)
        {
            m_Stats.GenerateStats();
            characterManager.SaveCharacter();//TODO have to do this too
        }
    }
    public bool UnequippedCurrentSpecialItemToInventory(int specialItemIndex)
    {
        if (equippedSpecialItems[specialItemIndex] != null)
        {
            if (!equippedSpecialItems[specialItemIndex].GetComponent<Item>().isBeltItem)
            {
                bool canUnequipped = AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(equippedSpecialItems[specialItemIndex].GetComponent<Item>().itemListIndex).GetComponent<Item>());
                if (!canUnequipped) return false;
            }
            equippedSpecialItems[specialItemIndex].GetComponent<Item>().OnUnequipped();
            equippedSpecialItems[specialItemIndex].transform.parent = null;
            equippedSpecialItems[specialItemIndex].SetActive(false);
            equippedSpecialItems[specialItemIndex] = null;
            if (specialItemIndex == 0)
            {
                RemoveCapeOnCharacter();
            }
            pv.RPC("UnequippedCurrentSpecialItemClient", RpcTarget.AllBuffered, specialItemIndex);
            //If this is not an npc, save the character
            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }
        }
        return true;
    }
    [PunRPC]
    public void UnequippedCurrentSpecialItemClient(int specialItemIndex)
    {
        if (pv.IsMine) return;
        if (m_specialItemSockets[specialItemIndex] != null)
        {
            if (m_specialItemSockets[specialItemIndex].GetChild(0) != null)
            {
                Destroy(m_specialItemSockets[specialItemIndex].GetChild(0).gameObject);
                m_specialItemSockets[specialItemIndex] = null;
            }
            if (specialItemIndex == 0)
            {
                RemoveCapeOnCharacter();
            }
        }
    }
    public void UnequippedCurrentArmor(ArmorType armorType)
    {
        Item item = equippedArmor[(int)armorType].GetComponent<Item>();
        item.inventoryIndex = -1;
        item.OnUnequipped();
        equippedArmor[(int)armorType].transform.parent = null;
        equippedArmor[(int)armorType].SetActive(false);
        equippedArmor[(int)armorType] = null;

        if (armorType == ArmorType.Helmet)
        {

            EquipHeadArmorOnCharacter();
        }
        else if (armorType == ArmorType.Chest)
        {

            EquipChestArmorOnCharacter();
        }
        else if (armorType == ArmorType.Legs)
        {

            EquipLegArmorOnCharacter();
        }
        pv.RPC("UnequippedCurrentArmorClient", RpcTarget.OthersBuffered, armorType);
        if (isPlayer && characterManager.isLoaded)
        {
            m_Stats.GenerateStats();
            characterManager.SaveCharacter();
        }
    }
    public bool UnequippedCurrentArmorToInventory(ArmorType armorType)
    {
        if (equippedArmor[(int)armorType] != null)
        {
            if (!equippedArmor[(int)armorType].GetComponent<Item>().isBeltItem)
            {
                bool canUnequipped = AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(equippedArmor[(int)armorType].GetComponent<Item>().itemListIndex).GetComponent<Item>());
                if (!canUnequipped) return false;
            }
            equippedArmor[(int)armorType].GetComponent<Item>().OnUnequipped();
            equippedArmor[(int)armorType].transform.parent = null;
            equippedArmor[(int)armorType].SetActive(false);
            equippedArmor[(int)armorType] = null;

            if (armorType == ArmorType.Helmet)
            {
                EquipHeadArmorOnCharacter();
            }
            else if (armorType == ArmorType.Chest)
            {
                EquipChestArmorOnCharacter();
            }
            else if (armorType == ArmorType.Legs)
            {
                EquipLegArmorOnCharacter();
            }

            pv.RPC("UnequippedCurrentArmorClient", RpcTarget.AllBuffered, armorType);
            //If this is not an npc, save the character
            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }
        }
        return true;
    }
    [PunRPC]
    public void UnequippedCurrentArmorClient(ArmorType armorType)
    {
        if (pv.IsMine) return;
        if (equippedArmor[(int)armorType] != null)
        {
            equippedArmor[(int)armorType].GetComponent<Item>().OnUnequipped();
            equippedArmor[(int)armorType].transform.parent = null;
            equippedArmor[(int)armorType].SetActive(false);
            equippedArmor[(int)armorType] = null;
            if (armorType == ArmorType.Helmet)
            {
                EquipHeadArmorOnCharacter();
            }
            else if (armorType == ArmorType.Chest)
            {
                EquipChestArmorOnCharacter();
            }
            else if (armorType == ArmorType.Legs)
            {
                EquipLegArmorOnCharacter();
            }
        }
    }
    public void UnequippedCurrentItem()
    {
        if (equippedItem != null)
        {
            hasItem = false;
            Item item = equippedItem.GetComponent<Item>();
            item.OnUnequipped();
            item.inventoryIndex = -1;
            equippedItem.transform.parent = null;
            equippedItem.SetActive(false);
            equippedItem.SetActive(false);
            equippedItem = null;

            m_Animator.SetInteger("ItemAnimationState", 0);
            ToggleTheseHands(true);
            pv.RPC("UnequippedCurrentItemClient", RpcTarget.OthersBuffered);

            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }
        }

    }
    public void UnequippedCurrentItem(bool spendItem)
    {
        hasItem = false;
        GameObject itemToDestroy = equippedItem;
        equippedItem.GetComponent<Item>().OnUnequipped();
        if (m_HandSockets[0].childCount > 0)
        {
            foreach (Transform child in m_HandSockets[0])
            {
                Destroy(child.gameObject);
            }
        }
        if (m_HandSockets[1].childCount > 0)
        {
            foreach (Transform child in m_HandSockets[1])
            {
                Destroy(child.gameObject);
            }
        }
        equippedItem = null;
        m_Animator.SetInteger("ItemAnimationState", 0);
        ToggleTheseHands(true);
        pv.RPC("UnequippedCurrentItemClient", RpcTarget.OthersBuffered);
        if (isPlayer && characterManager.isLoaded)
        {
            m_Stats.GenerateStats();
            characterManager.SaveCharacter();
        }
    }

    [PunRPC]
    public void UnequippedCurrentItemClient()
    {
        if (pv.IsMine) return;
        hasItem = false;
        if (equippedItem != null)
        {
            equippedItem?.GetComponent<Item>()?.OnUnequipped();
            equippedItem.transform.parent = null;
            Destroy(equippedItem.gameObject);
            equippedItem = null;
        }
    }
    public bool UnequippedCurrentItemToInventory()
    {
        if (equippedItem != null && equippedItem.GetComponent<Item>().fitsInBackpack)
        {
            if (!equippedItem.GetComponent<Item>().isBeltItem)
            {
                bool canReturnToInventory = AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(equippedItem.GetComponent<Item>().itemListIndex).GetComponent<Item>());

                if (!canReturnToInventory)
                {
                    return false;
                }
            }

            m_Animator.SetInteger("ItemAnimationState", 0);

            ToggleTheseHands(true);

            equippedItem.gameObject.SetActive(false);
            equippedItem = null;
            hasItem = false;
            pv.RPC("UnequippedCurrentItemClient", RpcTarget.AllBuffered);
            //If this is not an npc, save the character
            if (isPlayer && characterManager.isLoaded)
            {
                m_Stats.GenerateStats();
                characterManager.SaveCharacter();
            }

        }
        else
        {
            //If this item is not able to fit in the back pack, unequip
            UnequippedCurrentItem();
        }
        return true;

    }
    public void SpendItem()
    {

        if (equippedItem == null) return;
        Item item = equippedItem.GetComponent<Item>();
        if (item == null) return;
        bool spent = false;
        foreach (ItemStack itemStack in inventoryManager.items)
        {
            if (itemStack.item != null && itemStack.item.itemListIndex == equippedItem.GetComponent<Item>().itemListIndex)
            {
                inventoryManager.RemoveItem(itemStack.item.inventoryIndex, 1);
                spent = true;
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
        for (int i = 0; i < inventoryManager.beltItems.Length; i++)
        {
            ItemStack itemStack = inventoryManager.beltItems[i];
            if (itemStack.item != null && itemStack.item.itemListIndex == equippedItem.GetComponent<Item>().itemListIndex)
            {
                inventoryManager.RemoveBeltItem(i, 1);
                spent = true;
                if (isPlayer) characterManager.SaveCharacter();
            }
        }



        if (!spent)
        {
            UnequippedCurrentItem(true);
        }
    }

    public void SpendItem(Item item)
    {
        foreach (ItemStack stack in inventoryManager.items)
        {
            if (stack.item != null && stack.item.name == item.name)
            {
                if (item.inventoryIndex >= 0 && inventoryManager.items[item.inventoryIndex].count > 0)
                {
                    inventoryManager.RemoveItem(item.inventoryIndex, 1);
                    if (isPlayer) characterManager.SaveCharacter();
                    break;
                }
                else
                {
                    UnequippedCurrentItem(true);
                    break;
                }
            }

        }

    }

    // Finds all not equiped items in the screen that are close enough to the player to grab and adds them to the grabableItems list. This function also returns the closest
    Item GatherAllItemsInScene()
    {
        Item[] allItems = GameObject.FindObjectsOfType<Item>();
        Item closestItem = null;
        float closestDist = 7;
        foreach (Item item in allItems)
        {

            if (!item.isEquipped && item.isEquipable)
            {
                float currentItemDist = Vector3.Distance(transform.position + Vector3.up, item.gameObject.transform.position);
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
            return closestItem;
    }

    public void ReadyEarthMine()
    {
        if (mine == null)
        {
            if (!CheckForMana()) return;
            GameObject earthMine = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "EarthMine"), m_OtherSockets[0].position, Quaternion.identity);
            earthMine.GetComponent<EarthMineController>().SetSudoParent(m_OtherSockets[0].transform);
            mine = earthMine;
        }
        // earthMine.transform.parent = m_OtherSockets[0];
    }

    public void ShootBow()
    {
        Vector3 direction = transform.forward;
        if (tag == "Enemy")
        {
            direction = GetComponent<StateController>().target.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
            direction = direction.normalized;
        }
        else
        {
            bool hasArrows = false;
            for (int i = 0; i < inventoryManager.items.Length; i++)
            {
                if (inventoryManager.items[i].item && inventoryManager.items[i].item.itemListIndex == 14 && inventoryManager.items[i].count > 0)
                {
                    hasArrows = true;
                    inventoryManager.RemoveItem(i, 1);
                    break;
                }
            }
            if (!hasArrows)
            {
                for (int i = 0; i < inventoryManager.beltItems.Length; i++)
                {
                    if (inventoryManager.beltItems[i].item && inventoryManager.beltItems[i].item.itemListIndex == 14 && inventoryManager.beltItems[i].count > 0)
                    {
                        hasArrows = true;
                        inventoryManager.RemoveBeltItem(i, 1);
                        break;
                    }
                }
            }

            if (!hasArrows) return;
        }
        GameObject arrow = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Arrow"), transform.position + (transform.forward * 1.5f) + (transform.up * 2f), Quaternion.LookRotation(direction));
        arrow.GetComponent<ArrowControl>().Initialize(gameObject, equippedItem);
        arrow.GetComponent<Rigidbody>().velocity = direction * 80;
        arrow.GetComponent<Rigidbody>().useGravity = true;
    }

    public void CastWand()
    {
        GameObject MagicObject;
        if (equippedItem.GetComponent<Item>().itemListIndex == 55)
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "MagicMissle"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = (transform.forward * 25);
            return;
        }
        if (!CheckForMana()) return;
        if (equippedItem.GetComponent<Item>().itemListIndex == 49)
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "IceShardsParent"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            int childCount = MagicObject.transform.childCount;
            Transform[] magicChild = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                magicChild[i] = MagicObject.transform.GetChild(i);
            }
            for (int i = 0; i < childCount; i++)
            {
                magicChild[i].parent = null;
                magicChild[i].GetComponent<FireBallControl>().Initialize(gameObject, equippedItem, false);
                magicChild[i].GetComponent<Rigidbody>().velocity = magicChild[i].up * 10;
            }
        }
        else if (equippedItem.GetComponent<Item>().itemListIndex == 50)
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "RockWave"), transform.position + (transform.forward * 1.5f) + (transform.up), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<RockWallParticleController>().Initialize(this.gameObject);
            MagicObject.GetComponentInChildren<RockWallParticleController>().Initialize(this.gameObject);
        }
        else
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = (transform.forward * 20);
        }
    }

    private bool CheckForMana()
    {
        bool hasMana = false;
        for (int i = 0; i < inventoryManager.items.Length; i++)
        {
            if (inventoryManager.items[i].item && inventoryManager.items[i].item.itemListIndex == 26 && inventoryManager.items[i].count > 0)
            {
                hasMana = true;
                inventoryManager.RemoveItem(i, 1);
                break;
            }
        }

        if (!hasMana)
        {
            for (int i = 0; i < inventoryManager.beltItems.Length; i++)
            {
                if (inventoryManager.beltItems[i].item && inventoryManager.beltItems[i].item.itemListIndex == 26 && inventoryManager.beltItems[i].count > 0)
                {
                    hasMana = true;
                    inventoryManager.RemoveBeltItem(i, 1);
                    break;
                }
            }
        }

        if (!hasMana) return false;
        else return true;
    }

    public void CastWandArc()
    {
        if (equippedItem.GetComponent<Item>().itemListIndex == 55)
        {
            GameObject MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "MagicMissleBig"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = (transform.forward * 10);
            return;
        }

        if (!CheckForMana()) return;
        if (equippedItem.GetComponent<Item>().itemListIndex == 49)
        {
            GameObject glacialHeal = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GlacialHeal"), transform.position + (transform.up * 2.5f), Quaternion.LookRotation(transform.forward));
            glacialHeal.GetComponent<AoeHeal>().Initialize(gameObject);

        }
        else if (equippedItem.GetComponent<Item>().itemListIndex == 50)
        {
            GameObject earthMine = mine;
            earthMine.transform.rotation = Quaternion.Euler(0, 0, 0);
            earthMine.GetComponent<EarthMineController>().Initialize(earthMinePath, this.gameObject, equippedItem);
            mine = null;

        }
        else
        {
            GameObject fireBall = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            fireBall.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem, true);
            fireBall.GetComponent<Rigidbody>().velocity = (transform.forward * 7) + (transform.up * 15);
            fireBall.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void GrabItem()
    {
        newItem = GatherAllItemsInScene();

        if (newItem == null || newItem.itemName == "Fire Pit" || !newItem.hasLanded)
        {
            return;
        }
        if (TryGetComponent(out BeastGear beastGear))
        {
            return;
        }
        if (newItem.TryGetComponent(out BuildingObject bo) && bo.isPlaced)
        {
            return;
        }

        if (hasItem || newItem.gameObject.tag != "Tool" && newItem.gameObject.tag != "Food")
        {

            if (newItem != null)
            {
                if (!newItem.isEquipable)
                {

                    return;
                };
                if (newItem.fitsInBackpack)
                {
                    bool wasAdded = AddItemToInventory(newItem);
                    if (!wasAdded)
                    {
                        LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                        PlayerInventoryManager.Instance.DropItem(newItem.itemListIndex, newItem.transform.position);
                        return;
                    };
                    audioManager.PlayGrabItem();
                }
                else
                {
                    if (hasItem)
                    {
                        UnequippedCurrentItem();
                    }
                    audioManager.PlayGrabItem();
                    EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                }
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                //newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
        else
        {
            if (newItem != null)
            {
                newItem.inventoryIndex = -1;
                audioManager.PlayGrabItem();
                EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                //newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
    }
    public void GrabItem(Item item)
    {
        newItem = item;

        if (hasItem)
        {
            if (newItem != null)
            {
                if (!newItem.isEquipable) return;
                if (newItem.fitsInBackpack && inventoryManager)
                {
                    bool wasAdded = AddItemToInventory(m_ItemManager.GetPrefabByItem(newItem).GetComponent<Item>());
                    if (!wasAdded)
                    {
                        LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                        PlayerInventoryManager.Instance.DropItem(newItem.itemListIndex, newItem.transform.position);
                        return;
                    };
                    audioManager.PlayGrabItem();
                }
                else
                {
                    if (hasItem)
                    {
                        UnequippedCurrentItem();
                    }
                    EquipItem(newItem);
                }
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
            }
        }
        else
        {
            if (newItem != null)
            {
                newItem.inventoryIndex = -1;
                audioManager.PlayGrabItem();
                audioManager.PlayGrabItem();
                EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
            }
        }
        if (isPlayer) characterManager.SaveCharacter();
    }

}
