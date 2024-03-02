using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class scene_split_screen_WPIDEMO : MonoBehaviour
{

    public Canvas levelCanvas;
    public Material skybox;
    public GameObject sceneCam;

    private GameObject firstPersonPlayer;


    void OnEnable()
    {
        event_manager_WPIDEMO.onOpenOptionScreen += HideUI;
        event_manager_WPIDEMO.onCloseOptionScreen += ShowUI;
        event_manager_WPIDEMO.StartLevel(); //Tell eveyone the level has started
    }

    void OnDisable()
    {
        event_manager_WPIDEMO.onOpenOptionScreen -= HideUI;
        event_manager_WPIDEMO.onCloseOptionScreen -= ShowUI;

        if (firstPersonPlayer != null)
        {
            firstPersonPlayer.SetActive(true);
        }
    }

    void HideUI()
    {
        levelCanvas.enabled = false;
    }

    void ShowUI()
    {
        levelCanvas.enabled = true;
    }


    void Start()
    {
        //If this scene is active when play is hit, kill it and load the title screen
        if (scene_manager_WPIDEMO.curLevel == 0)
        {
            SceneManager.LoadScene("_root", LoadSceneMode.Single);
        }
        else
        {
            //Find First Person Player so that we can hide it
            firstPersonPlayer = GameObject.FindWithTag("Player");
            if (firstPersonPlayer != null)
            {
                firstPersonPlayer.SetActive(false);
                sceneCam.SetActive(false);
            }

            //Set skybox
            RenderSettings.skybox = skybox;

            //Hide/Show UI depending if Instruction Window is active
            if (scene_manager_WPIDEMO.instructionsWindowOpen)
            {
                HideUI(); //Do this if Instructions Window is up
            }
            else
            {
                ShowUI(); //Do this if Instructions Window closes 
            }


            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Scene_" + scene_manager_WPIDEMO.curLevel));
        }


    }


}
