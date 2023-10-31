using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LevelPrep : MonoBehaviourPunCallbacks
{
    public static LevelPrep Instance;
    public string settlementName;
    public string currentLevel;
    public int numberOfPlayers;
    public bool offline;
    public bool receivedLevelFiles;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

}
