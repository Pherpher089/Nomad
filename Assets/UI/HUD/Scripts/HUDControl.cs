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
    PlayersManager playersManager;
    HUDParent hudParent;

    public void Initialize()
    {
        Debug.Log("### H343");
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
        playersManager = GetComponent<PlayersManager>();
        hudParent = transform.GetComponentInChildren<HUDParent>();
        InitHealthBars();

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

    public void InitHealthBars()
    {
        int activePlayer = playersManager.playerList.Count;
        Debug.Log("active player " + activePlayer);

        for (int i = 0; i < hudParent.healthbarList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.canvasList[i].enabled = true;
                hudParent.healthbarList[i].minValue = 0;
                hudParent.healthbarList[i].maxValue = playersManager.playerList[i].GetComponent<HealthManager>().maxHealth;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
    }

    void LateUpdate()
    {
        if (playersManager)
        {
            for (int i = 0; i < playersManager.playerList.Count; i++)
            {
                hudParent.healthbarList[i].value = playersManager.playerList[i].GetComponent<HealthManager>().health;
            }
        }
    }
}

