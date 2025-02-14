using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using Newtonsoft.Json;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;
    public const string LevelDataKey = "levelData";
    public const string playerPosKey = "GroupCenterPosition";
    public string levelData;
    bool comingFromRoom = false;
    public List<RoomInfo> lastRoomList;
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        MenuManager.Instance.OpenMenu("loading");
        Initialize();
        // Check if there was a disconnection error
    }
    private void Update()
    {
        if (ErrorManager.Instance != null && !string.IsNullOrEmpty(ErrorManager.Instance.LastDisconnectError))
        {
            ShowError(ErrorManager.Instance.LastDisconnectError);
            ErrorManager.Instance.LastDisconnectError = ""; // Clear the error after displaying
        }
    }
    private void ShowError(string message)
    {
        errorText.text = message;
        MenuManager.Instance.OpenMenu("error"); // Ensure there is an "error" menu to show messages
    }
    public void Initialize()
    {
        if (LevelPrep.Instance.offline)
        {
            PhotonNetwork.OfflineMode = true; // Enable offline mode
            PhotonNetwork.CreateRoom("OfflineRoom"); // Create a local "offline" room
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(); // Otherwise connect as normal
        }
        if (ErrorManager.Instance == null)
        {
            GameObject errorManager = Resources.Load<GameObject>("Prefabs/ErrorManager");
            Instantiate(errorManager);
        }
    }
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.EnableCloseConnection = true;
        }
    }

    public override void OnJoinedLobby()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            if (comingFromRoom) MenuManager.Instance.OpenMenu("world");
            else MenuManager.Instance.OpenMenu("main");
            PhotonNetwork.NickName = LevelPrep.Instance.playerName;
        }
    }

    public void CreateRoom()
    {
        // if input field is empty, do not create room
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // Set the max number of players
        string pass = "";
        if (LevelPrep.Instance.roomPassword != null)
        {
            pass = LevelPrep.Instance.roomPassword;
        }
        PhotonNetwork.NickName = LevelPrep.Instance.playerName;
        roomOptions = SetLevelData(true, pass);
        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        roomNameInputField.text = "";
        MenuManager.Instance.OpenMenu("loading");
    }

    public void ClearRoomName()
    {
        roomNameInputField.text = "";
    }

    //For the master client, when creating a room, gather the level SaveData and add it to the room options so that it is available to new player that join the room. This should be all the save data. 
    //Todo we may need to change this due to the 300kb limit.
    public RoomOptions SetLevelData(bool createRoom, string password = "")
    {
        string settlementName = FindObjectOfType<LevelPrep>().settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string[] filePaths = Directory.GetFiles(saveDirectoryPath);
        // Read file contents and add to levelData
        List<string> levelDataList = new List<string>();
        foreach (string filePath in filePaths)
        {
            string fileContent = File.ReadAllText(filePath);
            levelDataList.Add(fileContent);
        }

        // Convert the list of strings to a single string
        string levelData = string.Join("|-|", levelDataList);
        if (createRoom)
        {
            RoomOptions roomOptions = new RoomOptions();
            if (password != "")
            {
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Password", LevelPrep.Instance.roomPassword }, { LevelDataKey, levelData }, { playerPosKey, new Vector3(0, 0, 0) } };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "Password" };
            }
            else
            {
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { LevelDataKey, levelData }, { playerPosKey, new Vector3(0, 0, 0) } };
            }
            return roomOptions;
        }
        else
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { LevelDataKey, levelData }, { playerPosKey, new Vector3(0, 0, 0) } });
            return null;
        }
    }

    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");
        Player[] players = PhotonNetwork.PlayerList;

        JoinGameInit();
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < players.Length; i++)
        {
            // Update UI
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    private void JoinGameInit()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(LevelDataKey, out object levelDataValue))
            {
                if (levelDataValue != null)
                {
                    levelData = (string)levelDataValue;
                    LevelManager.Instance.SaveProvidedLevelData(levelData);
                    // Set spawning information
                }
            }
        }
        if (!LevelPrep.Instance.overridePlayerSpawning)
        {
            LevelManager.Instance.CallSetPartySpawnCriteria();
        }
    }

    public void StartGame()
    {
        if (levelData == null || levelData == "")
        {
            JoinGameInit();
        }
        startGameButton.GetComponent<Button>().interactable = false;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + "GameProgress.json";
        string json;
        GameSaveData data;
        try
        {
            json = File.ReadAllText(filePath);
            data = JsonConvert.DeserializeObject<GameSaveData>(json);
        }
        catch
        {
            data = new GameSaveData(0, 0);
        }
        if (!LevelPrep.Instance.overridePlayerSpawning)
        {
            LevelManager.Instance.worldProgress = data.gameProgress;
            LevelManager.Instance.beastLevel = data.beastLevel;
            LevelManager.Instance.CallSetPartySpawnCriteria();
        }

        PhotonNetwork.LoadLevel(LevelPrep.Instance.currentLevel);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    // Modify the LeaveRoom() method in the Launcher class
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        comingFromRoom = true;
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.CustomProperties.TryGetValue("Password", out object roomPassword))
        {
            if ((string)roomPassword != null)
            {
                MenuManager.Instance.OpenMenu("password");
                LevelPrep.Instance.passwordProtectedRoomInfo = roomInfo;
                return;
            }

        }

        PhotonNetwork.JoinRoom(roomInfo.Name);
        MenuManager.Instance.OpenMenu("loading");

    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("loading");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        lastRoomList = roomList;
        UpdateRoomsList(roomList);
    }

    public void UpdateRoomsWithLatest()
    {
        UpdateRoomsList(lastRoomList);
    }

    private void UpdateRoomsList(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                continue;
            }
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LeaveRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }




    public void QuitGame()
    {
        Application.Quit();
    }
}

