using UnityEngine;
using Photon.Pun;
using System.IO;
public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    public int playerNum = -1;
    public GameObject controller;
    public bool initialized;
    public bool initComplete;
    void Awake()
    {
        playerNum = -1;
        initialized = false;
        initComplete = false;
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (pv.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        //Initialize it all!
        //if client request and apply world data
        //If this is the server, load player position;
        //If client, set position to the existing players position;
        //Vector3 position; //set position
        //Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();

        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DonteOnline"), transform.position, Quaternion.identity, 0, new object[] { pv.ViewID });
        Debug.Log("### creating ");

    }
    [PunRPC]
    public void Initialize(int _playerNum)
    {
        Debug.Log("start initializing " + _playerNum + " " + playerNum);

        if (playerNum == -1)
        {
            playerNum = _playerNum;
            initialized = true;
            Debug.Log("initializing " + playerNum);
        }
    }

    void Update()
    {
        if (!initComplete && initialized)
        {
            SetPlayerNumber(playerNum);
            Debug.Log("initialized " + playerNum);
        }
    }
    void SetPlayerNumber(int _playerNum)
    {
        PlayerNumber num = GetPlayerNumber(_playerNum);
        if (controller != null)
        {
            Debug.Log("Setting player number " + _playerNum);
            controller.GetComponent<ThirdPersonUserControl>().playerNum = num;
            controller.GetComponent<ThirdPersonUserControl>().SetPlayerPrefix(num);
        }
    }
    PlayerNumber GetPlayerNumber(int number)
    {
        switch (number)
        {
            case 0:
                return PlayerNumber.Single_Player;
            case 1:
                return PlayerNumber.Player_1;
            case 2:
                return PlayerNumber.Player_2;
            case 3:
                return PlayerNumber.Player_3;
            case 4:
                return PlayerNumber.Player_4;
        }
        return PlayerNumber.Single_Player;
    }
    // public void Die()
    // {
    //     PhotonNetwork.Destroy(controller);
    //     CreateController();
    // }
}
