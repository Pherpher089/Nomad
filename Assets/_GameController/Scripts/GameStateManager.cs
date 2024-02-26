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
    public static GameStateManager Instance;
    string m_WorldName = "Default";
    public bool newWorld = false;
    public GameState gameState;
    public TimeState timeState;

    public HUDControl hudControl;
    public PlayersManager playersManager;
    //Day-Night Cycle Control
    public float cycleSpeed = 1;
    [Range(0, 360)] public float timeCounter = 90;
    public TimeCycle timeCycle = TimeCycle.Dawn;
    public bool isRaid;
    public GameObject sun;
    [HideInInspector]
    public bool peaceful;
    [HideInInspector]
    public bool friendlyFire;
    [HideInInspector]
    public bool showOnScreenControls;
    public Material[] playerMats;
    public string[] players;
    public Vector3 currentRespawnPoint = Vector3.zero;
    public bool online;
    [HideInInspector]
    public bool initialized = false;
    public Vector3 spawnPoint = Vector3.zero;
    private float nextCheckTime = 0f;
    private float checkInterval = 2f; // Check every half a second
    public float raidCounter = 0;
    public List<InfoRuneController> activeInfoPrompts;
    public void Awake()
    {
        activeInfoPrompts = new List<InfoRuneController>();
        Instance = this;
        m_WorldName = LevelPrep.Instance.settlementName;
        sun = GameObject.Find("Sun");
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        showOnScreenControls = LevelPrep.Instance.settingsConfig.showOnScreenControls;
        friendlyFire = LevelPrep.Instance.settingsConfig.friendlyFire;
        peaceful = LevelPrep.Instance.settingsConfig.peaceful;
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
        raidCounter = 180;
        photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, true);
    }
    public void EndRaid()
    {
        raidCounter = 0;
        photonView.RPC("SetIsRaid", RpcTarget.AllBuffered, false);
        GameObject.FindGameObjectWithTag("MainPortal").GetComponent<MainPortalInteraction>().SetFragments();
    }
    [PunRPC]
    public void SetTimeRPC(float time = 90)
    {
        SetTime(time);
    }

    [PunRPC]
    public void SetIsRaid(bool isRaidValue)
    {
        isRaid = isRaidValue;
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

    void Update()
    {
        DayNightCycle();
        GameStateMachine();
        CheckForBoss();
        if (showOnScreenControls)
        {
            hudControl.UpdateOnScreenControls();
        }
        if (isRaid)
        {
            if (raidCounter > 0)
            {
                raidCounter -= Time.deltaTime;
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
            if (Vector3.Distance(playersManager.playersCentralPosition, boss.transform.position) < 60)
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
                sun.GetComponent<Light>().intensity = Mathf.Lerp(1f, .0f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(1f, .25f, t);
            }
            cycleSpeed = .5f;
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
                sun.GetComponent<Light>().intensity = Mathf.Lerp(0f, 1f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(.25f, 1f, t);
            }
            cycleSpeed = 2f;
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
