using UnityEngine;
using Photon.Pun;
using System;
using System.IO;
using Pathfinding;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ActorEquipment))]
public class EnemyManager : ActorManager
{
    [SerializeField] float m_turnSmooth = 5;
    [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f;
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    public float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.1f;
    public GameObject[] itemsToDrop;
    public Vector2Int[] lootRanges;

    private Rigidbody m_Rigidbody;
    private bool hasDiedAndDroppedLoot = false;
    private bool countDownDespawn = false;
    private int deathCounter = 0;

    // Pathfinding Components
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;

    public Animator m_Animator;

    public override void Awake()
    {
        base.Awake();
        if (!PhotonNetwork.IsMasterClient)
        {
            GetComponent<StateController>().enabled = false;
        }
    }

    public void Start()
    {
        // Gather references
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        equipment = GetComponent<ActorEquipment>();
    }

    public override void Update()
    {
        if (isDead && !hasDiedAndDroppedLoot)
        {
            // Stop the AI when dead
            aiPath.canMove = false;
            aiPath.destination = transform.position; // Ensure the destination is the current position

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<StateController>().currentState = null;
            GetComponent<StateController>().EnableAi(false);
            GetComponent<Collider>().enabled = false;
            DropLoot();
        }

        if (countDownDespawn)
        {
            if (deathCounter > 50)
            {
                // Instantiate death effect and destroy the enemy
                PhotonNetwork.Instantiate(System.IO.Path.Combine("PhotonPrefabs", "DeathEffect"), transform.position, transform.rotation);
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                deathCounter++;
            }
        }

        base.Update();
    }

    [PunRPC]
    public void UpdateAIPath(bool state)
    {
        // Enable or disable AI movement
        aiPath.canMove = state;
    }

    private void DropLoot()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            if (itemsToDrop != null && itemsToDrop.Length > 0)
            {
                for (int i = 0; i < itemsToDrop.Length; i++)
                {
                    Vector2Int range = lootRanges.Length > i && lootRanges[i] != null ? lootRanges[i] : new Vector2Int(0, 1);
                    int randomRange = UnityEngine.Random.Range(range.x, range.y);

                    for (int j = 0; j < randomRange; j++)
                    {
                        PlayerInventoryManager.Instance.DropItem(itemsToDrop[i].GetComponent<Item>().itemListIndex, transform.position + Vector3.up);
                    }
                }
            }

            if (equipment != null && equipment.equippedItem != null && UnityEngine.Random.Range(0, 1) < 0.01f)
            {
                // Drop equipped item (disabled for now)
                // PlayerInventoryManager.Instance.DropItem(equipment.equippedItem.GetComponent<Item>().itemIndex, transform.position + Vector3.up);
            }

            hasDiedAndDroppedLoot = true;
            countDownDespawn = true;
        }
    }
}
