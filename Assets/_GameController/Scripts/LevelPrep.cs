using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using Photon.Realtime;

public class LevelPrep : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public static LevelPrep Instance;
    [Header("World Properties")]
    public string settlementName;
    [Header("Player Properties")]
    [Description("Set number of LOCAL players. Make sure there is a name for each player in the Player Names field")]
    public int numberOfPlayers;
    public string[] playerNames;
    public bool offline;
    [HideInInspector]
    public bool receivedLevelFiles;
    [HideInInspector]
    public bool firstPlayerGamePad;
    [HideInInspector]
    public bool isFirstLoad; //Tells weather the portals should be on or off based on weather this is the first load or not
    public SettingsConfig settingsConfig;
    [Header("Dev Player Spawning")]
    public bool overridePlayerSpawning = false;
    public string playerSpawnName;
    public string currentLevel;
    [HideInInspector] public string playerName;
    [HideInInspector] public string roomPassword;
    [HideInInspector] public RoomInfo passwordProtectedRoomInfo;

    void Awake()
    {
        isFirstLoad = true;
        Instance = this;
        firstPlayerGamePad = settingsConfig.firstPlayerGamePad;
        DontDestroyOnLoad(this);
    }
}
