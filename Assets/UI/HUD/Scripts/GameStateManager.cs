using UnityEngine;

public enum GameState { PlayState, PauseState, WinState, FailState }
public enum GameplayState { Day, Night }
public enum TimeCycle { Dawn, Morning, Noon, Afternoon, Dusk, Evening, Midnight, Latenight }

public class GameStateManager : MonoBehaviour
{
    public string m_WorldName = "Default";
    public bool newWorld = false;
    public GameState gameState;
    public GameplayState gameplayState;

    HUDControl hudControl;
    public PlayersManager playersManager;
    //Day-Night Cycle Control
    public float cycleSpeed = 1;
    public float timeCounter = 0;
    public TimeCycle timeCycle = TimeCycle.Dawn;
    GameObject sun;

    public void Awake()
    {
        sun = GameObject.Find("Sun");
        playersManager = gameObject.GetComponent<PlayersManager>();
        hudControl = GetComponent<HUDControl>();
        InitializeGameState();
    }
    public void InitializeGameState()
    {
        playersManager.Init();
        hudControl.Initialize();
    }

    void GameplayStateMachine()
    {
        switch (gameplayState)
        {
            case GameplayState.Day:
                if (timeCycle == TimeCycle.Dusk)
                {
                    gameplayState = GameplayState.Night;
                }
                break;
            case GameplayState.Night:
                if (timeCycle == TimeCycle.Dawn)
                {
                    gameplayState = GameplayState.Day;
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
        if (timeCounter >= 45)
        {
            if (timeCycle == TimeCycle.Latenight)
            {
                timeCycle = TimeCycle.Dawn;
            }
            else
            {
                timeCycle += 1;
            }
            timeCounter = 0;
        }
    }
}
