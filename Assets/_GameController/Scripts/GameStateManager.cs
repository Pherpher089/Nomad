﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { PlayState, PauseState, WinState, FailState }
public enum TimeState { Day, Night }
public enum TimeCycle { Dawn, Morning, Noon, Afternoon, Dusk, Evening, Midnight, Latenight }

public class GameStateManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public float inventoryControlDeadZone = 0.005f;
    public float timeModifier = 1;
    public static GameStateManager Instance;
    public GameState gameState;
    public TimeState timeState;
    public HUDControl hudControl;
    public PlayersManager playersManager;
    public float cycleSpeed;
    [Range(0, 360)] public float timeCounter = 90;
    public bool isRaid;
    public Transform raidTarget;
    public bool isRaidComplete = false;
    public GameObject sun;
    public GameObject moon;
    public bool peaceful;
    public bool friendlyFire;
    public bool showOnScreenControls;
    public Vector3 currentRespawnPoint = Vector3.zero;
    public bool initialized = false;
    public Vector3 spawnPoint = Vector3.zero;
    public float raidCounter = 0;
    public List<InfoRuneController> activeInfoPrompts;
    [HideInInspector] public bool isTeleporting = false;
    public int readyPlayers = 0;
    public TentManager currentTent;
    public BossManager[] bosses;
    public bool masterIsQuitting = false;
    public bool enableBuildSnapping = false;
    public int numberOfBuilders = 0;
    public List<ObjectBuildController> activeBuildPieces = new();
    public float globalSnappingPointRadius = .5f;
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        activeInfoPrompts = new List<InfoRuneController>();
        Instance = this;
        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        moon.GetComponent<Light>().intensity = 0;
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        SettingsConfig levelPrep;
        if (LevelPrep.Instance != null)
        {
            levelPrep = LevelPrep.Instance.settingsConfig;
            friendlyFire = levelPrep.friendlyFire;
            peaceful = levelPrep.peaceful;
        }
        isTeleporting = false;
    }
    private void Start()
    {
        FindBosses();
    }

    void FindBosses()
    {
        bosses = FindObjectsOfType<BossManager>();

    }

    public void SetLoadingScreenOn()
    {
        hudControl.loadingScreen.SetActive(true);
    }

    public void InitializeGameState()
    {
        playersManager.UpdatePlayers(true);
        hudControl.Initialize();
        initialized = true;
        hudControl.loadingScreen.SetActive(false);
    }

    public void CallSetTimeRPC(float time = 90)
    {
        photonView.RPC("SetTimeRPC", RpcTarget.All, time);
    }

    public void StartRaid(Transform target, float time = 180)
    {
        int id = target.GetComponent<PhotonView>().ViewID;
        photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, true, id, time);
    }

    public void EndRaid()
    {
        if (isRaid)
        {
            photonView.RPC("SetRaidOver", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void SetTimeRPC(float time = 90)
    {
        SetTime(time);
    }

    [PunRPC]
    public void SetRaidOver()
    {
        isRaidComplete = true;
        isRaid = false;
        MainPortalManager.Instance.SetFragments();
        raidTarget = null;
        hudControl.raidCounterCanvasObject.SetActive(false);
    }

    [PunRPC]
    public void SetIsRaid(bool isRaidValue, int id, float _raidCounter)
    {
        isRaid = isRaidValue;
        raidTarget = PhotonView.Find(id).gameObject.transform;
        hudControl.raidCounterCanvasObject.SetActive(isRaidValue);
        raidCounter = _raidCounter;
    }

    public void SetTime(float time = 90)
    {
        timeCounter = time;
        sun.transform.rotation = Quaternion.Euler(time, 0, 0);
    }

    private void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.PlayState:
                // Handle PlayState logic
                break;
            case GameState.PauseState:
                // Handle PauseState logic
                break;
            case GameState.FailState:
                // Handle FailState logic
                break;
            default:
                break;
        }
    }

    public void CallChangeLevelRPC(string LevelName, string spawnName)
    {
        photonView.RPC("UpdateLevelInfo_RPC", RpcTarget.All, LevelName, spawnName);
    }

    [PunRPC]
    public void UpdateLevelInfo_RPC(string LevelName, string spawnName)
    {
        SetLoadingScreenOn();
        if (isTeleporting) return;
        isTeleporting = true;
        LevelManager.Instance.SaveLevel();
        LevelPrep.Instance.playerSpawnName = spawnName;
        LevelPrep.Instance.currentLevel = LevelName;
        CleanupPlayerInstances();
        photonView.RPC("ReadyToChangeScene", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void ReadyToChangeScene()
    {
        readyPlayers++;
    }

    private void CheckForSceneChange()
    {
        if (readyPlayers == PhotonNetwork.PlayerList.Length && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel("LoadingScene");
            readyPlayers = 0;
        }
    }

    private void CleanupPlayerInstances()
    {
        // Clean up PlayerManager instances
        PlayerManager[] existingPlayers = FindObjectsOfType<PlayerManager>();
        foreach (PlayerManager existingPlayer in existingPlayers)
        {
            if (existingPlayer != null && existingPlayer.GetComponent<PhotonView>().IsMine)
            {
                PhotonNetwork.Destroy(existingPlayer.gameObject);
            }
        }

        // Clean up instantiated player objects
        GameObject[] instantiatedPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in instantiatedPlayers)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                PhotonNetwork.Destroy(player);
            }
        }

        // Clean up the Beast object, if it exists
        BeastManager beastManager = FindObjectOfType<BeastManager>();
        if (beastManager != null && beastManager.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(beastManager.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log($"Game State Manager - On Application Quit - IsMaster:{PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.IsMasterClient)
        {
            if (LevelManager.Instance != null) LevelManager.Instance.SaveLevel();
            Debug.Log("Master client has left. Ending game for all clients.");
            photonView.RPC("OnQuitRpc", RpcTarget.All);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Adjust Player List
        //Maybe where we do color adjustments?
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master client has left. Ending game for all clients.");
        photonView.RPC("OnQuitRpc", RpcTarget.All);
    }

    [PunRPC]
    public void OnQuitRpc()
    {
        SetLoadingScreenOn();
        OnQuit();
    }
    public void OnQuit()
    {
        if (PhotonNetwork.IsMasterClient && LevelManager.Instance != null) PhotonNetwork.Destroy(LevelManager.Instance.gameObject);
        if (PhotonNetwork.IsMasterClient && RoomManager.Instance != null) PhotonNetwork.Destroy(RoomManager.Instance.gameObject);

        PhotonNetwork.LeaveRoom();
        StartCoroutine(WaitForDisconnectionAndLoadMainMenu());
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause != DisconnectCause.None)
        {
            ErrorManager.Instance.LastDisconnectError = ErrorManager.Instance.GetDisconnectMessage(cause);
            Debug.LogError(ErrorManager.Instance.LastDisconnectError);
        }
    }


    private IEnumerator WaitForDisconnectionAndLoadMainMenu()
    {
        if (LevelManager.Instance != null) Destroy(LevelManager.Instance.gameObject);
        if (RoomManager.Instance != null) Destroy(RoomManager.Instance.gameObject);
        while (PhotonNetwork.IsConnected || PhotonNetwork.InRoom || LevelManager.Instance != null || RoomManager.Instance != null)
        {
            yield return null;
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;

        DayNightCycle();
        GameStateMachine();
        CheckForBoss();
        if (PhotonNetwork.IsMasterClient) CheckForSceneChange();

        if (isRaid)
        {
            HandleRaid();
        }
    }

    private void HandleRaid()
    {
        if (raidCounter > 0)
        {
            raidCounter -= Time.deltaTime;
            int minutes = (int)raidCounter / 60;
            int seconds = (int)raidCounter % 60;
            string spacer = seconds < 10 ? "0" : "";
            hudControl.raidCounter.text = $"{minutes}:{spacer}{seconds}";
        }
        else
        {
            EndRaid();
        }
    }

    public void SwitchPeacefulSetting()
    {
        peaceful = !peaceful;
        LevelPrep.Instance.settingsConfig.peaceful = peaceful;
    }

    public void SwitchFriendlyFireSetting()
    {
        friendlyFire = !friendlyFire;
        LevelPrep.Instance.settingsConfig.friendlyFire = friendlyFire;
    }

    public void UpdateSettingsValues()
    {
        Transform pauseScreen = GameObject.Find("Canvas_PauseScreen").transform.GetChild(0);
        if (pauseScreen.gameObject.activeSelf)
        {
            UpdateToggle("GamePadToggle", LevelPrep.Instance.settingsConfig.firstPlayerGamePad);
        }
    }

    private void UpdateToggle(string toggleName, bool value, bool enabled = true)
    {
        var toggle = GameObject.Find(toggleName).GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(value);
        toggle.enabled = enabled;
    }

    private void CheckForBoss()
    {
        foreach (BossManager boss in bosses)
        {
            if (boss == null)
            {
                FindBosses();
                return;
            }
            if (Vector3.Distance(playersManager.playersCentralPosition, boss.transform.position) < 100)
            {
                hudControl.InitializeBossHealthBar(boss);
                return;
            }
            else
            {
                hudControl.TurnOfBossHealth();
            }
        }
    }


    private void DayNightCycle()
    {
        sun.transform.Rotate(Vector3.right * cycleSpeed * (Time.deltaTime / 2));
        timeCounter += cycleSpeed * (Time.deltaTime / 2);

        if (timeCounter < 180)
        {
            HandleDayCycle();
        }
        else
        {
            HandleNightCycle();
        }

        if (timeCounter >= 359)
        {
            timeCounter = 0;
        }
    }

    private void HandleDayCycle()
    {
        if (timeCounter > 150)
        {
            float t = Mathf.InverseLerp(150, 175, timeCounter);
            sun.GetComponent<Light>().intensity = Mathf.Lerp(1f, 0f, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(1f, .5f, t);
        }
        if (timeCounter < 30)
        {
            // moon.SetActive(false);
            float t = Mathf.InverseLerp(0, 30, timeCounter);
            sun.GetComponent<Light>().intensity = Mathf.Lerp(0f, 1f, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(.5f, 1f, t);
        }
        cycleSpeed = 1f * timeModifier;
        timeState = TimeState.Day;
    }

    private void HandleNightCycle()
    {
        sun.GetComponent<Light>().intensity = 0;
        cycleSpeed = 3f * timeModifier;
        timeState = TimeState.Night;
    }

    public void CloseInfoPrompts()
    {
        foreach (InfoRuneController im in activeInfoPrompts)
        {
            im.ShowInfo(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(timeCounter);
        }
        else
        {
            timeCounter = (float)stream.ReceiveNext();
        }
    }
}
