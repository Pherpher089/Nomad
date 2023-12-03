using System;
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
    Vector3 spawnPoint;
    public int playerColorIndex;
    void Awake()
    {
        playerNum = -1;
        initialized = false;
        initComplete = false;
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            UpdateGameStateForPlayer();
        }
        if (pv.IsMine)
        {
            CreateController();
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
    void ApplyPlayerColor(GameObject player)
    {
        if (pv.IsMine)
        {
            Color playerColor = (Color)PhotonNetwork.LocalPlayer.CustomProperties["PlayerColor"];
            // Apply this color to the player's material
            GetComponent<Renderer>().material.color = playerColor;
        }
    }
    void CreateController()
    {
        spawnPoint = transform.position;
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DonteOnline"), spawnPoint, Quaternion.identity, 0, new object[] { pv.ViewID });
        LevelManager.Instance.CallUpdatePlayerColorPRC(controller.GetComponent<PhotonView>().ViewID, playerColorIndex);
        if (PhotonNetwork.IsMasterClient && FindObjectOfType<NonmasterBeastInitialization>() == null)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "TheBeast"), spawnPoint + new Vector3(-8, 0, -8), Quaternion.identity);
        }
    }
    [PunRPC]
    public void Initialize(int _playerNum, int colorIndex)
    {
        if (playerNum == -1)
        {
            playerNum = _playerNum;
            playerColorIndex = colorIndex;
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
