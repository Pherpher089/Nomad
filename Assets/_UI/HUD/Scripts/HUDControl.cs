using static TMPro.TextMeshPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HUDControl : MonoBehaviour
{

    public GameObject pauseScreen;
    public GameObject failScreen;
    public GameObject loadingScreen;
    public GameObject bossHealthBarCanvasObject;
    public GameObject[] controlsUi;
    Slider healthBarP1;
    Slider healthBarP2;
    Slider healthBarP3;
    Slider healthBarP4;
    Slider hungerBarP1;
    Slider hungerBarP2;
    Slider hungerBarP3;
    Slider hungerBarP4;
    Slider bossHealthSlider;
    HUDParent hudParent;
    public bool isPaused = false;
    GameStateManager gameController;
    bool initialized = false;

    public void Initialize()
    {
        gameController = GetComponent<GameStateManager>();
        bossHealthBarCanvasObject = GameObject.Find("BossHealth");
        pauseScreen = GameObject.Find("Canvas_PauseScreen").transform.GetChild(0).gameObject;
        failScreen = GameObject.Find("Canvas_FailScreen").transform.GetChild(0).gameObject;
        loadingScreen = GameObject.Find("Canvas_LoadingScreen");
        healthBarP1 = GameObject.Find("HealthBar_P1").GetComponent<Slider>();
        healthBarP2 = GameObject.Find("HealthBar_P2").GetComponent<Slider>();
        healthBarP3 = GameObject.Find("HealthBar_P3").GetComponent<Slider>();
        healthBarP4 = GameObject.Find("HealthBar_P4").GetComponent<Slider>();
        hungerBarP1 = GameObject.Find("HungerBar_P1").GetComponent<Slider>();
        hungerBarP2 = GameObject.Find("HungerBar_P2").GetComponent<Slider>();
        hungerBarP3 = GameObject.Find("HungerBar_P3").GetComponent<Slider>();
        hungerBarP4 = GameObject.Find("HungerBar_P4").GetComponent<Slider>();
        hudParent = transform.GetComponentInChildren<HUDParent>();
        controlsUi = new GameObject[transform.childCount - 3];
        bossHealthSlider = GameObject.Find("BossHealthSlider").GetComponent<Slider>();
        bossHealthBarCanvasObject.SetActive(false);
        for (int i = 3; i < transform.childCount; i++)
        {
            controlsUi[i - 3] = transform.GetChild(i).gameObject;
        }
        hudParent.InitializeBars();
        InitSliders();
        pauseScreen.SetActive(false);
        initialized = true;

    }

    public void InitializeBossHealthBar(BossManager bossManager)
    {
        if (!initialized) return;
        bossHealthBarCanvasObject.SetActive(true);
        bossHealthBarCanvasObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = bossManager.gameObject.name;
        bossHealthSlider.value = bossManager.GetComponent<HealthManager>().health;
        bossHealthSlider.maxValue = bossManager.GetComponent<HealthManager>().maxHealth;
    }

    public void UpdateOnScreenControls()
    {
        if (PlayersManager.Instance.playerList.Count > 0)
        {
            int newActivePanel = LevelPrep.Instance.firstPlayerGamePad ? 0 : 5;
            GameObject item = PlayersManager.Instance.playerList[0].GetComponent<ActorEquipment>().equippedItem;
            if (!gameController.showOnScreenControls || PlayersManager.Instance.playerList[0].GetComponent<ThirdPersonUserControl>().usingUI || GameStateManager.Instance.gameState == GameState.PauseState)
            {
                newActivePanel = -1;
            }
            else if (PlayersManager.Instance.playerList[0].GetComponent<BuilderManager>().isBuilding)
            {
                newActivePanel += 4;
            }
            else if (item != null)
            {
                if (item.GetComponent<Item>().gameObject.CompareTag("Tool") && item.GetComponent<Item>().itemName == "Torch")
                {
                    newActivePanel++;
                }
                else if (item.GetComponent<BuildingMaterial>() != null)
                {
                    newActivePanel += 2;
                }
                else if (!item.GetComponent<Item>().gameObject.CompareTag("Tool"))
                {
                    newActivePanel += 3;
                }
            }

            for (int i = 0; i < controlsUi.Length; i++)
            {
                if (i == newActivePanel)
                {
                    controlsUi[i].SetActive(true);
                }
                else
                {
                    controlsUi[i].SetActive(false);
                }
            }
        }
    }

    public void EnablePauseScreen(bool _enabled)
    {
        isPaused = _enabled;
        pauseScreen.SetActive(_enabled);
        GameStateManager.Instance.UpdateSettingsValues();
        if (_enabled)
        {
            gameController.gameState = GameState.PauseState;
        }
        else
        {
            gameController.gameState = GameState.PlayState;
        }
    }

    public void EnableFailScreen(bool _enabled)
    {
        failScreen.SetActive(_enabled);
    }

    public void OnRetry()
    {
        //This UI is no longer needed
        //FindObjectOfType<GameStateManager>().RespawnParty();
    }

    public void OnContinue()
    {
        EnablePauseScreen(false);
    }

    public void OnQuit()
    {
        Application.Quit();
        SceneManager.LoadScene(0);
    }
    public void InitSliders()
    {
        int activePlayer = PlayersManager.Instance.playerList.Count;
        for (int i = 0; i < hudParent.healthList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.canvasList[i].enabled = true;
                hudParent.healthList[i].minValue = 0;
                hudParent.healthList[i].maxValue = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().maxHealth;

            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.hungerList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.hungerList[i].minValue = 0;
                hudParent.hungerList[i].maxValue = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().stomachCapacity;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.experienceList.Count; i++)
        {
            if (i < activePlayer)
            {
                SetExpSlider(i);
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.nameList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.nameList[i].text = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().characterName;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
    }

    private void SetExpSlider(int i)
    {
        CharacterStats stats = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>();
        hudParent.experienceList[i].minValue = stats.experienceThresholds[stats.characterLevel - 1];
        hudParent.experienceList[i].maxValue = stats.experienceThresholds[stats.characterLevel];
        hudParent.levelList[i].text = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().characterLevel.ToString();
    }

    void LateUpdate()
    {
        if (PlayersManager.Instance)
        {
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.healthList[i].value = PlayersManager.Instance.playerList[i].GetComponent<HealthManager>().health;
            }
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.hungerList[i].value = PlayersManager.Instance.playerList[i].GetComponent<HungerManager>().m_StomachValue;
            }
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.experienceList[i].value = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().experiencePoints;
                if (PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().experiencePoints >= hudParent.experienceList[i].maxValue)
                {
                    PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().GenerateStats();
                    SetExpSlider(i);
                }
            }
        }
    }
}

