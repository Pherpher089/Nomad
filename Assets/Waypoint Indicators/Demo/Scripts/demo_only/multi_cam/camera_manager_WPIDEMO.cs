using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class camera_manager_WPIDEMO : MonoBehaviour
{
    //Call cameras by their game object so we can enable/disable them for this example
    public GameObject cam1;
    public GameObject cam2;
    public GameObject cam3;

    //Stores the tag name of the camera you’ll be switching to
    private string newCameraTag;
    //Stores the tag name of the game object you’ll be calculating distance from 
    private string newDistCalTargetTag;

    //Stores the camera name seen at the top of this demo scene
    public TextMeshProUGUI activeWPIDemoCamTxtField;


    //Set the default camera on Start
    private void Start()
    {
        newCameraTag = cam1.tag;
        newDistCalTargetTag = cam1.tag;
        WPI_Manager.SwitchCams(newCameraTag, newDistCalTargetTag);
        cam1.SetActive(true);
        cam2.SetActive(false);
        cam3.SetActive(false);
    }


    void Update()
    {
        if (Input.GetKeyUp("1") || Input.GetKeyUp("[1]"))
        {
            activeWPIDemoCamTxtField.text = "CAMERA 1";
            newCameraTag = cam1.tag;
            newDistCalTargetTag = cam1.tag;
            WPI_Manager.SwitchCams(newCameraTag, newDistCalTargetTag);
            cam1.SetActive(true);
            cam2.SetActive(false);
            cam3.SetActive(false);
            
        }

        if (Input.GetKeyUp("2") || Input.GetKeyUp("[2]"))
        {
            activeWPIDemoCamTxtField.text = "CAMERA 2";
            newCameraTag = cam2.tag;
            newDistCalTargetTag = cam2.tag;
            WPI_Manager.SwitchCams(newCameraTag, newDistCalTargetTag);
            cam1.SetActive(false);
            cam2.SetActive(true);
            cam3.SetActive(false);
            
        }

        if (Input.GetKeyUp("3") || Input.GetKeyUp("[3]"))
        {
            activeWPIDemoCamTxtField.text = "CAMERA 3";
            newCameraTag = cam3.tag;
            newDistCalTargetTag = cam3.tag;
            WPI_Manager.SwitchCams(newCameraTag, newDistCalTargetTag);
            cam1.SetActive(false);
            cam2.SetActive(false);
            cam3.SetActive(true);
            
        }
    }
}
