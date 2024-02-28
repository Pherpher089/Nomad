using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WPI_Manager : MonoBehaviour
{
    #region Define Vars
    public static bool waypoint_indicators_are_visible = true;
    #endregion


    #region Define Delegates
    //Delegates that take no arguements
    public delegate void MyDelegate(); //Define method signature
    public static MyDelegate onToggleVisibility;

    //Delegates that take strings
    public delegate void MyStringDelegate(string newCameraTag, string newDistCalTargetTag);  //Define method signature
    public static MyStringDelegate onSwitchCams;
    #endregion


    #region Toggle Visibility
    public static void ToggleVisibility()
    {

        if (onToggleVisibility != null)
        {
            
            if (waypoint_indicators_are_visible)
            {
                waypoint_indicators_are_visible = false;
            }
            else
            {
                waypoint_indicators_are_visible = true;
            }

            onToggleVisibility();
        }
    }
    #endregion


    #region Switch Cameras
    public static void SwitchCams(string newCameraTag, string newDistCalTargetTag)
    {
        if (onSwitchCams != null)
        {
            onSwitchCams(newCameraTag, newDistCalTargetTag);
        }
    }
    #endregion

}
