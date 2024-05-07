
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
public class LoadNextScene : MonoBehaviourPunCallbacks
{
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
        Debug.Log("### Scene Loaded: " + scene.name);
        Hashtable props = new Hashtable { { "IsReady", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // Immediately check if all players are ready after setting the property
        CheckAllPlayersReady();
    }

    public void OnSceneUnloaded(Scene current)
    {
        Debug.Log("### Scene Unloaded: " + current.name);
        PhotonNetwork.LocalPlayer.CustomProperties.Remove("IsReady");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("### Player Properties Updated");
        if (changedProps.ContainsKey("IsReady"))
        {
            Debug.Log("### IsReady Property Changed for Player: " + targetPlayer.NickName);
            CheckAllPlayersReady();
        }
    }

    void CheckAllPlayersReady()
    {
        Debug.Log("### Checking if all players are ready...");
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            Debug.Log("### Only one player in the session. Proceeding to load the level.");
            LoadLevel();
            return;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isReady;
            if (!p.CustomProperties.TryGetValue("IsReady", out isReady) || !(bool)isReady)
            {
                Debug.Log("### Player " + p.NickName + " is not ready.");
                return; // If any player is not ready, exit the check
            }
        }
        // If all players are ready, load the main game scene
        LoadLevel();
    }

    void LoadLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("### All players are ready. Loading level: " + LevelPrep.Instance.currentLevel);
            PhotonNetwork.LoadLevel(LevelPrep.Instance.currentLevel);
        }
    }


}
