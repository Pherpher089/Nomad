using UnityEngine;
using Photon.Pun;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(HealthManager))]
public class BeastManager : MonoBehaviour
{
    public static BeastManager Instance;
    Rigidbody m_Rigidbody;
    StateController m_StateController;
    GameObject camObj;
    Animator m_Animator;
    PhotonView m_PhotonView;
    HealthManager m_HealthManager;
    public BeastStableController m_BeastStableController;
    InteractionManager m_InteractionManager;
    CapsuleCollider m_Collider;
    Vector3 m_OriginalColliderCenter;
    PhotonView pv;

    // State Variables
    public bool m_IsCamping = false;
    public bool m_IsInStable = false;

    //Riding variables
    public RideBeastInteraction rideBeast;
    public Dictionary<int, int> riders;
    public bool hasDriver = false;

    //Gear variables
    public string m_SaveFilePath;
    public int[][] m_GearIndices;
    public GameObject[][] m_Sockets;
    public BeastStorageContainerController[] m_BeastChests;
    public List<int> m_BlockedGearSlots = new List<int>();

    // Stamina variables
    Image staminaBarImage;
    public float m_Stamina;
    public float m_MaxStamina;

    // Ramming variables
    [HideInInspector] public bool m_isRamming = false;
    public GameObject m_RamTarget;

    // Beast movement variables
    public float ridingSpeed = 25;
    public float turnSpeed = 4f;
    public float m_RamSpeed = 30;
    private float m_CurrentSpeed = 0;
    public float acceleration = 1;
    public float turnAcceleration = 10;
    private float m_CurrentTurnSpeed = 0;
    private float lastYRotation;
    private Vector3 lastPosition;

    // Vacuum variables
    public List<Item> objectsInVauumRange = new List<Item>();

    void Awake()
    {
        m_Collider = GetComponent<CapsuleCollider>();
        m_OriginalColliderCenter = m_Collider.center;
        Instance = this;
        riders = new Dictionary<int, int>();
        m_Animator = GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        m_HealthManager = GetComponent<HealthManager>();
        m_Sockets = new GameObject[7][];
        m_Sockets[0] = new GameObject[2];
        m_Sockets[1] = new GameObject[4];
        m_Sockets[2] = new GameObject[1];
        m_Sockets[3] = new GameObject[4];
        m_Sockets[4] = new GameObject[1];
        m_Sockets[5] = new GameObject[1];
        m_Sockets[6] = new GameObject[1];

        GameObject[] _sockets = GameObject.FindGameObjectsWithTag("BeastGearSocket");
        m_BeastChests = new BeastStorageContainerController[3];
        //This ensures the sockets are in the expected order 
        //Some gear requires multiple gear slots which is why m_Sockets is a 2D array
        foreach (GameObject socket in _sockets)
        {
            if (socket.gameObject.name.Contains("Antler"))
            {
                if (socket.gameObject.name.Contains("1"))
                {
                    m_Sockets[0][0] = socket;
                }
                if (socket.gameObject.name.Contains("2"))
                {
                    m_Sockets[0][1] = socket;
                }
            }
            if (socket.gameObject.name.Contains("Back"))
            {
                if (socket.gameObject.name.Contains("1"))
                {
                    m_Sockets[1][0] = socket;
                }
                if (socket.gameObject.name.Contains("2"))
                {
                    m_Sockets[1][1] = socket;
                }
                if (socket.gameObject.name.Contains("3"))
                {
                    m_Sockets[1][2] = socket;
                }
                if (socket.gameObject.name.Contains("4"))
                {
                    m_Sockets[1][3] = socket;
                }
            }
            if (socket.gameObject.name.Contains("Head"))
            {
                m_Sockets[2][0] = socket;
            }
            if (socket.gameObject.name.Contains("Hoof"))
            {
                if (socket.gameObject.name.Contains("1"))
                {
                    m_Sockets[3][0] = socket;
                }
                if (socket.gameObject.name.Contains("2"))
                {
                    m_Sockets[3][1] = socket;
                }
                if (socket.gameObject.name.Contains("3"))
                {
                    m_Sockets[3][2] = socket;
                }
                if (socket.gameObject.name.Contains("4"))
                {
                    m_Sockets[3][3] = socket;
                }
            }
            if (socket.gameObject.name.Contains("LeftSide"))
            {
                m_Sockets[4][0] = socket;
            }
            if (socket.gameObject.name.Contains("Rump"))
            {
                m_Sockets[5][0] = socket;
            }
            if (socket.gameObject.name.Contains("RightSide"))
            {
                m_Sockets[6][0] = socket;
            }


        }
        rideBeast = GetComponent<RideBeastInteraction>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_StateController = GetComponent<StateController>();
        camObj = GameObject.FindWithTag("MainCamera");
        m_InteractionManager = GetComponent<InteractionManager>();
        staminaBarImage = transform.GetChild(transform.childCount - 2).GetChild(1).GetComponent<Image>();
        m_GearIndices = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            m_GearIndices[i] = new int[] { -1, -1, -1, -1 };
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        lastYRotation = transform.eulerAngles.y;
        lastPosition = transform.position;
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            BeastSaveData data = LoadBeast();
            if (LevelManager.Instance.beastLevel == 0)
            {
                m_BlockedGearSlots.Add(4);
                m_BlockedGearSlots.Add(6);
            }
            for (int i = 0; i < 7; i++)
            {
                int gearItemListIndex = -1;
                foreach (int index in data.beastGearItemIndices[i])
                {
                    if (index != -1)
                    {
                        gearItemListIndex = index;
                        break;
                    }
                }
                if (gearItemListIndex == -1) continue;
                BeastGear gear = ItemManager.Instance.GetBeastGearByIndex(gearItemListIndex).GetComponent<BeastGear>();
                if (LevelManager.Instance.beastLevel >= gear.requiredLevel)
                {
                    EquipGear(data.beastGearItemIndices[i], i, 0, gear.blockedSlotIndices);
                }

            }
            CallSetBeastCargoForEquipChest(data);
        }
    }

    public void CallSetBeastCargoForEquipChest(BeastSaveData data = null)
    {
        for (int i = 4; i < 7; i++)
        {
            if (m_Sockets[i][0].transform.childCount > 0 && m_Sockets[i][0].transform.GetChild(0).TryGetComponent<BeastStorageContainerController>(out var chest))
            {
                m_BeastChests[i - 4] = chest;
            }
            else
            {
                m_BeastChests[i - 4] = null;

            }
        }
        if (m_Sockets[1][1].transform.childCount > 0 && m_Sockets[1][1].transform.GetChild(0).TryGetComponent<BeastStorageContainerController>(out var vacuumeChest))
        {
            m_BeastChests[0] = vacuumeChest;
        }
        else
        {
            m_BeastChests[0] = null;

        }
        if (data != null)
            m_PhotonView.RPC("SetBeastCargoRPC", RpcTarget.All, data.rightChest, data.leftChest, data.rumpChest);
        else
            m_PhotonView.RPC("SetBeastCargoRPC", RpcTarget.All, "", "", "");
    }

    void Update()
    {
        if (m_PhotonView.IsMine) UpdateAnimator();
        UpdateStateBasedOnRiders();
        if (m_InteractionManager != null)
        {
            if (m_IsCamping || m_GearIndices[1][0] == -1)
            {
                m_InteractionManager.canInteract = false;
            }
            else
            {
                m_InteractionManager.canInteract = true;
            }
        }
        staminaBarImage.fillAmount = m_Stamina / m_MaxStamina;
    }
    public void Vacuum()
    {
        if (objectsInVauumRange != null)
        {
            foreach (Item item in objectsInVauumRange)
            {
                if (item == null)
                {
                    objectsInVauumRange.Remove(item);
                    break;
                }
                pv.RPC("SuckUpItem_RPC", RpcTarget.All, item.spawnId);
            }
        }
    }

    [PunRPC]
    void SuckUpItem_RPC(string spawnId)
    {
        List<Item> _itemsToVacuum = LevelManager.Instance.allItems.FindAll(x => x.spawnId == spawnId);

        if (_itemsToVacuum.Count > 0)
        {
            Item _itemToVacuum = _itemsToVacuum[0];
            if (_itemToVacuum != null && _itemToVacuum.isEquipped == false && _itemToVacuum.GetComponent<SpawnMotionDriver>().hasSaved == true)
            {
                StartCoroutine(SuckUpItemCoroutine(_itemToVacuum));
            }
        }
    }

    IEnumerator SuckUpItemCoroutine(Item item)
    {
        GameObject vacuumHead = GetComponentInChildren<VacuumGearController>().vacuumHead;
        float time = 0;
        float duration = 1;
        Vector3 startPos = item.transform.position;

        if (item != null)
        {
            while (time < duration)
            {
                if (item == null)
                {
                    break;
                }
                item.transform.position = Vector3.Lerp(startPos, vacuumHead.transform.position, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            if (item != null)
            {
                item.transform.position = vacuumHead.transform.position;
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        m_BeastChests[0].AddItem(item);
                    }
                    LevelManager.Instance.CallUpdateItemsRPC(item.spawnId);
                }
            }

            objectsInVauumRange.Remove(item);
        }
        else
        {
            objectsInVauumRange.Remove(item);
        }
    }

    public void UpdateStateBasedOnRiders()
    {
        if (riders.Count > 0)
        {
            m_StateController.aiActive = false;
            m_StateController.aiPath.enabled = false;
            if (hasDriver)
            {
                m_Rigidbody.isKinematic = false;
            }
            else
            {
                m_Rigidbody.isKinematic = true;
            }
            m_IsInStable = false;
        }
        else
        {
            m_Rigidbody.isKinematic = true;
            m_StateController.aiActive = true;
            m_StateController.aiPath.enabled = true;
        }
    }

    public void UpdateAnimator()
    {
        // Calculate rotation difference to determine turning (horizontal)
        float currentYRotation = transform.eulerAngles.y;
        float rotationDifference = Mathf.DeltaAngle(lastYRotation, currentYRotation);

        // Use the Y rotation delta directly for the Horizontal value
        float turnSmoothingFactor = 5f; // Adjust this for desired smoothness
        float targetHorizontal = Mathf.Clamp(rotationDifference * 0.1f, -1f, 1f); // Scaling factor to make the value more meaningful
        float horizontal = Mathf.Lerp(m_Animator.GetFloat("Horizontal"), targetHorizontal, turnSmoothingFactor * Time.deltaTime);

        // Calculate movement difference to determine forward/backward movement (vertical)
        Vector3 deltaPosition = transform.position - lastPosition;
        float speed = deltaPosition.magnitude / Time.deltaTime;

        // Set Animator's speed to reflect movement speed
        float maxSpeed = ridingSpeed; // Define the maximum speed of the moose
        if (!m_isRamming) m_Animator.speed = Mathf.Clamp(speed / maxSpeed, 0.5f, 2f); // Adjust between 0.5x and 2x animation speed

        // Determine if the moose is moving forward or backward
        Vector3 forward = transform.forward; // Moose's forward direction
        float direction = Vector3.Dot(deltaPosition.normalized, forward);

        // If the direction is negative, we are moving backward
        float targetVertical = Mathf.Clamp(speed / ridingSpeed, -1f, 1f);
        if (direction < 0)
        {
            targetVertical = -targetVertical; // Set negative if moving backward
        }

        // Apply a smoothing factor to reduce jitter for vertical
        float movementSmoothingFactor = 5f; // Adjust this for desired smoothness
        float vertical = Mathf.Lerp(m_Animator.GetFloat("Vertical"), targetVertical, movementSmoothingFactor * Time.deltaTime);

        // Determine if the moose is moving based on a small threshold
        bool isMoving = speed > 0.1f || Mathf.Abs(rotationDifference) > 1f;

        // If not moving, reset parameters to zero to keep idle state
        if (!isMoving)
        {
            vertical = 0;
            horizontal = 0;
            m_Animator.speed = 1f; // Reset to default speed when idle
        }

        // Update animator parameters
        m_Animator.SetBool("IsMoving", isMoving);
        m_Animator.SetFloat("Horizontal", horizontal);
        m_Animator.SetFloat("Vertical", vertical);

        // Store the current rotation and position for the next frame
        lastYRotation = currentYRotation;
        lastPosition = transform.position;
    }

    public void LevelUp(int level)
    {
        if (level < 3 && level >= 0)
        {
            m_PhotonView.RPC("LevelUpBeast_RPC", RpcTarget.All, level);
        }
        else
        {
            Debug.LogError($"{level} is out of range for a beast level");
        }
    }

    [PunRPC]
    public void LevelUpBeast_RPC(int level)
    {
        LevelManager.Instance.beastLevel = level;
        if (PhotonNetwork.IsMasterClient)
            LevelManager.Instance.SaveGameProgress(LevelManager.Instance.worldProgress, LevelManager.Instance.beastLevel);
    }

    public void CheckAndCallEvolve()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //PlayEffect
            string whichBeast = LevelManager.Instance.beastLevel switch
            {
                1 => "MamutTheBull",
                2 => "MamutTheBeast",
                _ => "MamutTheCalf",
            };

            if (!transform.gameObject.name.Contains(whichBeast))
            {
                GameObject newMoose = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", whichBeast), transform.position, transform.rotation);
                GameObject stableGameObject = GameObject.FindGameObjectWithTag("BeastSpawnPoint");
                if (whichBeast == "MamutTheBull")
                {
                    m_BlockedGearSlots.Remove(4);
                    m_BlockedGearSlots.Remove(6);
                }
                if (stableGameObject != null)
                {
                    BeastStableController stable = stableGameObject.GetComponentInParent<BeastStableController>();
                    stable.m_BeastObject = newMoose;
                    stable.m_BeastObject.GetComponent<BeastManager>().m_IsInStable = true;
                    stable.m_BeastObject.GetComponent<BeastManager>().m_BeastStableController = stable;
                    m_PhotonView.RPC("InitializeBeastWithStable", RpcTarget.OthersBuffered, stable.GetComponent<Item>().id);
                }

                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }

    [PunRPC]
    public void SetBeastCargoRPC(string rightChest = "", string leftChest = "", string rumpChest = "")
    {
        if (m_BeastChests[0] != null) m_BeastChests[0].m_State = rightChest;
        if (m_BeastChests[1] != null) m_BeastChests[1].m_State = leftChest;
        if (m_BeastChests[2] != null) m_BeastChests[2].m_State = rumpChest;
        SaveBeastStorage();

    }

    public void CallSetRiders(int photonView, int playerId)
    {
        m_PhotonView.RPC("SetRiders", RpcTarget.AllBuffered, photonView, playerId);
    }


    [PunRPC]
    public void SetRiders(int photonId, int playerId)
    {
        ThirdPersonCharacter player = PhotonView.Find(photonId).GetComponent<ThirdPersonCharacter>();
        if (riders.ContainsKey(photonId))
        {
            riders.Remove(photonId);
            player.isRiding = false;
            if (player.seatNumber == 1) hasDriver = false;
            player.seatNumber = 0;
            player.GetComponent<Collider>().isTrigger = false;
            player.GetComponent<PhotonTransformViewClassic>().enabled = true;

            player.GetComponentInChildren<Animator>().SetBool("Riding", false);

        }
        else
        {
            // add the rider;
            for (int j = 1; j < 5; j++)
            {
                if (!riders.ContainsValue(j))
                {
                    player.GetComponent<PhotonTransformViewClassic>().enabled = false;
                    riders.Add(photonId, j);
                    player.isRiding = true;
                    if (j == 1)
                    {
                        hasDriver = true;
                        m_PhotonView.TransferOwnership(playerId);
                        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
                        {
                            m_StateController.enabled = true;
                        }
                        else
                        {
                            m_StateController.enabled = false;
                        }
                    }
                    player.seatNumber = j;
                    player.GetComponent<Collider>().isTrigger = true;
                    player.GetComponentInChildren<Animator>().SetBool("Riding", true);
                    break;
                }
            }
        }
    }

    public void Hit()
    {
        m_PhotonView.RPC("SetCamping", RpcTarget.All);
    }

    [PunRPC]
    public void SetCamping()
    {
        if (riders.Count > 0) return;
        m_IsCamping = !m_IsCamping;
        if (m_PhotonView.IsMine)
        {
            // This prevents players from getting stuck on the beasts collider when he is laying down
            if (m_IsCamping)
            {
                m_Collider.center = new Vector3(m_OriginalColliderCenter.x, m_OriginalColliderCenter.y, m_OriginalColliderCenter.z + .5f);
            }
            else
            {
                m_Collider.center = m_OriginalColliderCenter;
            }

            m_Animator.SetBool("Camping", m_IsCamping);
        }
    }
    public void CallSetBeastStamina(float staminaValue)
    {
        m_PhotonView.RPC("SetBeastStamina", RpcTarget.All, staminaValue);
    }
    [PunRPC]
    public void SetBeastStamina(float staminaValue)
    {
        m_Stamina += staminaValue;
        if (m_Stamina < 0)
        {
            m_Stamina = 0;
        }
        if (m_Stamina > m_MaxStamina)
        {
            m_Stamina = m_MaxStamina;
        }
    }

    public void CallBeastMove(Vector2 move, bool ram)
    {
        m_PhotonView.RPC("BeastMove", RpcTarget.All, move, ram);
    }

    [PunRPC]
    public void BeastMove(Vector2 move, bool ram)
    {
        if (!m_PhotonView.IsMine || m_Animator.GetBool("Eating")) return;

        Rigidbody rb = GetComponent<Rigidbody>(); // Ensure Rigidbody is on the beast
        if (ram || m_isRamming)
        {
            HandleRamming(ram);
        }

        HandleMovement(move, rb);

    }

    private void HandleRamming(bool ram)
    {
        if (m_GearIndices[2][0] != 0) return;
        if (!m_isRamming && ram && m_Stamina > 0)
        {
            m_isRamming = true;
            m_Animator.SetBool("Ram", true);
            m_PhotonView.RPC("SetBeastStamina", RpcTarget.All, -30f);
        }
        // Animation speed needs to be set to one because the animation speed
        // is set based on movement speed for the walking animation
        if (m_isRamming)
        {
            m_Animator.speed = 1f;
        }

        if (!m_Animator.GetBool("Ram") && m_isRamming)
        {
            m_isRamming = false;
            m_Animator.SetBool("Ram", false);
        }

        // if (m_Stamina > 0)
        // {
        //     Rigidbody rb = GetComponent<Rigidbody>();
        //     Vector3 forward = transform.forward * m_RamSpeed * Time.deltaTime;
        //     rb.MovePosition(rb.position + forward);
        // }
    }
    private void HandleMovement(Vector2 move, Rigidbody rb)
    {
        if (move.magnitude > 1f) move.Normalize();

        // Calculate target speed and turn speed based on input
        float targetSpeed = ridingSpeed * move.y;
        float targetTurnSpeed = turnSpeed * 50 * move.x;

        // Determine the appropriate acceleration or deceleration factor
        float speedFactor = targetSpeed > m_CurrentSpeed ? acceleration : acceleration * 5;

        // Smoothly interpolate current speed and turn speed towards target values
        m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, targetSpeed, speedFactor * Time.deltaTime);
        m_CurrentTurnSpeed = Mathf.Lerp(m_CurrentTurnSpeed, targetTurnSpeed, turnAcceleration * Time.deltaTime);

        // Apply rotation and forward movement
        transform.Rotate(0, m_CurrentTurnSpeed * Time.deltaTime, 0);
        Vector3 forward = m_CurrentSpeed * Time.deltaTime * transform.forward;
        rb.MovePosition(rb.position + forward);

        // Reduce stamina when moving
        if (m_Stamina > 0)
        {
            if (rb.velocity.magnitude > 0.01f)
                m_PhotonView.RPC("SetBeastStamina", RpcTarget.All, -Time.deltaTime * 0.3f);
        }
        else
        {
            m_CurrentSpeed /= 3; // Slow down if stamina is depleted
        }
    }


    public void CallFeedBeast(float foodValue)
    {
        m_PhotonView.RPC("FeedBeast", RpcTarget.All, foodValue);
    }

    [PunRPC]
    public void FeedBeast(float foodValue)
    {
        if (!m_PhotonView.IsMine) return;
        m_Animator.SetBool("IsMoving", false);
        m_Animator.SetBool("Eating", true);
        CallSetBeastStamina(foodValue);
    }

    public BeastSaveData LoadBeast()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + "beast.json";
        string json;
        try
        {
            json = File.ReadAllText(filePath);
            BeastSaveData data = JsonConvert.DeserializeObject<BeastSaveData>(json);

            return data;
        }
        catch
        {
            int[][] empty = new int[7][]; // Initialize the jagged array with 6 rows

            // Fill the array based on the structure
            empty[0] = new int[] { -1, -1, -1, -1 };
            empty[1] = new int[] { -1, -1, -1, -1 };
            empty[2] = new int[] { -1, -1, -1, -1 };
            empty[3] = new int[] { -1, -1, -1, -1 };
            empty[4] = new int[] { -1, -1, -1, -1 };
            empty[5] = new int[] { -1, -1, -1, -1 };
            empty[6] = new int[] { -1, -1, -1, -1 };

            return new BeastSaveData(empty, "", "", "");
        }
    }

    public void SaveBeastStorage()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + "beast.json";
        string chestState1 = "";
        string chestState2 = "";
        string chestState3 = "";
        if (m_BeastChests[0])
        {
            chestState1 = m_BeastChests[0].m_State;
        }
        if (m_BeastChests[1])
        {
            chestState2 = m_BeastChests[1].m_State;
        }
        if (m_BeastChests[2])
        {
            chestState3 = m_BeastChests[2].m_State;
        }
        BeastSaveData beastSaveData = new BeastSaveData(m_GearIndices, chestState1, chestState2, chestState3);

        string json = JsonConvert.SerializeObject(beastSaveData);

        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file

            writer.Write(json);
        }
    }
    public void EquipGear(int[] gearItemIndices, int gearIndex, int gearLevel, int[] blockedGearSlots)
    {
        m_PhotonView.RPC("EquipGearPRC", RpcTarget.All, gearItemIndices, gearIndex, gearLevel, blockedGearSlots);
        m_GearIndices[gearIndex] = gearItemIndices;
    }

    [PunRPC]
    public void EquipGearPRC(int[] gearItemIndices, int gearIndex, int gearLevel, int[] blockedGearSlots)
    {
        int[] emptyGear = new int[] { -1, -1, -1, -1 };

        //Check to make sure the gear level requirement is met
        if (gearLevel > LevelManager.Instance.beastLevel)
        {
            Debug.Log("Beast level is too low to equip this gear");
            return;
        }
        //Make sure the gear slot is not blocked by another gear slot
        if (m_BlockedGearSlots.Contains(gearIndex))
        {
            Debug.Log("This gear slot is currently blocked");
            return;
        }
        if (gearItemIndices.SequenceEqual(emptyGear))
        {
            foreach (int blockedGearSlot in blockedGearSlots)
            {
                m_BlockedGearSlots.Remove(blockedGearSlot);
            }
            if (LevelManager.Instance.beastLevel == 0)
            {
                if (!m_BlockedGearSlots.Contains(4)) m_BlockedGearSlots.Add(4);
                if (!m_BlockedGearSlots.Contains(6)) m_BlockedGearSlots.Add(6);
            }
        }
        else
        {
            // Add the blocked gear slots to the list
            foreach (int blockedGearSlot in blockedGearSlots)
            {
                m_BlockedGearSlots.Add(blockedGearSlot);
            }
        }

        for (int i = 0; i < m_Sockets[gearIndex].Length; i++)
        {
            if (m_Sockets[gearIndex][i].transform.childCount > 0)
            {
                //Destroy existing object if it exists
                if (m_Sockets[gearIndex][i].transform.GetChild(0).gameObject.TryGetComponent<BeastStableController>(out var chest))
                {
                    SaveBeastStorage();
                }
                if (gearIndex is 1 && rideBeast != null)
                {
                    rideBeast.canInteract = false;
                }
                Destroy(m_Sockets[gearIndex][i].transform.GetChild(0).gameObject);
            }

            if (gearItemIndices[i] != -1)
            {
                Instantiate(ItemManager.Instance.GetBeastGearByIndex(gearItemIndices[i]), m_Sockets[gearIndex][i].transform.position, m_Sockets[gearIndex][i].transform.rotation, m_Sockets[gearIndex][i].transform);
            }
        }
        m_GearIndices[gearIndex] = gearItemIndices;
        if (gearItemIndices[0] is 2)
        {
            CallSetBeastCargoForEquipChest();
        }
        if (gearIndex == 1 && rideBeast != null)
        {
            rideBeast.canInteract = true;
        }
        SaveBeastStorage();
    }
    public void CallSaveBeastRPC(string data, string chestName)
    {
        m_PhotonView.RPC("SaveBeastRPC", RpcTarget.All, data, chestName);
    }
    [PunRPC]
    public void SaveBeastRPC(string data, string chestName)
    {
        GameObject.Find(chestName).GetComponent<BeastStorageContainerController>().m_State = data;
        SaveBeastStorage();
    }
    public void CallSetRamTargetHealthManagerRPR(int ramTargetViewId)
    {
        m_PhotonView.RPC("SetRamTargetHealthManagerRPR", RpcTarget.MasterClient, ramTargetViewId);
    }
    [PunRPC]
    public void SetRamTargetHealthManagerRPR(int ramTargetViewId)
    {
        m_RamTarget = PhotonView.Find(ramTargetViewId).gameObject;
    }
    public void CallSetRamTargetSourceObjectRPR(string ramTargetId)
    {
        m_PhotonView.RPC("SetRamTargetSourceObjectRPR", RpcTarget.MasterClient, ramTargetId);
    }
    [PunRPC]
    public void SetRamTargetSourceObjectRPR(string ramTargetId)
    {
        SourceObject[] sourceObjs = FindObjectsOfType<SourceObject>();
        foreach (SourceObject sourceObj in sourceObjs)
        {
            if (ramTargetId == sourceObj.id)
            {
                m_RamTarget = sourceObj.gameObject;
            }
        }
    }
}

public class BeastSaveData
{
    public int[][] beastGearItemIndices;
    public string leftChest;
    public string rightChest;
    public string rumpChest;
    public BeastSaveData(int[][] beastGearItemIndex, string rightChest, string leftChest, string rumpChest)
    {
        this.beastGearItemIndices = beastGearItemIndex;
        this.rightChest = rightChest;
        this.leftChest = leftChest;
        this.rumpChest = rumpChest;
    }
}
