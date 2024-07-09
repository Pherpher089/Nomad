using System;
using System.Collections.Generic;
using Photon.Pun;
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
    public bool isRaidComplete = false;
    public GameObject sun;
    public bool peaceful;
    public bool friendlyFire;
    public bool showOnScreenControls;
    public Vector3 currentRespawnPoint = Vector3.zero;
    public bool initialized = false;
    public Vector3 spawnPoint = Vector3.zero;
    public float raidCounter = 0;
    public List<InfoRuneController> activeInfoPrompts;
    private bool isTeleporting = false;
    public int readyPlayers = 0;
    public TentManager currentTent;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        activeInfoPrompts = new List<InfoRuneController>();
        Instance = this;
        sun = GameObject.Find("Sun");
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        SettingsConfig levelPrep;
        if (LevelPrep.Instance != null)
        {
            levelPrep = LevelPrep.Instance.settingsConfig;
            showOnScreenControls = levelPrep.showOnScreenControls;
            friendlyFire = levelPrep.friendlyFire;
            peaceful = levelPrep.peaceful;
        }
        isTeleporting = false;
    }

    public void SetLoadingScreenOn()
    {
        hudControl.loadingScreen.SetActive(true);
    }

    public void InitializeGameState()
    {
        playersManager.UpdatePlayers();
        hudControl.Initialize();
        initialized = true;
        hudControl.loadingScreen.SetActive(false);
    }

    public void CallSetTimeRPC(float time = 90)
    {
        photonView.RPC("SetTimeRPC", RpcTarget.All, time);
    }

    public void StartRaid()
    {
        photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, true, 180f);
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
    }

    [PunRPC]
    public void SetIsRaid(bool isRaidValue, float _raidCounter)
    {
        isRaid = isRaidValue;
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
        if (isTeleporting) return;
        isTeleporting = true;
        LevelManager.Instance.SaveLevel();
        LevelPrep.Instance.playerSpawnName = spawnName;
        LevelPrep.Instance.currentLevel = LevelName;
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

    private void OnApplicationQuit()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.SaveLevel();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;

        DayNightCycle();
        GameStateMachine();
        CheckForBoss();
        if (PhotonNetwork.IsMasterClient) CheckForSceneChange();

        if (showOnScreenControls)
        {
            hudControl.UpdateOnScreenControls();
        }

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
            UpdateToggle("ShowControlsOnScreenToggle", LevelPrep.Instance.settingsConfig.showOnScreenControls);

            if (PhotonNetwork.IsMasterClient)
            {
                UpdateToggle("PeacefulToggle", LevelPrep.Instance.settingsConfig.peaceful);
                UpdateToggle("FriendlyFireToggle", LevelPrep.Instance.settingsConfig.friendlyFire);
            }
            else
            {
                UpdateToggle("PeacefulToggle", LevelPrep.Instance.settingsConfig.peaceful, false);
                UpdateToggle("FriendlyFireToggle", LevelPrep.Instance.settingsConfig.friendlyFire, false);
            }
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
        BossManager[] bosses = FindObjectsOfType<BossManager>();
        foreach (BossManager boss in bosses)
        {
            if (Vector3.Distance(playersManager.playersCentralPosition, boss.transform.position) < 100)
            {
                hudControl.InitializeBossHealthBar(boss);
                return;
            }
        }
    }

    public void ToggleOnScreenControls()
    {
        showOnScreenControls = !showOnScreenControls;
        hudControl.UpdateOnScreenControls();
        LevelPrep.Instance.settingsConfig.showOnScreenControls = showOnScreenControls;
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
        if (timeCounter > 160)
        {
            float t = Mathf.InverseLerp(150, 180, timeCounter);
            sun.GetComponent<Light>().intensity = Mathf.Lerp(1f, .1f, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(1f, .5f, t);
        }
        cycleSpeed = 1f * timeModifier;
        timeState = TimeState.Day;
    }

    private void HandleNightCycle()
    {
        if (timeCounter > 330)
        {
            float t = Mathf.InverseLerp(330, 359, timeCounter);
            sun.GetComponent<Light>().intensity = Mathf.Lerp(.1f, 1f, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(.5f, 1f, t);
        }
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
