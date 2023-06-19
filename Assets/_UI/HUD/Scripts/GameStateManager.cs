using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { PlayState, PauseState, WinState, FailState }
public enum TimeState { Day, Night }
public enum TimeCycle { Dawn, Morning, Noon, Afternoon, Dusk, Evening, Midnight, Latenight }

public class GameStateManager : MonoBehaviour
{
    public string m_WorldName = "Default";
    public bool newWorld = false;
    public GameState gameState;
    public TimeState timeState;

    HUDControl hudControl;
    public PlayersManager playersManager;
    //Day-Night Cycle Control
    public float cycleSpeed = 1;
    [Range(0, 360)] public float timeCounter = 90;
    public TimeCycle timeCycle = TimeCycle.Dawn;
    private GameObject sun;
    public bool peaceful;
    public bool friendlyFire;
    public bool firstPlayerKeyboardAndMouse;
    public bool showOnScreenControls;
    public Material[] playerMats;
    public string[] players;
    public Vector3 currentRespawnPoint;

    [HideInInspector]
    public bool initialized = false;
    public void Awake()
    {
        sun = GameObject.Find("Sun");
        sun.transform.rotation = Quaternion.Euler(timeCounter, 0, 0);
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        InitializeGameState();
    }
    public void RespawnParty()
    {
        LevelManager.SaveLevel(currentRespawnPoint);
        SceneManager.LoadScene("EndlessTerrain");

    }
    public void InitializeGameState()
    {
        LevelManager.SpawnPlayers(players);
        playersManager.Init();
        hudControl.Initialize();
        initialized = true;
    }


    void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.PlayState:
                Time.timeScale = 1;
                break;
            case GameState.PauseState:
                Time.timeScale = 0;
                break;
            case GameState.FailState:
                Time.timeScale = 0;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        DayNightCycle();
        GameStateMachine();
        if (showOnScreenControls)
        {
            hudControl.UpdateOnScreenControls();
        }
    }

    public void ToggleOnScreenControls()
    {
        showOnScreenControls = !showOnScreenControls;
        hudControl.UpdateOnScreenControls();
    }

    private void DayNightCycle()
    {
        sun.transform.Rotate(Vector3.right * cycleSpeed * Time.deltaTime);
        float sunRotation = sun.transform.rotation.eulerAngles.x;
        timeCounter += cycleSpeed * Time.deltaTime;

        if (timeCounter < 180)
        {
            if (timeCounter > 150)
            {
                float t = Mathf.InverseLerp(150, 180, timeCounter);
                // Lerp from 1 to 0
                sun.GetComponent<Light>().intensity = Mathf.Lerp(1f, 0f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(1f, 0f, t);
            }
            timeState = TimeState.Day;
        }
        else if (timeCounter > 180)
        {
            if (timeCounter > 330)
            {
                float t = Mathf.InverseLerp(330, 359, timeCounter);
                // Lerp from 0 to 1
                sun.GetComponent<Light>().intensity = Mathf.Lerp(0f, 1f, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(0f, 1f, t);
            }
            timeState = TimeState.Night;
        }

        if (timeCounter >= 359)
        {
            timeCounter = 0;
        }
    }

}
