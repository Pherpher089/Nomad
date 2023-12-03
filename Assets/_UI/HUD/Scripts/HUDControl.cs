using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDControl : MonoBehaviour
{

    public GameObject pauseScreen;

    public GameObject failScreen;
    public GameObject[] ControlsUi;
    Slider healthBarP1;
    Slider healthBarP2;
    Slider healthBarP3;
    Slider healthBarP4;
    Slider hungerBarP1;
    Slider hungerBarP2;
    Slider hungerBarP3;
    Slider hungerBarP4;
    PlayersManager playersManager;
    HUDParent hudParent;
    public bool isPaused = false;
    GameStateManager gameController;

    public void Initialize()
    {
        gameController = GetComponent<GameStateManager>();
        pauseScreen = GameObject.Find("Canvas_PauseScreen").transform.GetChild(0).gameObject;
        failScreen = GameObject.Find("Canvas_FailScreen").transform.GetChild(0).gameObject;
        healthBarP1 = GameObject.Find("HealthBar_P1").GetComponent<Slider>();
        healthBarP2 = GameObject.Find("HealthBar_P2").GetComponent<Slider>();
        healthBarP3 = GameObject.Find("HealthBar_P3").GetComponent<Slider>();
        healthBarP4 = GameObject.Find("HealthBar_P4").GetComponent<Slider>();
        hungerBarP1 = GameObject.Find("HungerBar_P1").GetComponent<Slider>();
        hungerBarP2 = GameObject.Find("HungerBar_P2").GetComponent<Slider>();
        hungerBarP3 = GameObject.Find("HungerBar_P3").GetComponent<Slider>();
        hungerBarP4 = GameObject.Find("HungerBar_P4").GetComponent<Slider>();
        playersManager = GetComponent<PlayersManager>();
        hudParent = transform.GetComponentInChildren<HUDParent>();
        ControlsUi = new GameObject[transform.childCount - 3];
        for (int i = 3; i < transform.childCount; i++)
        {
            ControlsUi[i - 3] = transform.GetChild(i).gameObject;
        }
        hudParent.InitializeBars();
        InitSliders();
    }

    public void UpdateOnScreenControls()
    {
        // int newActivePanel = gameController.firstPlayerKeyboardAndMouse ? 5 : 0;
        // GameObject item = playersManager.playerList[0].GetComponent<ActorEquipment>().equippedItem;
        // if (!gameController.showOnScreenControls || playersManager.playerList[0].GetComponent<PlayerInventoryManager>().isActive)
        // {
        //     newActivePanel = -1;
        // }
        // else if (playersManager.playerList[0].GetComponent<BuilderManager>().isBuilding)
        // {
        //     newActivePanel += 4;
        // }
        // else if (item != null)
        // {
        //     if (item.GetComponent<Item>().gameObject.tag == "Tool" && item.GetComponent<Item>().itemName == "Torch")
        //     {
        //         newActivePanel++;
        //     }
        //     else if (item.GetComponent<BuildingMaterial>() != null)
        //     {
        //         newActivePanel += 2;
        //     }
        //     else if (item.GetComponent<Item>().gameObject.tag != "Tool")
        //     {
        //         newActivePanel += 3;
        //     }
        // }

        // for (int i = 0; i < ControlsUi.Length; i++)
        // {
        //     if (i == newActivePanel)
        //     {
        //         ControlsUi[i].SetActive(true);
        //     }
        //     else
        //     {
        //         ControlsUi[i].SetActive(false);
        //     }
        // }
    }

    public void EnablePauseScreen(bool _enabled)
    {
        isPaused = _enabled;
        pauseScreen.SetActive(_enabled);
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
        int activePlayer = playersManager.playerList.Count;
        for (int i = 0; i < hudParent.healthList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.canvasList[i].enabled = true;
                hudParent.healthList[i].minValue = 0;
                hudParent.healthList[i].maxValue = playersManager.playerList[i].GetComponent<CharacterStats>().maxHealth;
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
                hudParent.hungerList[i].maxValue = playersManager.playerList[i].GetComponent<CharacterStats>().stomachCapacity;
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
                hudParent.nameList[i].text = playersManager.playerList[i].GetComponent<CharacterStats>().characterName;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
    }

    private void SetExpSlider(int i)
    {
        CharacterStats stats = playersManager.playerList[i].GetComponent<CharacterStats>();
        hudParent.experienceList[i].minValue = stats.experienceThresholds[stats.characterLevel - 1];
        hudParent.experienceList[i].maxValue = stats.experienceThresholds[stats.characterLevel];
        hudParent.levelList[i].text = playersManager.playerList[i].GetComponent<CharacterStats>().characterLevel.ToString();
    }

    void LateUpdate()
    {
        if (playersManager)
        {
            for (int i = 0; i < playersManager.playerList.Count; i++)
            {
                hudParent.healthList[i].value = playersManager.playerList[i].GetComponent<HealthManager>().health;
            }
            for (int i = 0; i < playersManager.playerList.Count; i++)
            {
                hudParent.hungerList[i].value = playersManager.playerList[i].GetComponent<HungerManager>().m_StomachValue;
            }
            for (int i = 0; i < playersManager.playerList.Count; i++)
            {
                hudParent.experienceList[i].value = playersManager.playerList[i].GetComponent<CharacterStats>().experiencePoints;
                if (playersManager.playerList[i].GetComponent<CharacterStats>().experiencePoints >= hudParent.experienceList[i].maxValue)
                {
                    playersManager.playerList[i].GetComponent<CharacterStats>().GenerateStats();
                    SetExpSlider(i);
                }
            }
        }
    }
}

