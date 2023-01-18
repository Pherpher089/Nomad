using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour {

    Canvas mainMenu;
    Button playButton;
    Button quitButton;

    void Awake()
    {
        mainMenu = GameObject.Find("Canvas_MainMenu").GetComponent<Canvas>();
        playButton = GameObject.Find("Button_Play").GetComponent<Button>();
        quitButton = GameObject.Find("Button_Quit").GetComponent<Button>();
    }

}
