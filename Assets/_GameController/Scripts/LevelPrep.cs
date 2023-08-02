using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LevelPrep : MonoBehaviourPunCallbacks
{
    public static LevelPrep Instance;
    public string worldName;
    public int numberOfPlayers;
    public bool offline;
    public bool receivedLevelFiles;
    public bool localMultiplayerTesting = false;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

}
