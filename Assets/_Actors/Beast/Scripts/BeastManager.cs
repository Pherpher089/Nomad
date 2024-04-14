using UnityEngine;
using Photon.Pun;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

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
    public float ridingSped = 13;

    public float turnSpeed = 2.5f;
    float m_xMovement;
    float m_zMovement;
    Rigidbody m_Rigidbody;
    StateController m_StateController;
    GameObject camObj;
    private float lastYRotation;
    private Vector3 lastPosition;

    public bool m_isRamming = false;
    public float m_RamSpeed = 30;
    InteractionManager m_InteractionManager;
    BoxCollider m_Collider;
    Vector3 m_OriginalColliderCenter;
    void Awake()
    {
        m_Collider = GetComponent<BoxCollider>();
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

    }
    // Start is called before the first frame update
    void Start()
    {
        lastYRotation = transform.eulerAngles.y;
        lastPosition = transform.position;
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
        float currentYRotation = transform.eulerAngles.y;
        float rotationDifference = Mathf.DeltaAngle(lastYRotation, currentYRotation);

        float maxTurningSpeed = 10.0f;
        float h = Mathf.Clamp(rotationDifference / maxTurningSpeed, -1f, 1f);


        Vector3 deltaPosition = transform.position - lastPosition;
        float v = deltaPosition.magnitude / Time.deltaTime;
        float maxSpeed = 10f; // This should be the maximum speed you expect the beast to move at
        v = Mathf.Clamp(v / maxSpeed, 0f, 1f);

        if (Mathf.Abs(v) > 0.01f || Mathf.Abs(h) > 0.01f)
        {
            m_Animator.SetBool("IsMoving", true);
            m_Animator.SetFloat("Vertical", v);
            m_Animator.SetFloat("Horizontal", h);
        }
        else
        {
            m_Animator.SetBool("IsMoving", false);
        }
        lastYRotation = currentYRotation;
        lastPosition = transform.position;
    }

    [PunRPC]
    public void SetBeastCargoRPC(string rightChest, string leftChest)
    {
        m_BeastChests[0].m_State = rightChest;
        m_BeastChests[1].m_State = leftChest;

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
        if (PhotonNetwork.IsMasterClient)
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

    public void CallBeastMove(Vector2 move, bool ram)
    {
        m_PhotonView.RPC("BeastMove", RpcTarget.All, move, ram);
    }

    [PunRPC]
    public void BeastMove(Vector2 move, bool ram)
    {
        if (!m_PhotonView.IsMine) return;
        if (ram || m_isRamming)
        {
            if (!m_isRamming)
            {
                m_isRamming = true;
                m_Animator.SetBool("Ram", true);
            }

            if (!m_Animator.GetBool("Ram") && m_isRamming)
            {
                m_isRamming = false;
                m_Animator.SetBool("Ram", false);
            }

            //Check for gear
            transform.Translate(new Vector3(0, 0, m_RamSpeed * Time.deltaTime));
        }
        else
        {
            if (move.magnitude > 1f) move.Normalize();
            //move = camObj.transform.TransformDirection(new Vector3(move.x, 0, move.y * 1.5f));
            m_xMovement = turnSpeed * move.x;
            m_zMovement = ridingSped * move.y * Time.deltaTime;
            transform.Rotate(0, m_xMovement, 0);
            transform.Translate(new Vector3(0, 0, m_zMovement));
        }
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
            Debug.Log(json);
            BeastSaveData data = JsonConvert.DeserializeObject<BeastSaveData>(json);
            Debug.Log(data);

            return data;
        }
        catch
        {
            Debug.Log("~ No beast to load, creating new beast");
            return new BeastSaveData(-1, "", "");
        }
    }

    public void SaveBeast()
    {
        if (!GetComponent<PhotonView>().IsMine) return;
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
        m_PhotonView.RPC("EquipGearPRC", RpcTarget.All, gearItemIdex);
        m_GearIndex = gearItemIdex;
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
