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
    [HideInInspector]
    public static GameStateManager Instance;
    [HideInInspector]
    public GameState gameState;
    [HideInInspector]
    public TimeState timeState;
    [HideInInspector]
    public HUDControl hudControl;
    [HideInInspector]
    public PlayersManager playersManager;
    [HideInInspector]
    public float cycleSpeed;
    [HideInInspector]
    [Range(0, 360)] public float timeCounter = 90;
    [HideInInspector]
    public bool isRaid;
    [HideInInspector]
    public bool isRaidComplete = false;
    [HideInInspector]
    public GameObject sun;
    [HideInInspector]
    public bool peaceful;
    [HideInInspector]
    public bool friendlyFire;
    [HideInInspector]
    public bool showOnScreenControls;
    public Vector3 currentRespawnPoint = Vector3.zero;
    [HideInInspector]
    public bool initialized = false;
    [HideInInspector]
    public Vector3 spawnPoint = Vector3.zero;
    [HideInInspector]
    public float raidCounter = 0;
    [HideInInspector]
    public List<InfoRuneController> activeInfoPrompts;
    [HideInInspector]
    bool isTeleporting = false;
    [HideInInspector]
    public int readyPlayers = 0;
    [HideInInspector]
    public TentManager currentTent;

    public void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        activeInfoPrompts = new List<InfoRuneController>();
        Instance = this;
        sun = GameObject.Find("Sun");
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        showOnScreenControls = LevelPrep.Instance.settingsConfig.showOnScreenControls;
        friendlyFire = LevelPrep.Instance.settingsConfig.friendlyFire;
        peaceful = LevelPrep.Instance.settingsConfig.peaceful;
        isTeleporting = false;
    }

    public void setLoadingScreenOn()
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
    void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.PlayState:
                break;
            case GameState.PauseState:
                break;
            case GameState.FailState:
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

    void CheckForSceneChange()
    {
        if (readyPlayers == PhotonNetwork.PlayerList.Length && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel("LoadingScene");
            readyPlayers = 0;
        }
    }
    void OnApplicationQuit()
    {
        LevelManager.Instance.SaveLevel();
    }

    void Update()
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
            if (raidCounter > 0)
            {
                raidCounter -= Time.deltaTime;
                var minutes = (int)raidCounter / 60;
                var seconds = (int)raidCounter % 60;
                var spacer = seconds < 10 ? "0" : "";
                hudControl.raidCounter.text = $"{minutes}:{spacer}{seconds}";
            }
            else
            {
                EndRaid();
            }
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
        if (GameObject.Find("Canvas_PauseScreen").transform.GetChild(0).gameObject.activeSelf)
        {
            GameObject.Find("GamePadToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.firstPlayerGamePad);
            GameObject.Find("ShowControlsOnScreenToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.showOnScreenControls);
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject.Find("PeacefulToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.peaceful);
                GameObject.Find("FriendlyFireToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.friendlyFire);
            }
            else
            {
                GameObject.Find("PeacefulToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.peaceful);
                GameObject.Find("FriendlyFireToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(LevelPrep.Instance.settingsConfig.friendlyFire);
                GameObject.Find("PeacefulToggle").GetComponent<Toggle>().enabled = false;
                GameObject.Find("FriendlyFireToggle").GetComponent<Toggle>().enabled = false;
            }
        }
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
        float sunRotation = sun.transform.rotation.eulerAngles.x;
        timeCounter += cycleSpeed * (Time.deltaTime / 2);

        if (timeCounter < 180)
        {
            if (timeCounter > 160)
            {
                float t = Mathf.InverseLerp(150, 180, timeCounter);
                // Lerp from 1 to 0
                sun.GetComponent<Light>().intensity = Mathf.Lerp(1f, .1f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(1f, .5f, t);
            }
            cycleSpeed = 1f * timeModifier;
            timeState = TimeState.Day;
            if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name == "HubWorld" && isRaid)
            {
                //photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, false);
            }
        }
        else if (timeCounter > 180)
        {
            if (timeCounter > 330)
            {
                float t = Mathf.InverseLerp(330, 359, timeCounter);
                // Lerp from 0 to 1
                sun.GetComponent<Light>().intensity = Mathf.Lerp(.1f, 1f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(.5f, 1f, t);
            }
            cycleSpeed = 3f * timeModifier;
            if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name == "HubWorld" && !isRaid)
            {
                //photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, true);
            }
            timeState = TimeState.Night;
        }

        if (timeCounter >= 359)
        {
            timeCounter = 0;
        }

    }
    public void CloseInfoPrompts()
    {
        foreach (InfoRuneController im in activeInfoPrompts)
        {
            im.ShowInfo(this.gameObject);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(timeCounter);
        }
        else
        {
            // Network player, receive data
            this.timeCounter = (float)stream.ReceiveNext();
        }
    }
}
