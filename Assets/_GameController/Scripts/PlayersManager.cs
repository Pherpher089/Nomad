using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayersManager : MonoBehaviour
{
    public static PlayersManager Instance;
    public Vector3 playersCentralPosition;
    public List<ThirdPersonUserControl> playerList = new();

    public List<ThirdPersonUserControl> localPlayerList = new();
    public List<ThirdPersonUserControl> deadPlayers = new();
    public bool initialized = false;
    public int totalPlayers = 0;
    public int totalDeadPlayers = 0;
    float updateInterval = 2.0f;
    float timeSinceLastUpdate = 0f;
    PhotonView pv;
    public const string playerPosKey = "GroupCenterPosition";

    public void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateGroupCenterPosition();
            timeSinceLastUpdate = 0f;
        }
    }
    void UpdateGroupCenterPosition()
    {
        if (GameStateManager.Instance.masterIsQuitting) return;
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["GroupCenterPosition"] = GetCenterPoint();
        PhotonNetwork.CurrentRoom.SetCustomProperties(playerProperties);
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(playerPosKey, out object groupCenterObj);
        Vector3 groupCenter = (Vector3)groupCenterObj;
    }
    public void CheckForDeath()
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players != null && players.Length <= 0)
        {
            StartCoroutine(WaitAndRespanwParty());
        }
    }

    public GameObject[] GetPlayerObjects()
    {
        if (GameStateManager.Instance.isTeleporting) return null;
        GameObject[] playerObjects = new GameObject[playerList.Count];
        int i = 0;
        List<ThirdPersonUserControl> newList = playerList;
        foreach (ThirdPersonUserControl player in playerList)
        {
            if (player == null || player.gameObject == null)
            {
                newList.Remove(player);
            }
        }

        playerList = newList;

        foreach (ThirdPersonUserControl player in playerList)
        {
            if (player == null || player.gameObject == null)
            {
                playerList.Remove(player);
            }
            else
            {
                playerObjects[i] = player.gameObject;
                i++;
            }

        }
        return playerObjects;
    }

    public void UpdatePlayers(bool isGameState = false)
    {
        pv.RPC("RPC_UpdatePlayers", RpcTarget.All, isGameState);
    }
    [PunRPC]
    public void RPC_UpdatePlayers(bool isGameState = false)
    {
        if (!isGameState && !GameStateManager.Instance.initialized) return;
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        playerList = new List<ThirdPersonUserControl>();
        localPlayerList = new List<ThirdPersonUserControl>();
        deadPlayers = new List<ThirdPersonUserControl>();


        foreach (GameObject playerObject in playerObjects)
        {
            ThirdPersonUserControl player = playerObject.GetComponent<ThirdPersonUserControl>();
            playerList.Add(player);
            if (player.GetComponent<PhotonView>().IsMine)
            {
                localPlayerList.Add(player);
                CharacterStats stats = playerObject.GetComponent<CharacterStats>();

                if (!player.initialized)
                {

                    stats.Initialize(player.characterName);
                }
            }
            player.initialized = true;
        }

        if (LevelPrep.Instance.firstPlayerGamePad)
        {
            ChangePlayerOneInput(LevelPrep.Instance.firstPlayerGamePad);
        }
        initialized = true;
    }
    public void DeathUpdate(ThirdPersonUserControl player)
    {
        playerList.Remove(player);
        deadPlayers.Add(player);
        //RPC that takes a PV and updates its tag on all clients
    }

    IEnumerator WaitAndRespanwParty()
    {
        yield return new WaitForSeconds(3);
        RespawnParty();
    }
    public void RespawnParty()
    {
        GetComponent<PhotonView>().RPC("RespawnParty_RPC", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void RespawnParty_RPC()
    {
        if (!LevelPrep.Instance.overridePlayerSpawning)
        {
            if (LevelManager.Instance.worldProgress == 0)
            {
                LevelPrep.Instance.currentLevel = "TutorialWorld";
                LevelPrep.Instance.playerSpawnName = "start";
            }
            else
            {
                string sceneName = SceneManager.GetActiveScene().name;
                if (sceneName != "HubWorld" && GameStateManager.Instance.currentTent != null)
                {
                    LevelPrep.Instance.currentLevel = sceneName;
                    LevelPrep.Instance.playerSpawnName = "tent";
                }
                else
                {
                    LevelPrep.Instance.currentLevel = "HubWorld";
                    LevelPrep.Instance.playerSpawnName = "";
                }
            }
        }
        LevelManager.Instance.SaveLevel();
        LevelPrep.Instance.isFirstLoad = false;
        GameStateManager.Instance.CallChangeLevelRPC(LevelPrep.Instance.currentLevel, LevelPrep.Instance.playerSpawnName);
        PlayerSpawnPoint[] spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
        Vector3 spawnPoint = Vector3.zero;
        foreach (PlayerSpawnPoint spawn in spawnPoints)
        {
            if (spawn.name == LevelPrep.Instance.playerSpawnName)
            {
                spawnPoint = spawn.transform.position;
            }
        }
        Instance.RespawnDeadPlayers(spawnPoint);
    }
    public void RespawnDeadPlayers(Vector3 spawnPoint)
    {
        GetComponent<PhotonView>().RPC("RespawnDeadPlayers_RPC", RpcTarget.All, spawnPoint);
    }
    [PunRPC]
    public void RespawnDeadPlayers_RPC(Vector3 spawnPoint)
    {
        int c = 0;
        foreach (ThirdPersonUserControl player in deadPlayers)
        {
            player.transform.position = spawnPoint + new Vector3(c, 0, c);
            c++;
            player.GetComponent<ActorManager>().Revive();
        }
        foreach (ThirdPersonUserControl player in deadPlayers)
        {
            playerList.Add(player);
        }
        deadPlayers = new List<ThirdPersonUserControl>();
    }

    public void RespawnDeadPlayer(Vector3 spawnPoint, int photonViewId)
    {
        GetComponent<PhotonView>().RPC("RespawnDeadPlayer_RPC", RpcTarget.All, spawnPoint, photonViewId);
    }
    [PunRPC]
    public void RespawnDeadPlayer_RPC(Vector3 spawnPoint, int photonViewId)
    {
        foreach (ThirdPersonUserControl player in deadPlayers)
        {
            if (player.GetComponent<PhotonView>().ViewID == photonViewId)
            {
                player.transform.position = spawnPoint;
                player.GetComponent<ActorManager>().Revive();
                playerList.Add(player);
                deadPlayers.Remove(player);
            }
        }
    }
    public Vector3 GetCenterPoint()
    {
        Vector3 centerPoint = Vector3.zero;
        foreach (ThirdPersonUserControl player in playerList)
        {
            centerPoint += player.transform.position;
        }
        centerPoint /= playerList.Count;
        return centerPoint;
    }

    public float GetDistanceToClosestPlayer(Transform fromPosition)
    {
        if (GameStateManager.Instance.isTeleporting) return -1f;
        float shortestDistance = 10000000;
        try
        {
            foreach (GameObject player in GetPlayerObjects())
            {
                float dist = Vector3.Distance(fromPosition.position, player.transform.position);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                }
            }
        }
        catch
        {
            Debug.LogWarning("Players list was modified");
            shortestDistance = 0;
        }

        return shortestDistance;
    }

    public void ChangePlayerOneInput()
    {
        ChangePlayerOneInput(!LevelPrep.Instance.firstPlayerGamePad);
        LevelPrep.Instance.firstPlayerGamePad = !LevelPrep.Instance.firstPlayerGamePad;
        LevelPrep.Instance.settingsConfig.firstPlayerGamePad = LevelPrep.Instance.firstPlayerGamePad;
    }
    public void ChangePlayerOneInput(bool gamePad)
    {
        foreach (ThirdPersonUserControl player in playerList)
        {
            if (gamePad)
            {
                if (player.playerNum == PlayerNumber.Single_Player)
                {
                    player.playerNum = PlayerNumber.Player_1;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_1)
                {
                    player.playerNum = PlayerNumber.Player_2;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_2)
                {
                    player.playerNum = PlayerNumber.Player_3;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_3)
                {
                    player.playerNum = PlayerNumber.Player_4;
                    player.SetPlayerPrefix(player.playerNum);
                }
            }
            else
            {
                if (player.playerNum == PlayerNumber.Player_1)
                {
                    player.playerNum = PlayerNumber.Single_Player;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_2)
                {
                    player.playerNum = PlayerNumber.Player_1;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_3)
                {
                    player.playerNum = PlayerNumber.Player_2;
                    player.SetPlayerPrefix(player.playerNum);
                }
                else if (player.playerNum == PlayerNumber.Player_4)
                {
                    player.playerNum = PlayerNumber.Player_3;
                    player.SetPlayerPrefix(player.playerNum);
                }

            }
        }
    }
    public float GetPlayersMaxDistance()
    {
        // Calculate the center point of all the players
        Vector3 centerPoint = Vector3.zero;
        foreach (ThirdPersonUserControl player in playerList)
        {
            centerPoint += player.transform.position;
        }
        centerPoint /= playerList.Count;
        // Calculate the distance between all the players
        float maxDistance = 0f;
        foreach (ThirdPersonUserControl player in playerList)
        {
            float distance = Vector3.Distance(centerPoint, player.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        return maxDistance;
    }
}
