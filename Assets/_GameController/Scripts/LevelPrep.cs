using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using Photon.Realtime;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

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
    public bool isFirstLoad; //Tells whether the portals should be on or off based on whether this is the first load or not
    public SettingsConfig settingsConfig;
    [Header("Dev Player Spawning")]
    public bool overridePlayerSpawning = false;
    public string playerSpawnName;
    public string currentLevel;
    public string earliestCompatibleVersion;
    [HideInInspector] public string playerName;
    [HideInInspector] public string roomPassword;
    [HideInInspector] public RoomInfo passwordProtectedRoomInfo;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DeleteOldSaveData();
        string versionFilePath = Application.persistentDataPath + "/RealmwalkerVersion.json";
        File.WriteAllText(versionFilePath, JsonConvert.SerializeObject(new LastVersion(Application.version)));

        isFirstLoad = true;
        Instance = this;
        firstPlayerGamePad = settingsConfig.firstPlayerGamePad;
        DontDestroyOnLoad(this);
    }

    void DeleteOldSaveData()
    {
        string filePath = Application.persistentDataPath + "/RealmwalkerVersion.json";
        string json;
        LastVersion data;
        try
        {
            json = File.ReadAllText(filePath);
            data = JsonConvert.DeserializeObject<LastVersion>(json);
            if (data != null && IsVersionOlder(data.version, earliestCompatibleVersion))
            {
                DeleteSaveData();
            }
        }
        catch
        {
            Debug.Log("~ No previous saved version found, deleting save data.");
            DeleteSaveData();
        }
    }

    bool IsVersionOlder(string lastVersion, string earliestCompatibleVersion)
    {
        return CompareVersions(lastVersion, earliestCompatibleVersion) < 0;
    }

    int CompareVersions(string version1, string version2)
    {
        // Split versions into parts by '.' or other delimiters
        string[] v1Parts = version1.Split('.');
        string[] v2Parts = version2.Split('.');

        int length = Mathf.Max(v1Parts.Length, v2Parts.Length);
        for (int i = 0; i < length; i++)
        {
            string v1Part = i < v1Parts.Length ? v1Parts[i] : "0";
            string v2Part = i < v2Parts.Length ? v2Parts[i] : "0";

            // Compare numeric parts
            int v1Numeric = int.Parse(new string(v1Part.TakeWhile(char.IsDigit).ToArray()));
            int v2Numeric = int.Parse(new string(v2Part.TakeWhile(char.IsDigit).ToArray()));

            if (v1Numeric != v2Numeric)
            {
                return v1Numeric.CompareTo(v2Numeric);
            }

            // Compare non-numeric parts (e.g., 'a', 'b')
            string v1Suffix = new string(v1Part.SkipWhile(char.IsDigit).ToArray());
            string v2Suffix = new string(v2Part.SkipWhile(char.IsDigit).ToArray());

            int suffixComparison = string.Compare(v1Suffix, v2Suffix);
            if (suffixComparison != 0)
            {
                return suffixComparison;
            }
        }

        return 0; // Versions are equal
    }

    void DeleteSaveData()
    {
        string levelsPath = Application.persistentDataPath + "/Levels/";
        string charactersPath = Application.persistentDataPath + "/Characters/";

        if (Directory.Exists(levelsPath))
        {
            Directory.Delete(levelsPath, true);
            Debug.Log("~ Deleted old save data from Levels folder.");
        }

        if (Directory.Exists(charactersPath))
        {
            Directory.Delete(charactersPath, true);
            Debug.Log("~ Deleted old save data from Characters folder.");
        }
    }

    public void ResetLevelPrep()
    {
        isFirstLoad = true;
        firstPlayerGamePad = settingsConfig.firstPlayerGamePad;
        DontDestroyOnLoad(this);
    }
}

public class LastVersion
{
    public string version;
    public LastVersion(string version)
    {
        this.version = version;
    }
}
