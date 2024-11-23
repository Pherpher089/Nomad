using UnityEngine;
using Photon.Pun;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using System.Reflection;
using UnityEngine.UI;
using System;
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(HealthManager))]
public class BeastManager : MonoBehaviour
{
    public static BeastManager Instance;
    Animator m_Animator;
    PhotonView m_PhotonView;
    HealthManager m_HealthManager;
    public BeastStableController m_BeastStableController;
    public bool m_IsCamping = false;
    public bool m_IsInStable = false;
    public int m_GearIndex;
    public string m_SaveFilePath;
    GameObject m_Socket;
    public GameObject m_RamTarget;
    public BeastStorageContainerController[] m_BeastChests = new BeastStorageContainerController[2];
    public RideBeastInteraction rideBeast;
    public Dictionary<int, int> riders;
    public bool hasDriver = false;

    Rigidbody m_Rigidbody;
    StateController m_StateController;
    GameObject camObj;
    private float lastYRotation;
    private Vector3 lastPosition;


    InteractionManager m_InteractionManager;
    CapsuleCollider m_Collider;
    Vector3 m_OriginalColliderCenter;

    Image staminaBarImage;
    public float m_Stamina;

    public bool m_isRamming = false;
    public float ridingSpeed = 25;
    public float turnSpeed = 4f;
    public float m_RamSpeed = 30;
    public float m_MaxStamina;
    private float m_CurrentSpeed = 0;
    public float acceleration = 1;
    public float turnAcceleration = 10;
    private float m_CurrentTurnSpeed = 0;
    PhotonView pv;
    void Awake()
    {
        m_Collider = GetComponent<CapsuleCollider>();
        m_OriginalColliderCenter = m_Collider.center;
        Instance = this;
        riders = new Dictionary<int, int>();
        m_Animator = GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        m_HealthManager = GetComponent<HealthManager>();
        m_Socket = GameObject.FindGameObjectWithTag("BeastGearSocket");
        rideBeast = GetComponent<RideBeastInteraction>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_StateController = GetComponent<StateController>();
        camObj = GameObject.FindWithTag("MainCamera");
        m_InteractionManager = GetComponent<InteractionManager>();
        staminaBarImage = transform.GetChild(transform.childCount - 2).GetChild(1).GetComponent<Image>();

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
            EquipGear(data.beastGearItemIndex);
            m_PhotonView.RPC("SetBeastCargoRPC", RpcTarget.All, data.rightChest, data.leftChest);

        }
    }

    void Update()
    {
        if (m_PhotonView.IsMine) UpdateAnimator();
        UpdateStateBasedOnRiders();
        if (m_IsCamping)
        {
            m_InteractionManager.canInteract = false;
        }
        else
        {
            m_InteractionManager.canInteract = true;
        }
        staminaBarImage.fillAmount = m_Stamina / m_MaxStamina;
    }

    public void UpdateStateBasedOnRiders()
    {
        if (riders.Count > 0)
        {
            m_StateController.aiActive = false;
            m_StateController.navMeshAgent.enabled = false;
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
            m_StateController.navMeshAgent.enabled = true;
        }
    }

    public void UpdateAnimator()
    {
        // Calculate rotation difference to determine turning (horizontal)
        float currentYRotation = transform.eulerAngles.y;
        float rotationDifference = Mathf.DeltaAngle(lastYRotation, currentYRotation);

        // Use the Y rotation delta directly for the Horizontal value
        float turnSmoothingFactor = 5f; // Adjust this for desired smoothness
        float targetHorizontal = rotationDifference * 0.1f; // Scaling factor to make the value more meaningful
        float horizontal = Mathf.Lerp(m_Animator.GetFloat("Horizontal"), targetHorizontal, turnSmoothingFactor * Time.deltaTime);

        // Calculate movement difference to determine forward/backward movement (vertical)
        Vector3 deltaPosition = transform.position - lastPosition;
        float speed = deltaPosition.magnitude / Time.deltaTime;

        // Set Animator's speed to reflect movement speed
        float maxSpeed = ridingSpeed; // Define the maximum speed of the moose
        m_Animator.speed = Mathf.Clamp(speed / maxSpeed, 0.5f, 2f); // Adjust between 0.5x and 2x animation speed

        // Determine if the moose is moving forward or backward
        Vector3 forward = transform.forward; // Moose's forward direction
        float direction = Vector3.Dot(deltaPosition.normalized, forward);

        // If the direction is negative, we are moving backward
        float targetVertical = Mathf.Clamp(speed / ridingSpeed, 0f, 1f);
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
    public void SetBeastCargoRPC(string rightChest, string leftChest)
    {
        if (LevelManager.Instance.beastLevel == 2)
        {
            m_BeastChests[0].m_State = rightChest;
            m_BeastChests[1].m_State = leftChest;
            SaveBeast();
        }
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
            player.GetComponent<PhotonTransformView>().enabled = true;

            player.GetComponentInChildren<Animator>().SetBool("Riding", false);

        }
        else
        {
            // add the rider;
            for (int j = 1; j < 5; j++)
            {
                if (!riders.ContainsValue(j))
                {
                    player.GetComponent<PhotonTransformView>().enabled = false;
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
        else
        {
            HandleMovement(move, rb);
        }
    }

    private void HandleRamming(bool ram)
    {
        if (!m_isRamming && ram && m_Stamina > 0 && m_GearIndex == 0)
        {
            m_isRamming = true;
            m_Animator.SetBool("Ram", true);
            m_PhotonView.RPC("SetBeastStamina", RpcTarget.All, -30f);
        }

        if (!m_Animator.GetBool("Ram") && m_isRamming)
        {
            m_isRamming = false;
            m_Animator.SetBool("Ram", false);
        }

        if (m_Stamina > 0 && m_GearIndex == 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 forward = transform.forward * m_RamSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + forward);
        }
    }
    private void HandleMovement(Vector2 move, Rigidbody rb)
    {
        if (move.magnitude > 1f) move.Normalize();
        float modifierY = 1;
        if (move.y < 0.1f && move.y > -0.1f)
        {
            modifierY = 2;
        }
        float modifierX = 1;
        if (move.x < 0.1f && move.x > -0.1f)
        {
            modifierX = 2;
        }
        // Apply acceleration and deceleration for smoother movement
        m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, ridingSpeed * move.y, acceleration * Time.deltaTime * modifierY);
        m_CurrentTurnSpeed = Mathf.Lerp(m_CurrentTurnSpeed, turnSpeed * move.x, turnAcceleration * Time.deltaTime * modifierX);

        // Update rotation and forward movement
        transform.Rotate(0, m_CurrentTurnSpeed, 0);
        Vector3 forward = m_CurrentSpeed * Time.deltaTime * transform.forward;
        rb.MovePosition(rb.position + forward);

        // Reduce stamina when moving
        if (m_Stamina > 0)
        {
            if (m_Rigidbody.velocity.magnitude > 0.01f)
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
            return new BeastSaveData(-1, "", "");
        }
    }

    public void SaveBeast()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + "beast.json";
        BeastSaveData beastSaveData = new BeastSaveData(m_GearIndex, m_BeastChests[0].m_State, m_BeastChests[1].m_State);
        string json = JsonConvert.SerializeObject(beastSaveData);
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }
    public void EquipGear(int gearItemIdex)
    {
        //Later this will be based on gear equipped
        if (LevelManager.Instance.beastLevel == 2)
        {
            m_PhotonView.RPC("EquipGearPRC", RpcTarget.All, gearItemIdex);
            m_GearIndex = gearItemIdex;
        }
    }
    [PunRPC]
    public void EquipGearPRC(int gearItemIdex)
    {
        if (m_Socket.transform.childCount > 0)
        {
            m_BeastStableController.m_SaddleStationController.AddItem(ItemManager.Instance.beastGearList[m_GearIndex].GetComponent<BeastGear>());
            Destroy(m_Socket.transform.GetChild(0).gameObject);
        }
        if (gearItemIdex != -1)
        {
            GameObject gear = Instantiate(ItemManager.Instance.GetBeastGearByIndex(gearItemIdex), m_Socket.transform.position, m_Socket.transform.rotation, m_Socket.transform);
        }
        m_GearIndex = gearItemIdex;
        SaveBeast();

    }
    public void CallSaveBeastRPC(string data, string chestName)
    {
        m_PhotonView.RPC("SaveBeastRPC", RpcTarget.All, data, chestName);
    }
    [PunRPC]
    public void SaveBeastRPC(string data, string chestName)
    {
        GameObject.Find(chestName).GetComponent<BeastStorageContainerController>().m_State = data;
        SaveBeast();
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
    public int beastGearItemIndex;
    public string leftChest;
    public string rightChest;
    public BeastSaveData(int beastGearItemIndex, string rightChest, string leftChest)
    {
        this.beastGearItemIndex = beastGearItemIndex;
        this.rightChest = rightChest;
        this.leftChest = leftChest;
    }
}
