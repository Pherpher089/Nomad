
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class LoadNextScene : MonoBehaviourPunCallbacks
{
    bool hasLoaded = false;
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "IsReady", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            playerProperties["GroupCenterPosition"] = Vector3.zero;
            PhotonNetwork.CurrentRoom.SetCustomProperties(playerProperties);
        }
        // Immediately check if all players are ready after setting the property
        CheckAllPlayersReady();
    }

    public void OnSceneUnloaded(Scene current)
    {
        PhotonNetwork.LocalPlayer.CustomProperties.Remove("IsReady");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("IsReady"))
        {
            CheckAllPlayersReady();
        }
    }

    void CheckAllPlayersReady()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isReady;
            if (!p.CustomProperties.TryGetValue("IsReady", out isReady) || !(bool)isReady)
            {
                return; // If any player is not ready, exit the check
            }
        }
        // If all players are ready, load the main game scene

        StartCoroutine(WaitAndLoadLevel());
    }

    IEnumerator WaitAndLoadLevel()
    {
        yield return new WaitForSeconds(3);
        LoadLevel();
    }

    void LoadLevel()
    {
        if (PhotonNetwork.IsMasterClient && !hasLoaded)
        {
            hasLoaded = true;
            PhotonNetwork.LoadLevel(LevelPrep.Instance.currentLevel);
        }
    }
}

