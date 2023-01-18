using UnityEngine;

public enum ItemOwner { Player, Enemy, Other, Null }//TODO - plug this in for each item

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
public class Item : MonoBehaviour
{

    public ItemOwner itemOwner;
    //The player or npc holding the object
    public GameObject m_OwnerObject;
    public bool isEquiped;
    public Rigidbody rigidbodyRef;
    public MeshCollider col;
    public Collider ignoredCollider;
    public Sprite icon;
    public bool isEquipable = true;
    public bool fitsInBackpack = true;
    public string name = "default";
    public int inventoryIndex = -1;
    public int itemAnimationState = 0;
    //1 one handed item, 2 is an item carried with two hands, 3 one handed weapon, 4 2handed weapon
    public void Awake()
    {
        col = GetComponent<MeshCollider>();
        rigidbodyRef = GetComponent<Rigidbody>();
        col.convex = true;
        col.inflateMesh = true;
        col.skinWidth = 0.1f;
    }

    void LateUpdate()
    {
        Outline();
    }

    void Outline()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerObjects)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= 3 && !isEquiped)
            {
                GetComponent<Outline>().enabled = true;
            }
            else
            {
                GetComponent<Outline>().enabled = false;
            }
        }
    }
    public virtual void OnEquipt(GameObject character)
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
        m_OwnerObject = character;
        ignoredCollider = character.gameObject.GetComponent<Collider>();
        Physics.IgnoreCollision(col, ignoredCollider);
        rigidbodyRef.isKinematic = true;
        col.isTrigger = true;
        isEquiped = true;
    }

    public virtual void OnUnequipt()
    {
        //Setting all of the item owner variables to null and false
        Physics.IgnoreCollision(col, ignoredCollider, false);
        col.isTrigger = false;
        rigidbodyRef.isKinematic = false;
        isEquiped = false;
        ignoredCollider = null;
        itemOwner = ItemOwner.Null;
        m_OwnerObject = null;
    }


    // Pretty sure we can get rid of these. But there is a good chance a child 
    //class is implementing overrides. I removed the ones in Item
    public virtual void PrimaryAction(float input)
    {

    }

    public virtual void SecondaryAction(float input)
    {

    }
}
