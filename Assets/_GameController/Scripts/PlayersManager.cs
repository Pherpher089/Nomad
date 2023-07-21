using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    public static PlayersManager Instance;
    public Vector3 playersCentralPosition;
    public List<ThirdPersonUserControl> playerList = new List<ThirdPersonUserControl>();
    public void Awake()
    {
        Instance = this;
    }
    public void UpdatePlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        playerList = new List<ThirdPersonUserControl>();
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
                            stats.Initialize(player.playerName);
                        }
                    }
                }
            }
        }
    }
    public void DeathUpdate(ThirdPersonUserControl player)
    {
        playerList.Remove(player);
        if (playerList.Count == 0)
        {
            FindObjectOfType<HUDControl>().EnableFailScreen(true);
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
