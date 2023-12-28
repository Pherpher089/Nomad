using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(HealthManager))]
public class BeastManager : MonoBehaviour
{
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
    // Start is called before the first frame update
    void Awake()
    {
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        m_HealthManager = GetComponent<HealthManager>();
        m_Socket = transform.GetChild(1).gameObject;
        if (PhotonNetwork.IsMasterClient)
        {
            BeastSaveData data = LoadBeast();
            EquipGear(data.beastGearItemIndex);
        }
    }

    public void Hit()
    {
        m_PhotonView.RPC("SetCamping", RpcTarget.All);
    }

    [PunRPC]
    public void SetCamping()
    {
        m_IsCamping = !m_IsCamping;
        if (PhotonNetwork.IsMasterClient)
        {
            m_Animator.SetBool("Camping", m_IsCamping);
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
            BeastSaveData data = JsonConvert.DeserializeObject<BeastSaveData>(json);
            return data;
        }
        catch
        {
            Debug.Log("~ Level Data does not exist");
            return new BeastSaveData(-1);
        }
    }

    public void SaveBeast()
    {
        if (!GetComponent<PhotonView>().IsMine) return;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + "beast.json";
        BeastSaveData beastSaveData = new BeastSaveData(m_GearIndex);
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
            m_BeastStableController.m_SaddleStationController.AddItem(ItemManager.Instance.beastGearList[m_GearIndex].GetComponent<Item>());
            Destroy(m_Socket.transform.GetChild(0).gameObject);
        }
        if (gearItemIdex != -1)
        {
            GameObject gear = Instantiate(ItemManager.Instance.GetBeastGearByIndex(gearItemIdex), transform.position, transform.rotation, m_Socket.transform);
            gear.GetComponent<BeastGear>().beastManager = this;
        }
        m_GearIndex = gearItemIdex;
        if (PhotonNetwork.IsMasterClient)
        {
            SaveBeast();
        }
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
    public BeastSaveData(int beastGearItemIndex)
    {
        this.beastGearItemIndex = beastGearItemIndex;
    }
}
