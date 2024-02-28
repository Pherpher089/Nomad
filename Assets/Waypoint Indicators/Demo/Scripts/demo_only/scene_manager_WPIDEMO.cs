using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class scene_manager_WPIDEMO : MonoBehaviour
{
    //This script manages:
    //Title screen
    //Instruction menu


    //Root Canvas Vars
    public Canvas mainCanvas;
    public GameObject titleScreen;
    public Transform playerPos;
    public static bool instructionsWindowOpen;
    private GameObject instructionsWindowGameObject;
    private RectTransform instructionsWindowRect;
    public static bool canShoot;
    public static int spawnCount = 0;
    public static int spawnCountMax = 500;
    public static int curLevel;

    private GameObject station1;
    private GameObject station2;
    private GameObject station3;
    private GameObject station4;


    //Scene Vars
    /*
    public float spawnSpeed = 5f;
    public TextMeshProUGUI spawnCountTextField;
    public TextMeshProUGUI spawnDescTextField;
    public Transform spawnPos;
    private int exampleNum = 1;
    private int totalExamples = 5;
    public GameObject shapeSelectGameObject;
    private RectTransform shapeSelectRect;
    private Image shapeSelectImg;
    public Sprite slot01;
    public Sprite slot02;
    public Sprite slot03;
    public Sprite slot04;
    public Sprite slot05;
    private GameObject newWaypoint;
    private Rigidbody newWaypointRB;
    private string shapeName;
    */




    void OnEnable()
    {
        event_manager_WPIDEMO.onLoadScene += LoadLevel;
        event_manager_WPIDEMO.onCloseOptionScreenWithX += CloseOptionScreenManually;
    }

    void OnDisable()
    {
        event_manager_WPIDEMO.onLoadScene -= LoadLevel;
        event_manager_WPIDEMO.onCloseOptionScreenWithX -= CloseOptionScreenManually;
    }



    void Start()
    {
        //mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        Cursor.lockState = CursorLockMode.Locked;

        //Hides title screen, then turns on the instructions prompt
        Invoke("HideTitleScreen", 0f);

        //Default to Scene_1 (Day)
        LoadLevel(1);
    }

    void Update()
    {
        //Toggle Instructions Popup
        //Input.GetKeyDown(KeyCode.Escape) - Do not use ESCAPE because it gets in the way of Debugging
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            ToggleInstructions();
        }

        /*
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            event_manager_WPIDEMO.onLoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            event_manager_WPIDEMO.onLoadScene(2);
        }
        */

        //Toggle Hide or Show all waypoint indicators in scene without hiding their game objects (new in v4.1.3)
        if (Input.GetKeyUp("h"))
        {
            //Hides all waypoints if they are active, and shows them if they are inactive
            WPI_Manager.ToggleVisibility();
        }

    }


    void HideTitleScreen()
    {
        titleScreen.SetActive(false);
        //Turns Instructions window on

        ToggleInstructions();   
    }


    void LoadLevel(int levelToLoad)
    {
        //Load level number based on button_manager input
        SceneManager.LoadScene("Scene_" + levelToLoad, LoadSceneMode.Additive);

        //Reset player position
        playerPos.position = new Vector3(0f, 1.15f, 0f);
        //Reset player to face forward
        //playerPos.rotation = Quaternion.Euler(0, 0, 0);

        //If there is at least one level present other than the title screen "0", unload it
        if (curLevel > 0)
        {
            SceneManager.UnloadSceneAsync("Scene_" + curLevel);
        }
            //Reset currnt level variable with the level number we just loaded
            curLevel = levelToLoad;

        if (curLevel == 1)
        {
            //Destroy Space Stations
            //DestroyStations();
        }
        if (curLevel == 2)
        {
            //Destroy Space Stations (In case user loads Scene 2 while Scene 2 is currentl active
            //DestroyStations();
            //Instantiate Space Stations
            //InstantiateStations();
        }
    }

    void InstantiateStations()
    {
        //This spawns 4 sations around the player in the Night Scene (2)
        //Each pre-scene spawning prefab must have a "reload_WPIDEMO.cs" attached to it
        station1 = Instantiate(Resources.Load("Night/4 Station", typeof(GameObject)), new Vector3(54f, 17f, 42f), Quaternion.identity) as GameObject;
        station2 = Instantiate(Resources.Load("Night/4 Station", typeof(GameObject)), new Vector3(-58f, -11f, 42f), Quaternion.identity) as GameObject;
        station3 = Instantiate(Resources.Load("Night/4 Station", typeof(GameObject)), new Vector3(-58f, -27f, -54f), Quaternion.identity) as GameObject;
        station4 = Instantiate(Resources.Load("Night/4 Station", typeof(GameObject)), new Vector3(54f, 7f, -54f), Quaternion.identity) as GameObject;
    }

    void DestroyStations()
    {
        //Destroys any instantiated Space Station objects in Scene 2
        Destroy(station1);
        Destroy(station2);
        Destroy(station3);
        Destroy(station4);
    }

    void CloseOptionScreenManually()
    {
        //Tell level script that we are closing the Instructions Window and to enable its UI again
        event_manager_WPIDEMO.CloseOptionScreen();

        Destroy(instructionsWindowGameObject);
        //Debug.Log("Options Window Closed");
        instructionsWindowOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        //Enable player movement
        fpc_WPIDEMO.playerCanMove = true;
        canShoot = true;
    }


    public void ToggleInstructions()
    {
        if (!instructionsWindowOpen) //If options window is closed, open it
        {
            //Tell the level script that the Instructions Window is open and to hide its UI so won't be covered up
            event_manager_WPIDEMO.OpenOptionScreen();

            instructionsWindowGameObject = Instantiate(Resources.Load("UI/Intructions Window", typeof(GameObject)), transform.position, Quaternion.identity) as GameObject;
            instructionsWindowGameObject.layer = 5;
            instructionsWindowRect = instructionsWindowGameObject.GetComponent<RectTransform>();
            instructionsWindowRect.transform.SetParent(mainCanvas.transform);
            instructionsWindowRect.localScale = new Vector2(1f, 1f); //user set scale
            instructionsWindowRect.offsetMin = new Vector2(0, instructionsWindowRect.offsetMin.y);
            instructionsWindowRect.offsetMax = new Vector2(0, instructionsWindowRect.offsetMax.y);
            instructionsWindowRect.offsetMax = new Vector2(instructionsWindowRect.offsetMax.x, 0);
            instructionsWindowRect.offsetMin = new Vector2(instructionsWindowRect.offsetMin.x, 0);
            //instructionsWindowRect.transform.localPosition = new Vector3(0f, 0f, 0f);

            //Debug.Log("Options Window Open");
            instructionsWindowOpen = true;
            Cursor.lockState = CursorLockMode.None;
            //Stop player movement
            fpc_WPIDEMO.playerCanMove = false;
            canShoot = false;
        }
        else //Close options window
        {
            //Tell level script that we are closing the Instructions Window and to enable its UI again
            event_manager_WPIDEMO.CloseOptionScreen();

            Destroy(instructionsWindowGameObject);
            //Debug.Log("Options Window Closed");
            instructionsWindowOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Enable player movement
            fpc_WPIDEMO.playerCanMove = true;
            canShoot = true;
        }
    }
}
