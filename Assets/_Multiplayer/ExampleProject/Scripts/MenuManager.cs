using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] Menu[] menus;
    [SerializeField] TMP_InputField playerNameInput;
    public List<String> worldNames;
    [SerializeField] Transform worldListContent;
    [SerializeField] GameObject worldListItemPrefab;
    [SerializeField] Toggle usePassword;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TMP_Text passwordErrorText;
    [SerializeField] TMP_InputField clientPasswordField;
    [SerializeField] TMP_InputField newWorldInput;


    void Awake()
    {
        Instance = this;
        GetWorldNames();
    }
    public void OpenMenu(string menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void UpdateRoomsList()
    {
        Launcher.Instance.UpdateRoomsWithLatest();
    }

    void GetWorldNames()
    {
        string settlementName = FindObjectOfType<LevelPrep>().settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/");
        Directory.CreateDirectory(saveDirectoryPath);
        string[] filePaths = Directory.GetDirectories(saveDirectoryPath);
        // Read file contents and add to levelData
        List<string> _worldNames = new();
        for (int i = 0; i < filePaths.Length; i++)
        {
            string[] splitPath = filePaths[i].Split("/");
            _worldNames.Add(splitPath[splitPath.Length - 1]);
        }
        worldNames = _worldNames;
    }

    public void DeleteWorld(string worldName)
    {
        worldNames.Remove(worldName);
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{worldName}/");
        string[] files = Directory.GetFiles(saveDirectoryPath);
        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
        Directory.Delete(saveDirectoryPath);
        PrintWorldNames();
    }

    public void CreateNewWorld()
    {
        worldNames.Add(newWorldInput.text);
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{newWorldInput.text}/");
        Directory.CreateDirectory(saveDirectoryPath);
        newWorldInput.text = "";
    }

    public void ClearWorldName()
    {
        newWorldInput.text = "";
    }

    public void PrintWorldNames()
    {

        foreach (Transform child in worldListContent)
        {
            Destroy(child.gameObject);
        }
        foreach (string worldName in worldNames)
        {
            Instantiate(worldListItemPrefab, worldListContent).GetComponent<WorldListItem>().SetUp(worldName);
        }
        worldListContent.GetComponentInParent<WorldSelectControl>().SelectFirstWorld();
    }

    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
    public void SetName()
    {
        LevelPrep.Instance.playerName = playerNameInput.text;
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public void ClearPlayerName()
    {
        playerNameInput.text = "";
    }

    public void SetPassword()
    {
        if (usePassword.isOn)
        {
            LevelPrep.Instance.roomPassword = passwordField.text;
        }
    }
    public void ClearPassword()
    {
        passwordField.text = "";
    }
    public void OnToggle()
    {
        passwordField.interactable = usePassword.isOn;
    }

    public void CheckPassword()
    {
        if (LevelPrep.Instance.passwordProtectedRoomInfo.CustomProperties.TryGetValue("Password", out object roomPassword))
        {
            if (!LevelPrep.Instance.passwordProtectedRoomInfo.IsOpen)
            {
                OpenMenu("world");

            }
            else if (clientPasswordField.text == (string)roomPassword)
            {
                PhotonNetwork.JoinRoom(LevelPrep.Instance.passwordProtectedRoomInfo.Name);
                OpenMenu("loading");
            }
            else
            {
                passwordErrorText.text = "Incorrect Password";
            }
            clientPasswordField.text = "";
        }

    }
    public void ClearClientPassword()
    {
        clientPasswordField.text = "";
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        // If running in the Unity Editor
        EditorApplication.isPlaying = false;
#else
        // If running in a build version
        Application.Quit();
#endif
    }
}
