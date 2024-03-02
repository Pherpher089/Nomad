using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_manager_WPIDEMO : MonoBehaviour
{
    #region Load Levels
    public void ResetToTitleScreen()
    {
        event_manager_WPIDEMO.onResetToTitleScreen();
    }

    public void LoadDayScene()
    {
        event_manager_WPIDEMO.onLoadScene(1);
    }

    public void LoadNightScene()
    {
        event_manager_WPIDEMO.onLoadScene(2);
    }

    public void LoadMultiCamScene()
    {
        event_manager_WPIDEMO.onLoadScene(3);
    }

    public void LoadSplitScreenScene()
    {
        event_manager_WPIDEMO.onLoadScene(4);
    }

    public void CloseInstructionWindow()
    {
        event_manager_WPIDEMO.onCloseOptionScreenWithX();
    }

    public void OpenInstructionWindow()
    {
        //event_manager_WPIDEMO.onOpenOptionScreen();
        Debug.Log("Poop");
    }

    #endregion


}
