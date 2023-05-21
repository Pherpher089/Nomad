using UnityEngine;

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
    public float timeCounter = 0;
    public TimeCycle timeCycle = TimeCycle.Dawn;
    GameObject sun;
    public bool peaceful;
    public bool friendlyFire;
    public bool firstPlayerKeyboardAndMouse;
    public string[] players;
    [HideInInspector]
    public bool initialized = false;
    public void Awake()
    {
        sun = GameObject.Find("Sun");
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        InitializeGameState();
    }
    public void InitializeGameState()
    {
        LevelManager.SpawnPlayers(players);
        playersManager.Init();
        hudControl.Initialize();
        initialized = true;
    }

    void GameplayStateMachine()
    {
        switch (timeState)
        {
            case TimeState.Day:
                if (timeCycle == TimeCycle.Dusk)
                {
                    timeState = TimeState.Night;
                }
                break;
            case TimeState.Night:
                if (timeCycle == TimeCycle.Dawn)
                {
                    timeState = TimeState.Day;
                }
                break;
            default:
                break;
        }
    }

    void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.PlayState:
                Time.timeScale = 1;
                hudControl.EnablePauseScreen(false);
                if (Input.GetButtonDown("Cancel"))
                {
                    gameState = GameState.PauseState;
                }
                break;
            case GameState.PauseState:
                Time.timeScale = 0;
                hudControl.EnablePauseScreen(true);
                if (Input.GetButtonDown("Cancel"))
                {
                    gameState = GameState.PlayState;
                }
                break;
            case GameState.WinState:
                Time.timeScale = 0;
                hudControl.EnableWinScreen(true);
                break;
            case GameState.FailState:
                Time.timeScale = 0;
                hudControl.EnableFailScreen(true);
                break;
            default:
                break;
        }
    }

    void Update()
    {
        DayNightCycle();
        GameplayStateMachine();
    }

    private void DayNightCycle()
    {
        sun.transform.Rotate(-Vector3.right * cycleSpeed * Time.deltaTime);
        float sunRotation = sun.transform.rotation.eulerAngles.x;
        timeCounter += cycleSpeed * Time.deltaTime;
        if (sun.transform.rotation.x < 180)
        {
            timeState = TimeState.Day;
            sun.GetComponent<Light>().intensity = 1;

        }
        else if (sun.transform.rotation.x < 360)
        {
            timeState = TimeState.Night;
            sun.GetComponent<Light>().intensity = 0;

        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }
}
