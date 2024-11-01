using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.AI;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ActorEquipment))]
public class EnemyManager : ActorManager
{
    [SerializeField] float m_turnSmooth = 5;
    [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    public float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.1f;
    public GameObject[] itemsToDrop;
    public Vector2Int[] lootRanges;
    Rigidbody m_Rigidbody;
    //Animator m_Animator;
    [HideInInspector] public bool m_IsGrounded;
    public Animator m_Animator;
    //EquipmentVariables
    //NavMeshAgent m_NavMeshAgent;
    NavMeshAgent navMeshAgent;
    bool hasDiedAndDroppedLoot = false;
    bool countDownDespawn = false;
    int deathCounter = 0;
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
        //Gather references from the rest of the game object
        navMeshAgent = GetComponent<NavMeshAgent>();
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        equipment = GetComponent<ActorEquipment>();
    }


    public override void Update()
    {
        if (isDead && !hasDiedAndDroppedLoot)
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.destination = transform.position;
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<StateController>().currentState = null;
            GetComponent<StateController>().aiActive = false;
            GetComponent<Collider>().enabled = false;
            DropLoot();
        }
        if (countDownDespawn)
        {
            if (deathCounter > 50)
            {
                //death
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DeathEffect"), transform.position, transform.rotation);
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
    public void UpdateNavMeshAgent(bool state)
    {
        //todo
    }

    private void DropLoot()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            if (itemsToDrop != null && itemsToDrop.Length > 0)
            {
                for (int i = 0; i < itemsToDrop.Length; i++)
                {
                    Vector2Int range = new Vector2Int(0, 1);
                    if (lootRanges.Length > i && lootRanges[i] != null)
                    {
                        range = lootRanges[i];
                    }
                    int randomRange = UnityEngine.Random.Range(range.x, range.y);
                    for (int j = 0; j < randomRange; j++)
                    {
                        PlayerInventoryManager.Instance.DropItem(itemsToDrop[i].GetComponent<Item>().itemListIndex, transform.position + Vector3.up);
                    }
                }
            }

            if (equipment != null && equipment.equippedItem != null && UnityEngine.Random.Range(0, 1) < 0.01f)
            {
                //PlayerInventoryManager.Instance.DropItem(equipment.equippedItem.GetComponent<Item>().itemIndex, transform.position + Vector3.up);
            }
            hasDiedAndDroppedLoot = true;
            countDownDespawn = true;
        }
    }
}
