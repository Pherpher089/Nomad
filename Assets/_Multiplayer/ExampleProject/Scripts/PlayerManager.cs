using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using Newtonsoft.Json;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    public int playerNum = -1;
    public GameObject controller;
    public bool initialized;
    public bool initComplete;
    Vector3 spawnPoint;
    public int playerColorIndex;
    public string playerName;
    public const string playerPosKey = "GroupCenterPosition";

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
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        spawnPoint = Vector3.zero;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(playerPosKey, out object groupCenterObj);
        Vector3 groupCenter = (Vector3)groupCenterObj;
        if (!PhotonNetwork.IsMasterClient && groupCenter != null && groupCenter != Vector3.zero)
        {
            spawnPoint = groupCenter;

        }
        else
        {
            FindSpawnerAndSetSpawnPoint();

            if (spawnPoint == Vector3.zero)
            {
                switch (LevelManager.Instance.worldProgress)
                {
                    case 0:
                        LevelPrep.Instance.currentLevel = SceneManager.GetActiveScene().name;
                        LevelPrep.Instance.playerSpawnName = "start-tutorial";
                        break;
                    case 1:
                        LevelPrep.Instance.currentLevel = SceneManager.GetActiveScene().name;
                        LevelPrep.Instance.playerSpawnName = "start";
                        break;
                }

                FindSpawnerAndSetSpawnPoint();
            }
        }

        Vector3 spawnModifier = Vector3.right * playerNum;
        spawnModifier.y = 0;

        spawnPoint = new(UnityEngine.Random.Range(-3, 0) + spawnPoint.x, spawnPoint.y + 1, UnityEngine.Random.Range(-3, 0) + spawnPoint.z);
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "RealmWalker"), spawnPoint + spawnModifier, Quaternion.identity, 0, new object[] { pv.ViewID });
        controller.GetComponent<ThirdPersonUserControl>().characterName = playerName;
        LevelManager.Instance.CallUpdatePlayerColorPRC(controller.GetComponent<PhotonView>().ViewID, playerColorIndex);
        PlayersManager.Instance.UpdatePlayers();
        if (PhotonNetwork.IsMasterClient && FindObjectOfType<BeastManager>() == null)
        {
            string whichBeast = LevelManager.Instance.beastLevel switch
            {
                1 => "MamutTheBull",
                2 => "MamutTheBeast",
                _ => "MamutTheCalf",
            };
            BeastSpawnPoint beastSpawn = null;
            // If we find the beast spawner in the stable - Spawn him there
            if (GameObject.FindGameObjectWithTag("BeastSpawnPoint"))
            {
                BeastStableController stable = GameObject.FindGameObjectWithTag("BeastSpawnPoint").GetComponentInParent<BeastStableController>();
                GameObject _beastSpawn = GameObject.FindGameObjectWithTag("BeastSpawnPoint");
                spawnPoint = _beastSpawn.transform.position;
                Quaternion spawnRotation = _beastSpawn.transform.rotation;
                stable.m_BeastObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", whichBeast), spawnPoint, spawnRotation);
                stable.m_BeastObject.GetComponent<BeastManager>().m_IsInStable = true;
                stable.m_BeastObject.GetComponent<BeastManager>().m_BeastStableController = stable;
                pv.RPC("InitializeBeastWithStable", RpcTarget.OthersBuffered, stable.GetComponent<Item>().id);
            }
            else
            {
                BeastSpawnPoint[] _spawns = FindObjectsOfType<BeastSpawnPoint>();
                foreach (BeastSpawnPoint spawn in _spawns)
                {
                    if (spawn.spawnName == LevelPrep.Instance.playerSpawnName)
                    {
                        spawnPoint = spawn.transform.position + spawn.transform.forward * 3;
                        beastSpawn = spawn;
                    }
                }

                Vector3 newSpawnPoint = ActorUtils.GetRandomValidSpawnPoint(5, spawnPoint);
                GameObject beastObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", whichBeast), newSpawnPoint, Quaternion.identity);
                if (beastSpawn)
                {
                    beastObj.GetComponent<StateController>().currentState = beastSpawn.startingState;
                }
            }
        }
    }

    private void FindSpawnerAndSetSpawnPoint()
    {
        string spawnName = LevelPrep.Instance.playerSpawnName;

        PlayerSpawnPoint[] spawns = FindObjectsOfType<PlayerSpawnPoint>();
        foreach (PlayerSpawnPoint spawn in spawns)
        {
            if (spawn.spawnName == spawnName)
            {
                spawnPoint = spawn.transform.position;
            }
        }
        if (spawnPoint == Vector3.zero)
        {
            PortalInteraction[] portals = GameObject.FindObjectsOfType<PortalInteraction>();
            foreach (PortalInteraction portal in portals)
            {
                if (portal.destinationLevel == spawnName)
                {
                    spawnPoint = portal.transform.position;
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
                stable.m_BeastObject = BeastManager.Instance.gameObject;
                BeastManager.Instance.m_BeastStableController = stable;
                BeastManager.Instance.m_IsInStable = true;
            }
        }
    }

    [PunRPC]
    public void Initialize(int _playerNum, int colorIndex, string _playerName)
    {
        if (playerNum == -1)
        {
            playerNum = _playerNum;
            playerColorIndex = colorIndex;
            playerName = _playerName;
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
