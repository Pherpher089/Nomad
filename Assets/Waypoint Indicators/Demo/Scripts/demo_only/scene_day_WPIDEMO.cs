using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class scene_day_WPIDEMO : MonoBehaviour
{
    //This script manages:
    //Spawning mechanic

    //Root Canvas Vars
    /*
    public Canvas mainCanvas;
    public GameObject titleScreen;
    private bool instructionsWindowOpen;
    private GameObject instructionsWindowGameObject;
    private RectTransform instructionsWindowRect;
    public static bool canShoot;
    public static int spawnCount = 0;
    public static int spawnCountMax = 500;
    */



    //Scene Vars
    public float spawnSpeed = 5f;
    public Canvas levelCanvas;
    public TextMeshProUGUI spawnCountTextField;
    public TextMeshProUGUI spawnDescTextField;
    public TextMeshProUGUI wpDesc;
    private Transform spawnPos;
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
    private GameObject btn_Day;
    private GameObject btn_Day_Selected;
    private GameObject btn_Night;
    private GameObject btn_Night_Selected;


    public Material skybox;


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
            event_manager_WPIDEMO.StartLevel(); //Tell eveyone the level has started

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

            spawnCountTextField.color = new Color32(0, 0, 0, 255);
            spawnDescTextField.color = new Color32(0, 0, 0, 255);
            wpDesc.color = new Color32(0, 0, 0, 255);

            //mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            //The player lives on the root level, but we need to target the spawnPos in it for Game Object shooting
            spawnPos = GameObject.Find("Player").transform.GetChild(0).transform;

            shapeSelectRect = shapeSelectGameObject.GetComponent<RectTransform>();
            shapeSelectImg = shapeSelectRect.GetComponent<Image>();
            //Cursor.lockState = CursorLockMode.Locked;

            spawnCountTextField.text = "0";
            spawnDescTextField.text = "Waypoints";

            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Scene_" + scene_manager_WPIDEMO.curLevel));
        }


    }

    void Update()
    {
        //Cycle through Game Object types
        if (Input.GetKeyDown("q") || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            exampleNum--;
            if (exampleNum == 0)
            {
                exampleNum = totalExamples;
            }
        }

        if (Input.GetKeyDown("e") || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            exampleNum++;
            if (exampleNum == totalExamples + 1)
            {
                exampleNum = 1;
            }
        }

        if (scene_manager_WPIDEMO.curLevel != 0)
        {
            switch (exampleNum)
            {
                case 1:
                    shapeSelectImg.sprite = slot01;
                    shapeName = "1 Centered";
                    wpDesc.text = "Centered";
                    break;
                case 2:
                    shapeSelectImg.sprite = slot02;
                    shapeName = "2 Standard";
                    wpDesc.text = "Standard";
                    break;
                case 3:
                    shapeSelectImg.sprite = slot03;
                    shapeName = "3 Destination";
                    wpDesc.text = "Destination";
                    break;
                case 4:
                    shapeSelectImg.sprite = slot04;
                    shapeName = "4 Interaction";
                    wpDesc.text = "Interaction";
                    break;
                case 5:
                    shapeSelectImg.sprite = slot05;
                    shapeName = "5 Text & Dialog";
                    wpDesc.text = "Text & Dialog";
                    break;
            }
        }


        //SHOOT
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown("space")) //Shoot primary weapon
        {
            if (scene_manager_WPIDEMO.canShoot)
            {
                if (scene_manager_WPIDEMO.spawnCount < scene_manager_WPIDEMO.spawnCountMax)
                {
                    Shoot(shapeName);
                    scene_manager_WPIDEMO.spawnCount++;
                }
            }

        }

        //UPDATE TEXT
        if (scene_manager_WPIDEMO.spawnCount < scene_manager_WPIDEMO.spawnCountMax)
        {
            spawnCountTextField.text = scene_manager_WPIDEMO.spawnCount.ToString();
            spawnDescTextField.text = "Waypoints";
        }
        else
        {
            spawnCountTextField.text = "MAX";
            spawnDescTextField.text = "Press [R] to reset";
        }

    }



    void Shoot(string str)
    {
        newWaypoint = Instantiate(Resources.Load("Day/" + str, typeof(GameObject)), spawnPos.position, Quaternion.identity) as GameObject;
        newWaypoint.name = str;
        newWaypointRB = newWaypoint.AddComponent<Rigidbody>();
        newWaypointRB = newWaypoint.GetComponent<Rigidbody>();
        newWaypointRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        newWaypointRB.AddForce(spawnPos.forward * spawnSpeed, ForceMode.Impulse); //give velocity forward
        Destroy(newWaypointRB.GetComponent<Rigidbody>(), 1f);
    }


}
