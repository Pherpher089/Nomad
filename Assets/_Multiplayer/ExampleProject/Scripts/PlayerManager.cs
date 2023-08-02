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
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            UpdateGameStateForPlayer();
        }
    }
    void UpdateGameStateForPlayer()
    {
        if (!LevelPrep.Instance.receivedLevelFiles)
        {
            LevelPrep.Instance.receivedLevelFiles = true;
            if (!PhotonNetwork.IsMasterClient)
            {
                string LevelDataKey = "LevelData";
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(LevelDataKey, out object levelDataValue))
                {
                    string levelData = (string)levelDataValue;
                    LevelManager.Instance.SaveProvidedLevelData(levelData);
                }
            }
        }
    }
    void CreateController()
    {
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DonteOnline"), transform.position + Vector3.up * .35f, Quaternion.identity, 0, new object[] { pv.ViewID });
    }
    [PunRPC]
    public void Initialize(int _playerNum)
    {

        if (playerNum == -1)
        {
            playerNum = _playerNum;
            initialized = true;
        }
    }

    void Update()
    {
        if (!initComplete && initialized)
        {
            SetPlayerNumber(playerNum);
            initComplete = true;
        }
    }
    void SetPlayerNumber(int _playerNum)
    {
        PlayerNumber num = GetPlayerNumber(_playerNum);
        if (controller != null)
        {
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
