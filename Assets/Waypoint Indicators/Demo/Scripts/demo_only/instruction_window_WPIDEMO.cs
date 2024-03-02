using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class instruction_window_WPIDEMO : MonoBehaviour
{
    public GameObject btn_Day;
    public GameObject btn_Day_Selected;
    public GameObject indicator_Day;
    public GameObject btn_Night;
    public GameObject btn_Night_Selected;
    public GameObject indicator_Night;
    public GameObject btn_MultiCam;
    public GameObject btn_MultiCam_Selected;
    public GameObject indicator_MultiCam;
    public GameObject btn_SplitScreen;
    public GameObject btn_SplitScreen_Selected;
    public GameObject indicator_SplitScreen;



    void OnEnable()
    {
        event_manager_WPIDEMO.onLoadScene += SwapSceneButtonStates;
    }

    void OnDisable()
    {
        event_manager_WPIDEMO.onLoadScene -= SwapSceneButtonStates;
    }


    void SwapSceneButtonStates(int levelToLoad)
    {
        if (levelToLoad == 1)
        {
            //Highlight Day Scene
            btn_Day.SetActive(false);
            btn_Day_Selected.SetActive(true);
            indicator_Day.SetActive(true);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (levelToLoad == 2)
        {
            //Highlight Night Scene
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(false);
            btn_Night_Selected.SetActive(true);
            indicator_Night.SetActive(true);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (levelToLoad == 3)
        {
            //Highlight MultiCam Scene
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(false);
            btn_MultiCam_Selected.SetActive(true);
            indicator_MultiCam.SetActive(true);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (levelToLoad == 4)
        {
            //Highlight Split Screen
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(false);
            btn_SplitScreen_Selected.SetActive(true);
            indicator_SplitScreen.SetActive(true);
        }
    }


    void Start()
    {
        //When this loads, check to see what the levelNum is then show buttons accordingly
        if (scene_manager_WPIDEMO.curLevel == 1)
        {
            //Highlight Day Scene
            btn_Day.SetActive(false);
            btn_Day_Selected.SetActive(true);
            indicator_Day.SetActive(true);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (scene_manager_WPIDEMO.curLevel == 2)
        {
            //Highlight Night Scene
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(false);
            btn_Night_Selected.SetActive(true);
            indicator_Night.SetActive(true);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (scene_manager_WPIDEMO.curLevel == 3)
        {
            //Highlight Night Scene
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(false);
            btn_MultiCam_Selected.SetActive(true);
            indicator_MultiCam.SetActive(true);
            btn_SplitScreen.SetActive(true);
            btn_SplitScreen_Selected.SetActive(false);
            indicator_SplitScreen.SetActive(false);
        }
        if (scene_manager_WPIDEMO.curLevel == 4)
        {
            //Highlight Split Screen
            btn_Day.SetActive(true);
            btn_Day_Selected.SetActive(false);
            indicator_Day.SetActive(false);
            btn_Night.SetActive(true);
            btn_Night_Selected.SetActive(false);
            indicator_Night.SetActive(false);
            btn_MultiCam.SetActive(true);
            btn_MultiCam_Selected.SetActive(false);
            indicator_MultiCam.SetActive(false);
            btn_SplitScreen.SetActive(false);
            btn_SplitScreen_Selected.SetActive(true);
            indicator_SplitScreen.SetActive(true);
        }
    }

}
