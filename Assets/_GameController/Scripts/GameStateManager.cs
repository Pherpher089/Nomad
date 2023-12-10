using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    HUDControl hudControl;
    public PlayersManager playersManager;
    //Day-Night Cycle Control
    public float cycleSpeed = 1;
    [Range(0, 360)] public float timeCounter = 90;
    public TimeCycle timeCycle = TimeCycle.Dawn;
    public GameObject sun;
    public bool peaceful;
    public bool friendlyFire;
    [SerializeField]
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
    public void Awake()
    {
        Instance = this;
        m_WorldName = LevelPrep.Instance.settlementName;
        sun = GameObject.Find("Sun");
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        // InitializeGameState();
    }
    public void InitializeGameState()
    {
        playersManager.UpdatePlayers();
        hudControl.Initialize();
        initialized = true;
    }

    public void SetTime(float time, float sunRot)
    {
        timeCounter = time;
        sun.transform.rotation = Quaternion.Euler(sunRot, 0, 0);
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

        //NOTE: Previously this was used to save the state of the level periodically and update the other clients. May not need this. Disabling due to errors
        // if (Time.time >= nextCheckTime)
        // {
        //     Vector3 centerPoint = PlayersManager.Instance.GetCenterPoint();
        //     if (Vector3.Distance(spawnPoint, centerPoint) > 20)
        //     {
        //         LevelManager.SaveLevel();
        //         if (PhotonNetwork.IsMasterClient)
        //         {
        //             LevelManager.Instance.UpdateLevelData();
        //         }
        //     }
        //     nextCheckTime = Time.time + checkInterval;
        // }
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
            cycleSpeed = 0.5f;
            timeState = TimeState.Day;
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
            timeState = TimeState.Night;
        }

        if (timeCounter >= 359)
        {
            timeCounter = 0;
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
