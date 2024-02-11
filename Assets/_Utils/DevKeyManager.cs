using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DevKeyManager : MonoBehaviour
{
    private string persistentPath;
    private string charactersPath;
    private string levelsPath;

    private void Start()
    {
        persistentPath = Application.persistentDataPath;
        charactersPath = Path.Combine(persistentPath, "characters");
        levelsPath = Path.Combine(persistentPath, "levels");
    }

    private void Update()
    {
        // Dev key for deleting character stats files
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DeleteCharacterStatsFiles(charactersPath);
        }

        // Dev key for deleting character inventory files
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DeleteCharacterInventoryFiles(charactersPath);
        }

        // Dev key for deleting level directories
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DeleteLevelDirectories(levelsPath);
        }
        // Dev key for deleting level directories
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ResetPlayerStats();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            KillPlayers();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            RevivePlayer();
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            GameStateManager.Instance.CallSetTimeRPC(0);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Break();
        }
    }
    private void KillPlayers()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            return;
        }

        HealthManager[] hlthMans = FindObjectsOfType<HealthManager>();
        foreach (var item in hlthMans)
        {
            if (item.tag == "Player")
            {
                item.TakeHit(item.health);
            }
        }
    }
    private void ResetPlayerStats()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            return;
        }
        HungerManager[] hms = FindObjectsOfType<HungerManager>();
        foreach (var item in hms)
        {
            item.m_StomachValue = item.m_StomachCapacity;
        }
        HealthManager[] hlthMans = FindObjectsOfType<HealthManager>();
        foreach (var item in hlthMans)
        {
            if (item.tag == "Player")
            {
                item.health = item.maxHealth;
            }
        }
    }

    private void DeleteCharacterStatsFiles(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] statsFiles = Directory.GetFiles(dirPath, "*-stats.*");
            foreach (string file in statsFiles)
            {
                DeleteFile(file);
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteCharacterInventoryFiles(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] allFiles = Directory.GetFiles(dirPath);
            foreach (string file in allFiles)
            {
                if (!file.EndsWith("-stats"))
                {
                    DeleteFile(file);
                }
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteLevelDirectories(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] directories = Directory.GetDirectories(dirPath);
            foreach (string directory in directories)
            {
                DeleteDirectory(directory);
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Debug.Log("Deleted file: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting file: " + filePath + ". Error: " + e.Message);
        }
    }

    private void DeleteDirectory(string dirPath)
    {
        try
        {
            Directory.Delete(dirPath, true);  // The 'true' parameter allows for recursive deletion
            Debug.Log("Deleted directory: " + dirPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting directory: " + dirPath + ". Error: " + e.Message);
        }
    }

    private void RevivePlayer()
    {
        ThirdPersonUserControl[] players = GameObject.FindObjectsOfType<ThirdPersonUserControl>();
        foreach (ThirdPersonUserControl player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                PlayersManager.Instance.RespawnDeadPlayer(player.transform.position, player.GetComponent<PhotonView>().ViewID);
            }
        }
    }
}
