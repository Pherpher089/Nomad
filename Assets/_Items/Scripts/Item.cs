using System.Xml;
using UnityEngine;


public enum ItemOwner { Player, Enemy, Other, Null }//TODO - plug this in for each item
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]

public class Item : MonoBehaviour
{
    [Tooltip("The name that is displayed in the inventory.")]
    public string itemName = "default";
    [Tooltip("The description that is displayed in the inventory.")]
    public string itemDescription = " Default Description";
    [Tooltip("The items value in gold. For future.")]
    public int value = 0;
    [Tooltip("The icon that shows in the inventory and crafting UIs.")]
    public Sprite icon;
    [Tooltip("Can a player pick this up and hold it?")]
    public bool isEquipable = true;
    [Tooltip("Can a player store this in their backpack?")]
    public bool fitsInBackpack = true;
    [Tooltip("This indicates to the animator, how to hold this item. 0 = Unarmed, 1 = Two Handed weapon, 2 = One handed weapon, 3 = Small item like an apple, 4 = Bow, 5 = Wand.")]
    public int itemAnimationState = 0;
    [Tooltip("The index which this item exists on the Game Controller's Item Manager's Item List.")]
    public int itemListIndex;
    private Rigidbody m_Rigidbody;
    private MeshCollider m_Collider;
    private Collider ignoredCollider;
    [HideInInspector][SerializeField] public string id = "";
    [HideInInspector] public int inventoryIndex = -1;
    [HideInInspector] public bool hasLanded = true;
    [HideInInspector] public ItemOwner itemOwner;
    [HideInInspector] public GameObject m_OwnerObject;
    [HideInInspector] public bool isEquipped = false;
    [HideInInspector] public string spawnId;
    [HideInInspector] public bool isBeltItem = false;
    public override bool Equals(object obj)
    {
        // If the passed object is null or not an Item instance, they're not equal
        if (obj == null || GetType() != obj.GetType())
            return false;

        // If they are the same instance, they are equal
        if (ReferenceEquals(this, obj))
            return true;

        // Now we compare any identifying properties that would make your Item instances unique
        Item other = (Item)obj;
        return itemName == other.itemName; // Assuming itemName is your unique identifier
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            // Cast the unsigned literal to an int explicitly within the unchecked context
            int hash = (int)2166136261;
            const int hashingMultiplier = 16777619;

            // Compute the hash code using the properties that determine object equality
            hash = (hash * hashingMultiplier) ^ (itemName?.GetHashCode() ?? 0);
            // If you have other properties, you would continue building the hash code like this:
            // hash = (hash * hashingMultiplier) ^ (OtherProperty?.GetHashCode() ?? 0);

            return hash;
        }
    }
    public virtual void Awake()
    {
        m_Collider = GetComponent<MeshCollider>();
        m_Rigidbody = GetComponent<Rigidbody>();
        if (!itemName.Contains("Spell Circle"))
        {
            m_Collider.convex = true;
        }
    }

    void LateUpdate()
    {
        OutlineOnPlayerProximity();
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
        ignoredCollider = character.GetComponent<Collider>();
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
