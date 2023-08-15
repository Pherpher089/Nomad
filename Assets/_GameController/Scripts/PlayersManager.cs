using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayersManager : MonoBehaviour
{
    public static PlayersManager Instance;
    public Vector3 playersCentralPosition;
    public List<ThirdPersonUserControl> playerList = new List<ThirdPersonUserControl>();
    public List<ThirdPersonUserControl> deadPlayers = new List<ThirdPersonUserControl>();
    public bool initialized = false;
    public int totalPlayers = 0;
    public int totalDeadPlayers = 0;

    PhotonView pv;
    public void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
    }
    public void CheckForDeath()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players != null && players.Length <= 0)
        {
            Debug.Log("### starting respawn coroutine");
            StartCoroutine(WaitAndRespanwParty());
        }
    }
    public void UpdatePlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        playerList = new List<ThirdPersonUserControl>();
        deadPlayers = new List<ThirdPersonUserControl>();

        for (int i = 0; i < playerObjects.Length; i++)
        {
            foreach (GameObject playerObject in playerObjects)
            {
                CharacterStats stats = playerObject.GetComponent<CharacterStats>();
                ThirdPersonUserControl player = playerObject.GetComponent<ThirdPersonUserControl>();
                if (player.isActiveAndEnabled)
                {
                    if (player.playerPos == i)
                    {
                        playerList.Add(player);
                        if (!stats.isLoaded)
                        {
                            stats.Initialize(player.characterName);
                        }
                    }
                }
                player.initialized = true;
            }
        }
        initialized = true;
    }
    public void DeathUpdate(ThirdPersonUserControl player)
    {
        Debug.Log("### death update");
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
        GetComponent<PhotonView>().RPC("RespawnParty_RPC", RpcTarget.All);
    }
    [PunRPC]
    public void RespawnParty_RPC()
    {
        LevelManager.SaveLevel(GameStateManager.Instance.currentRespawnPoint);
        Instance.RespawnDeadPlayers(GameStateManager.Instance.currentRespawnPoint);
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

    public void ChangePlayerOneInput()
    {
        foreach (ThirdPersonUserControl player in playerList)
        {
            if (player.playerNum == PlayerNumber.Single_Player)
            {
                player.playerNum = PlayerNumber.Player_1;
                player.SetPlayerPrefix(player.playerNum);
                FindObjectOfType<GameStateManager>().firstPlayerKeyboardAndMouse = false;
            }
            else if (player.playerNum == PlayerNumber.Player_1)
            {
                player.playerNum = PlayerNumber.Single_Player;
                player.SetPlayerPrefix(player.playerNum);
                FindObjectOfType<GameStateManager>().firstPlayerKeyboardAndMouse = true;
            }
            player.GetComponent<PlayerInventoryManager>().UpdateButtonPrompts();
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
