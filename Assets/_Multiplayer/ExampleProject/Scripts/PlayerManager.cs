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
        if (LevelPrep.Instance.playerSpawnName != "")
        {
            PlayerSpawnPoint[] spawns = GameObject.FindObjectsOfType<PlayerSpawnPoint>();
            foreach (PlayerSpawnPoint spawn in spawns)
            {
                if (spawn.spawnName == LevelPrep.Instance.playerSpawnName)
                {
                    spawnPoint = spawn.transform.position;
                }
            }
            if (spawnPoint == transform.position)
            {
                PortalInteraction[] portals = GameObject.FindObjectsOfType<PortalInteraction>();
                foreach (PortalInteraction portal in portals)
                {
                    if (portal.destinationLevel == LevelPrep.Instance.playerSpawnName)
                    {
                        spawnPoint = portal.transform.position;
                    }
                }
            }
        }

        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DonteOnline"), spawnPoint, Quaternion.identity, 0, new object[] { pv.ViewID });
        LevelManager.Instance.CallUpdatePlayerColorPRC(controller.GetComponent<PhotonView>().ViewID, playerColorIndex);

        if (PhotonNetwork.IsMasterClient && FindObjectOfType<BeastManager>() == null)
        {
            BeastSpawnPoint beastSpawn = null;
            if (GameObject.FindGameObjectWithTag("BeastSpawnPoint"))
            {
                BeastStableController stable = GameObject.FindGameObjectWithTag("BeastSpawnPoint").GetComponentInParent<BeastStableController>();
                spawnPoint = GameObject.FindGameObjectWithTag("BeastSpawnPoint").transform.position;
                stable.m_BeastObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "TheBeast"), spawnPoint, Quaternion.identity);
                stable.m_BeastObject.GetComponent<BeastManager>().m_IsInStable = true;
                stable.m_BeastObject.GetComponent<BeastManager>().m_BeastStableController = stable;
                pv.RPC("InitializeBeastWithStable", RpcTarget.OthersBuffered, stable.GetComponent<Item>().id);
            }
            else
            {
                if (LevelPrep.Instance.playerSpawnName != "")
                {
                    BeastSpawnPoint[] spawns = FindObjectsOfType<BeastSpawnPoint>();
                    foreach (BeastSpawnPoint spawn in spawns)
                    {
                        if (spawn.spawnName == LevelPrep.Instance.playerSpawnName)
                        {
                            spawnPoint = spawn.transform.position;
                            beastSpawn = spawn;
                        }
                    }

                }
                GameObject beastObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "TheBeast"), spawnPoint + new Vector3(-8, 0, -8), Quaternion.identity);
                if (beastSpawn)
                {
                    beastObj.GetComponent<StateController>().currentState = beastSpawn.startingState;
                }
            }
        }
    }
    [PunRPC]
    public void InitializeBeastWithStable(string stableId)
    {
        BeastStableController[] stables = FindObjectsOfType<BeastStableController>();
        foreach (BeastStableController stable in stables)
        {
            if (stable.GetComponent<Item>().id == stableId)
            {
                stable.m_BeastObject = GameObject.FindGameObjectWithTag("Beast");
                stable.m_BeastObject.GetComponent<BeastManager>().m_BeastStableController = stable;
                stable.m_BeastObject.GetComponent<BeastManager>().m_IsInStable = true;
            }
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
