using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDControl : MonoBehaviour
{

    Canvas pauseScreen;
    Canvas winScreen;
    Canvas failScreen;
    Button retryButton;
    Button continueButton;
    Button quitButton;
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

    public void Initialize()
    {
        pauseScreen = GameObject.Find("Canvas_PauseScreen").GetComponent<Canvas>();
        winScreen = GameObject.Find("Canvas_WinScreen").GetComponent<Canvas>();
        failScreen = GameObject.Find("Canvas_FailScreen").GetComponent<Canvas>();
        retryButton = GameObject.Find("Button_Retry").GetComponent<Button>();
        continueButton = GameObject.Find("Button_Continue").GetComponent<Button>();
        quitButton = GameObject.Find("Button_Quit").GetComponent<Button>();
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
        InitSliders();

    }

    public void EnablePauseScreen(bool _enabled)
    {
        pauseScreen.enabled = _enabled;
    }

    public void EnableWinScreen(bool _enabled)
    {
        winScreen.enabled = _enabled;
    }

    public void EnableFailScreen(bool _enabled)
    {
        failScreen.enabled = _enabled;
    }

    public void OnRetry()
    {

    }

    public void OnContinue()
    {

    }

    public void OnQuit()
    {

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

