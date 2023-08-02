using UnityEngine;

/// <summary>
/// These are the different kinds of actors that could hold any item. This 
/// value is assigned to the item on equipped.
/// </summary>
public enum ItemOwner { Player, Enemy, Other, Null }//TODO - plug this in for each item
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
/// <summary>
/// The base class for all items in the game. Requires a Rigidbody and MeshCollider
/// </summary>
public class Item : MonoBehaviour
{
    /// <summary>
    /// The name of the item. Needs to be obsolete. Use the GameObject name.
    /// </summary>
    public string itemName = "default";
    public string itemDescription = " Default Description";
    [HideInInspector]
    public string id = "";
    public int value = 0;
    /// <summary>
    /// What kind of Actor is holding this item
    /// </summary>
    public ItemOwner itemOwner;
    /// <summary>
    /// The Game Object of the actor that own's the item
    /// </summary>
    public GameObject m_OwnerObject;
    /// <summary>
    /// Is this item currently equipped by an actor?
    /// </summary>
    public bool isEquipped;
    /// <summary>
    /// The icon to show in the inventory.
    /// </summary>
    public Sprite icon;
    /// <summary>
    /// Can this item be held?
    /// </summary>
    public bool isEquipable = true;
    /// <summary>
    /// Can this item be stored in the backpack?
    /// </summary>
    public bool fitsInBackpack = true;
    /// <summary>
    /// This determines what set of animations to use with this item. 1 one 
    /// handed item, 2 is an item carried with two hands, 3 one handed weapon, 
    /// 4 2handed weapon.
    /// </summary>
    public int itemAnimationState = 0;
    /// <summary>
    /// The index of inventory slot this item is currently taking up, -1 not being
    /// in the inventory. 
    /// </summary>
    public int inventoryIndex = -1;
    private Rigidbody m_Rigidbody;
    private MeshCollider m_Collider;
    /// <summary>
    /// This is the carrying actor's collider to prevent an actor from attacking
    /// themselves with the item.
    /// </summary>
    private Collider ignoredCollider;
    public bool hasLanded = true;
    public TerrainChunk parentChunk;
    public int itemIndex;
    public virtual void Awake()
    {
        m_Collider = GetComponent<MeshCollider>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider.convex = true;
    }

    void LateUpdate()
    {
        OutlineOnPlayerProximity();
    }

    public bool SaveItem(TerrainChunk chunk, bool isDestroyed)
    {
        int index = ItemManager.Instance.GetItemIndex(this.gameObject);

        LevelManager.Instance.UpdateSaveData(chunk, index, id, isDestroyed, transform.position, transform.rotation.eulerAngles, true);
        if (isDestroyed)
        {
            GameObject.Destroy(this.gameObject);
        }
        return true;
    }

    /// <summary>
    /// Checks for player distance and highlights the object if the player can 
    /// equipped the item. Note: Don't know if the player pickup distance is 
    /// matched here.
    /// </summary>
    void OutlineOnPlayerProximity()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerObjects)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (GetComponent<Outline>() != null)
            {
                if (distance <= 3 && !isEquipped && isEquipable)
                {
                    GetComponent<Outline>().enabled = true;
                }
                else
                {
                    GetComponent<Outline>().enabled = false;
                }
            }
        }
    }
    /// <summary>
    /// This is called to equipped the item. All derived classes should 
    /// implement the base class method as well. 
    /// </summary>
    /// <param name="character">The actor that is to equipped the item</param>
    public virtual void OnEquipped(GameObject character)
    {
        //Setting equip variables regarding the player
        if (character.CompareTag("Player"))
        {
            itemOwner = ItemOwner.Player;
        }
        else if (character.CompareTag("Enemy"))
        {
            itemOwner = ItemOwner.Enemy;
        }
        else
        {
            itemOwner = ItemOwner.Other;
        }
        //assigning the remaining necessary values
        m_OwnerObject = character;
        ignoredCollider = character.gameObject.GetComponent<Collider>();
        if (ignoredCollider != null && m_Collider != null)
        {
            Physics.IgnoreCollision(m_Collider, ignoredCollider);
            m_Collider.isTrigger = true;
            m_Rigidbody.isKinematic = true;
        }
        isEquipped = true;
    }
    /// <summary>
    /// The method used to unequipped an item. This will automatically be
    /// unequipped from the actor currently holding it.
    /// </summary>
    public virtual void OnUnequipped()
    {
        //Setting all of the item owner variables to null and false
        if (ignoredCollider != null && m_Collider != null)
        {
            Physics.IgnoreCollision(m_Collider, ignoredCollider, false);
            m_Collider.isTrigger = true;
            m_Rigidbody.isKinematic = true;
        }
        isEquipped = false;
        ignoredCollider = null;
        itemOwner = ItemOwner.Null;
        m_OwnerObject = null;
    }

    /// <summary>
    /// This is the virtual method that is to be used in conjunction with the
    /// input from the player or otherwise. These are empty because each derived
    /// class will have its own functionality however every derived class has to
    /// make this function call generic. This way, the items can be used in 
    /// the same way across the board.
    /// </summary>
    /// <param name="input">The input value of the controller. From 0 to 1.</param>
    public virtual void PrimaryAction(float input)
    {
        //Just an override
    }
    /// <summary>
    /// This is the virtual method that is to be used in conjunction with the
    /// input from the player or otherwise. Also this is the secondary action
    ///  These are empty because each derived class will have its own 
    /// functionality however every derived class has to make this function 
    /// call generic. This way, the items can be used in 
    /// the same way across the board.
    /// </summary>
    /// <param name="input">The input value of the controller. From 0 to 1.</param>
    public virtual void SecondaryAction(float input)
    {
        //Just an override
    }
}
