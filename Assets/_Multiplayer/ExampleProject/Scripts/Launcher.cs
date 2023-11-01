using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;
using System.IO;
using UnityEngine.SceneManagement;

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
    public string levelData;

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        MenuManager.Instance.OpenMenu("loading");
        Initialize();
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
    }
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;
        }
    }

    public override void OnJoinedLobby()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            MenuManager.Instance.OpenMenu("main");
            PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
        }
    }

    public void CreateRoom()
    {
        // if input field is empty, do not create room
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        RoomOptions ro = SetLevelData(true);
        PhotonNetwork.CreateRoom(roomNameInputField.text, ro);
        MenuManager.Instance.OpenMenu("loading");

    }

    //For the master client, when creating a room, gather the level SaveData and add it to the room options so that it is available to new player that join the room. This should be all the save data. 
    //Todo we may need to change this due to the 300kb limit.
    public RoomOptions SetLevelData(bool createRoom)
    {
        string settlementName = FindObjectOfType<LevelPrep>().settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{settlementName}/");

        string targetFilePath = Path.Combine(saveDirectoryPath, LevelPrep.Instance.currentLevel);

        if (!File.Exists(targetFilePath))
        {
            // Handle case where file doesn't exist.
            // For example, log an error, return null, or throw an exception
            Debug.LogError($"Target file {LevelPrep.Instance.currentLevel} does not exist in directory {saveDirectoryPath}.");
            return null;
        }

        string fileContent = File.ReadAllText(targetFilePath);

        if (createRoom)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { LevelDataKey, fileContent } };
            return roomOptions;
        }
        else
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { LevelDataKey, fileContent } });
            return null;
        }
    }


    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");
        Player[] players = PhotonNetwork.PlayerList;
        if (!PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().buildIndex == 0)
        {
            SetLevelDataFromRoomOptions();
        }
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);

    }

    public void SetLevelDataFromRoomOptions()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(LevelDataKey, out object levelDataValue))
        {
            if (levelDataValue != null)
            {
                Debug.Log("Level Data: " + levelDataValue);
                levelData = (string)levelDataValue;
                LevelManager.Instance.SaveProvidedLevelData(levelData);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            // The current player is the new master client, so everyone needs to leave the room.
            PhotonNetwork.LeaveRoom();
        }
    }

    public void StartGame()
    {
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
        MenuManager.Instance.OpenMenu("loading");
    }


    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("online");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
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
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
