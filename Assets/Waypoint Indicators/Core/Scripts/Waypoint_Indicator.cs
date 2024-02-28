using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Waypoint_Indicator : MonoBehaviour
{
    //Version 1.4.6
    //1. Fixed Min Display Range offset between Standard and Centered Tracking
    //2. Added experimental split screen support

    #region Variables
    //Make this public to allow for external scripts to send Text Description copy here as "description"
    private bool isDynamic = false;


    #region CANVAS - By default, be sure to tag your Canvas as "Canvas" for the script to see it
    //!! Make sure your Canvas > Reference Resolution is the same aspect ratio as your Play Screen or else Waypoint boundary edges may not align exactly
    private Canvas mainCanvas;
    private RectTransform mainCanvasRect;
    private GameObject[] mainCanvasObjs;
    private GameObject mainCanvasObj;
    public string canvas_tag_name = "Canvas";
    private float canvasScaleFactor;
    private Vector2 canvasRefRez;
    //public string[] canvas_tag_name_array;
    #endregion


    #region CAMERA - By default, be sure to tag your Camera as "MainCamera" for the script to see it
    private Camera mainCamera;
    private GameObject[] mainCameraObjs; //This is temporarily used to see if more than one game object is taggd with this name
    private GameObject mainCameraObj;
    public string camera_tag_name = "MainCamera";
    //public string[] camera_tag_name_array = new string[] { "MainCamera" };
    public bool multiCam;
    #endregion


    #region DISTANCE CALCULATION
    private GameObject[] distCalTargets; //This is temporarily used to see if more than one game object is taggd with this name
    private GameObject distCalTarget; //The GAME OBJECT this game object uses to calculate the distance from (defualt is camera)
    public string distCalTargetTag = "MainCamera"; //The GAME OBJECT TAG NAME this game object uses to calculate the distance from (defualt is camera)
    #endregion


    #region Tracking Types: Standard / Ceneterd
    [Header("Tracking Types")]

    [Space(10)]
    [Header("*** Custom Editor Missing! See documentation. ***")]
    [Space(10)]

    public bool enableStandardTracking = false; //Tracks over objects in view and to the edge of the screen when out of view.
    public bool enableCenteredTracking = false; //Tracks objects along a circular perimeter originating from the center of the screen.

    [Header("Parent")]
    public bool showBoundaryBox = true;
    public Color boundaryBoxColor = new Color32(255, 255, 102, 255);
    public Vector2Int parentSize = new Vector2Int(150, 150);
    private float parentPaddingX;
    private float parentPaddingY;
    private float onScreenSnapOffset_X;
    private float onScreenSnapOffset_Y;
    public float displayRangeMin = 0f;
    public float displayRangeMax = 500f;
    private float displayRangeDifference;
    public bool raycastTarget = false; //Check TRUE if you want this element to block mouse clicks



    [Header("Sprite Global")]
    public bool enableSprite = false;
    public int spriteDepth;
    public bool offScreenSpriteRotates = false;

    [Header("Sprite On-Screen")]
    //ON-SCREEN SPRITE
    public Sprite onScreenSprite;
    public Color onScreenSpriteColor = new Color32(255, 255, 255, 255);
    public float onScreenSpriteSize = 1f;
    public Vector2 onScreenSpriteOffset = new Vector2(0f, 0f);
    [Range(0, 360)]
    public float onScreenSpriteRotation = 0f;
    public bool onScreenSpriteFadeWithRange;
    public bool onScreenSpriteScaleWithRange;
    public bool reverseOnScreenSpriteScaleWithRange;
    public bool onScreenSpriteHide = false;


    [Header("Sprite Off-Screen")]
    //OFF-SCREEN SPRITE
    public Sprite offScreenSprite;
    public Color offScreenSpriteColor = new Color32(255, 255, 255, 255);
    public float offScreenSpriteSize = 1f;
    public Vector2 offScreenSpriteOffset = new Vector2(0f, 0f);
    [Range(0, 360)]
    public float offScreenSpriteRotation = 0f;
    public bool offScreenSpriteFadeWithRange;
    public bool offScreenScaleWithRange;
    public bool reverseOffScreenSpriteScaleWithRange;
    public bool offScreenSpriteHide = false;




    [Header("Game Object Global")]
    public bool enableGameObject = false;
    public int gameObjectDepth;
    public bool offScreenObjectRotates = false;

    //ON-SCREEN GAME OBJECT
    [Header("Object On-Screen")]
    public GameObject onScreenGameObject;
    public Color onScreenGameObjectColor = new Color32(255, 255, 255, 255);
    public float onScreenGameObjectSize = 1f;
    public Vector2 onScreenGameObjectOffset = new Vector2(0f, 0f);
    [Range(0, 360)]
    public float onScreenGameObjectRotation = 0f;
    public bool onScreenGameObjectFadeWithRange;
    public bool onScreenGameObjectScaleWithRange;
    public bool reverseOnScreenGameObjectScaleWithRange;
    public bool onScreenGameObjectHide = false;

    //OFF-SCREEN GAME OBJECT
    [Header("Object Off-Screen")]
    public GameObject offScreenGameObject;
    public Color offScreenGameObjectColor = new Color32(255, 255, 255, 255);
    public float offScreenGameObjectSize = 1f;
    public Vector2 offScreenGameObjectOffset = new Vector2(0f, 0f);
    [Range(0, 360)]
    public float offScreenGameObjectRotation = 0f;
    public bool offScreenGameObjectFadeWithRange;
    public bool offScreenGameObjectScaleWithRange;
    public bool reverseOffScreenGameObjectScaleWithRange;
    public bool offScreenGameObjectHide = false;



    [Header("Text Global")]
    public bool enableText = true;
    public int textDepth;
    public string textDescription = "Hello!";
    public string distIncrement = "m";
    public TMP_FontAsset textFont;
    public static string description; //Text pulled from Quest scripts (if marked as dynamic)
    public float textSize = 40;
    public Color textColor = new Color32(0, 0, 0, 255);
    public enum textAlignValue
    {
        Left,
        Center,
        Right
    }
    public textAlignValue textAlign = textAlignValue.Center;
    public float textLineSpacing = 20f;
    public Vector2 edgeDetectOffset = new Vector2(0f, 0f);

    [Header("Text On-Screen")]
    public bool onScreenSpriteHideDesc = false;
    public bool onScreenSpriteHideDist = false;
    public Vector2 onScreenTextOffset = new Vector2(0f, 0f);

    [Header("Text Off-Screen")]
    public bool offScreenSpriteHideDesc = false;
    public bool offScreenSpriteHideDist = false;
    public Vector2 offScreenTextOffset = new Vector2(0f, 0f);



    //CENTERED
    [Header("Diameter")]
    public bool showDiameter = false;
    public Color diameterColor = new Color32(0, 0, 0, 255);
    public float diameterSize = 50f;
    public float onScreenCenteredRangeMin = 0f;
    public float onScreenCenteredRangeMax = 50f;
    private float onScreenCenteredDisplayRangeDifference;
    public bool raycastTargetCentered = false; //Check TRUE if you want this element to block mouse clicks


    [Header("Centered Sprite On-Screen")]
    public bool enableCenteredSprite;
    public int onScreenCenteredSpriteDepth;
    public Sprite onScreenCenteredSprite;
    public Color onScreenCenteredSpriteColor = new Color32(255, 255, 255, 255);
    public float onScreenCenteredSpriteSize = 1f;
    [Range(0, 360)]
    public float onScreenCenteredSpriteRotation = 0f;
    public bool onScreenSpriteCenteredFadeWithRange;
    public bool onScreenSpriteCenteredScaleWithRange;
    public bool onScreenSpriteCenteredScaleReverse;
    public bool hideOnScreenCenteredSprite;
    public bool hideOffScreenCenteredSprite;


    [Header("Centered Prefab On-Screen")]
    public bool enableCenteredPrefab;
    public int onScreenCenteredPrefabDepth;
    public GameObject onScreenCenteredPrefab;
    public Color onScreenCenteredPrefabColor = new Color32(255, 255, 255, 255);
    public float onScreenCenteredPrefabSize = 1f;
    [Range(0, 360)]
    public float onScreenCenteredPrefabRotation = 0f;
    public bool onScreenPrefabCenteredFadeWithRange;
    public bool onScreenPrefabCenteredScaleWithRange;
    public bool onScreenPrefabCenteredScaleReverse;
    public bool hideOnScreenCenteredPrefab;
    public bool hideOffScreenCenteredPrefab;


    [Header("Radius")]
    public bool enableRadiusGizmo; //A circular wireframe tool for aligning crescent-shaped icons.  Scene View only. Must have Gizmos and 2D enabled.
    public Color radiusGizmoColor = new Color32(0, 0, 0, 255);
    public float radiusGizmoSize = 50f;



    //Custom Editor Drawer Toggle States
    [Header("Ignore (For use with Custom Editor)")]
    public bool toggleSetupOptions = true;
    public bool toggleParentOptions = false;
    public bool toggleSpriteOptions = false;
    public bool toggleGameObjectOptions = false;
    public bool toggleTextOptions = false;
    public bool toggleDiameterOptions = false;
    public bool toggleCenteredSpriteOptions = false;
    public bool toggleCenteredPrefabOptions = false;
    public bool toggleRadiusGizmoOptions = false;
    #endregion


    #region Setup Vars Standard Tracking
    private Vector3 screenCenter;
    private Vector3 wpParentPos;
    private Vector3 objectInWorldScreenPos; //This is the WorldToScreen x,y,z of the object this script sits on (Standard Tracking)
    private float angle;
    private float waypointDist;
    private int waypointDistInt;
    private int iSpriteIndicator = 0;
    private int iGameObjectIndicator = 0;
    private int iCenteredSpriteIndicator = 0;
    private int iCenteredPrafabIndicator = 0;
    private int iText = 0;
    private int iScreenCheck;
    private int iStandardTrackingEnabled;
    private bool parentOnScreen;
    private float iconAlphaValue;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    #endregion


    #region Setup Vars Centered Tracking
    private Vector2 onScreenCenteredSpriteOriginalSize;
    private Vector2 newOnScreenCenteredSize;
    //private Vector2 onScreenCenteredPrefabOriginalSize;
    private Vector2 newOnScreenPrefabCenteredSize;
    private Vector3 screenCenteredCenter;
    private Vector3 wpCenteredParentPos;
    private Color iconCenteredColor;
    private Color prefabCenteredColor;
    private float waypointCenteredDist;
    //private int iScreenCheckCentered; //Used as a toggle
    private int iCenteredTrackingEnabled;
    private bool centeredParentOnScreen;
    private float iconCenteredAlphaValue;
    private float prefabCenteredAlphaValue;
    private float centeredScaleValueX;
    private float centeredScaleValueY;
    #endregion


    #region  Instantiation Functions - Standard Tracking
    //WP Parent Vars
    private GameObject wpParentGameObject;
    private RectTransform wpParentRectTransform;
    private Image wpParentImage;


    //WP Sprite Indicator Vars
    private GameObject spriteIndicator; //spriteIndicator
    private RectTransform spriteIndicatorRect; //spriteIndicatorRect
    private Image spriteIndicatorImage; //spriteIndicatorImage
    private Color spriteIndicatorColor;
    private bool spriteIndicatorCreated = false;
    private Vector2 onScreenSpriteOriginalSize;
    private Vector2 offScreenSpriteOriginalSize;
    private Vector2 newOnScreenSize;
    private Vector2 newOffScreenSize;
    private float Sprite_Z_Scale = 1f; //Canvas > Pixel Perfect will not show if 0f
    private float scaleValueX; //Scale Sprite with Range
    private float scaleValueY; //Scale Sprite with Range


    //WP Game Object Indicator Vars (Prefab)
    private GameObject gameObjectIndicator; //This is the Game Object version of the Sprite
    private bool gameObjectIndicatorCreated = false;
    private int gameObjectIndicatorOnOffScreenStatus = 0; //0 on screen, 1 off screen
    private CanvasGroup gameObjectIndicatorCanvasGroup;
    private GameObject gameObjectIndicatorChildGameObject;
    private RectTransform gameObjectIndicatorChildRect;
    private RawImage gameObjectIndicatorChildImg; //The image that controls the sprite that sits inside the game object (index 0)
    private Color gameObjectIndicatorChildIndicatorColor;
    private float Prefab_Z_Scale = 1f; //Canvas > Pixel Perfect will not show if 0f
    private float scaleValueGameObjectX; //Scale Prefab with Range
    private float scaleValueGameObjectY; //Scale Prefab with Range


    //WP Text Vars
    private GameObject textGameObject;
    private TextMeshProUGUI textField;
    private ContentSizeFitter textContentSizeFitter;
    private bool waypointTextCreated = false;
    #endregion


    #region  Instantiation Functions - Centered Tracking
    //Centered Parent Vars
    private GameObject wpCenteredParentGameObject;
    private RectTransform wpCenteredParentRectTransform;
    private Image wpCenteredParentImage;
    private float cosCentered;
    private float sinCentered;
    private float mCentered;
    private Vector3 screenBoundsCentered;
    private float angleCentered;
    private Vector3 objectInWorldScreenPosCentered;
    private Vector3 objectWorldToViewportPosCentered;

    //Centered Sprite Vars
    private GameObject iconCenteredGameObject;
    private RectTransform iconCentered;
    private Image iconCenteredImage;
    private bool waypointIconCenteredCreated = false;

    //Centered Prefab Vars
    private GameObject centeredPrefabIndicator; //This is the Game Object version of the Sprite
    private CanvasGroup centeredPrefabIndicatorCanvasGroup;
    private GameObject centeredPrefabIndicatorChildGameObject;
    private RectTransform centeredPrefabIndicatorChildRect;
    private RawImage centeredPrefabIndicatorChildImg; //The image that controls the sprite that sits inside the game object (index 0)
    private Color centeredPrefabIndicatorChildIndicatorColor;
    private bool centeredPrefabIndicatorCreated = false;
    #endregion


    #region Other Vars
    //Declaration Vars
    private bool distanceTargetDefined;

    //Screen Edge Detect Bool
    private bool topEdgeDetected;
    private bool botEdgeDetected;
    private bool rightEdgeDetected;
    private bool leftEdgeDetected;

    //Math Vars
    private float cos;
    private float sin;
    private float m;
    private Vector3 screenBounds;

    //Screen Space-Camera Vars
    private Vector3 objectInViewportPos;
    private Vector3 proportionalPosition;
    private Vector3 uiOffset;

    private Vector3 objectInViewportPosCentered;
    private Vector3 proportionalPositionCentered;
    private Vector3 uiOffsetCentered;
    #endregion


    #region Checks to see if the Tag name exists in the Project List
    bool DoesTagExist(string SomeTag)
    {
        try
        {
            GameObject.FindGameObjectWithTag(SomeTag);
            return true;
        }
        catch
        {
            return false;
        }
    }

    bool doesCamTagExist;
    bool doesCanvasTagExist;
    #endregion
    #endregion


    #region Check for Camera and Canvas Functions
    void CheckForCamera()
    {
        #region Check for Camera Tags
        //Check to see if the Tag Name is blank
        if (camera_tag_name == "" || camera_tag_name == null || camera_tag_name == " ")
        {
            //Tag name is blank please add a tag
            if (!multiCam)
            {
                Debug.LogError("Camera tag name field is blank! See waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\"");
            }

            mainCamera = null;
        }
        else //Tag name has a value now lets check to see if that value exsists as a tag somewhere
        {
            if (camera_tag_name == canvas_tag_name)
            {
                Debug.Log("Tag Name Conflict! You cannot name your Canvas and your Camera with the same tag name.");
                mainCamera = null;
            }
            else
            {
                mainCameraObjs = GameObject.FindGameObjectsWithTag(camera_tag_name);
                if (mainCameraObjs.Length == 1)
                {
                    //We have a match and all is good!
                    //Debug.Log("We have a match and all is good!");
                    mainCameraObj = GameObject.FindGameObjectWithTag(camera_tag_name);
                    mainCamera = mainCameraObj.GetComponent<Camera>();
                }
                if (mainCameraObjs.Length > 1)
                {
                    //There are more than one object of this tag name and here is a list (please make sure just one is labeled)
                    Debug.LogError("The Camera tag name \"" + camera_tag_name + "\" on waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\" is already being used in your scene " + mainCameraObjs.Length + " times: (see below)");
                    foreach (GameObject mainCameraObj in mainCameraObjs)
                    {
                        Debug.LogError("\nGame Object: " + mainCameraObj.name + " - Tag: " + camera_tag_name);
                    }
                    mainCamera = null;
                }
                if (mainCameraObjs.Length < 1)
                {
                    //There are no objects in the scene that match this tag, please add one in
                    if (!multiCam)
                    {
                        Debug.LogError("You tagged a Camera \"" + camera_tag_name + "\" in your scene, but I can't find it! See waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\"");
                    }
                    mainCamera = null;
                }
            }
        }
        #endregion
    }

    void CheckForCanvas()
    {
        #region Check for Canvas Tags
        //Make sure th Tag Name is blank
        if (canvas_tag_name == "" || canvas_tag_name == null || canvas_tag_name == " ")
        {
            //Tag name is blank please add a tag
            Debug.LogError("Canvas tag name field is blank! See waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\"");
            mainCanvas = null;

        }
        else //Tag name has a value now lets check to see if that value exsists as a tag somewhere
        {
            if (canvas_tag_name == camera_tag_name)
            {
                //No need to display this erorr twice as it is being handled by the Camera Setup
                //Debug.Log("Tag Name Conflict! You cannot name your Canvas and your Camera with the same tag name.");
                mainCanvas = null;
            }
            else
            {
                mainCanvasObjs = GameObject.FindGameObjectsWithTag(canvas_tag_name);

                if (mainCanvasObjs.Length == 1)
                {
                    //We have a match and all is good!
                    //Debug.Log("We have a match and all is good!");
                    mainCanvasObj = GameObject.FindGameObjectWithTag(canvas_tag_name);
                    mainCanvas = mainCanvasObj.GetComponent<Canvas>();
                    mainCanvasRect = mainCanvas.GetComponent<RectTransform>();
                }
                if (mainCanvasObjs.Length > 1)
                {
                    //There are more than one object of this tag name and here is a list (please make sure just one is labeled)
                    Debug.LogError("The Canvas tag name \"" + canvas_tag_name + "\" on waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\" is already being used in your scene " + mainCanvasObjs.Length + " times: (see below)");
                    foreach (GameObject mainCanvasObj in mainCanvasObjs)
                    {
                        Debug.LogError("\nGame Object: " + mainCanvasObj.name + " - Tag: " + canvas_tag_name);
                    }
                    mainCanvas = null;
                }
                if (mainCanvasObjs.Length < 1)
                {
                    //There are no objects in the scene that match this tag, please add one in
                    Debug.LogError("You tagged a Canvas \"" + canvas_tag_name + "\" in your scene, but I can't find it! See waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\"");
                    mainCanvas = null;
                }
            }
        }
        #endregion
    }
    #endregion



    #region WPI_Manager Functions

    //Multi-Cam - Reset Camera tag for camera switching
    void SwitchCams(string newCameraTag, string newDistCalTargetTag)
    {
        camera_tag_name = newCameraTag;
        distCalTargetTag = newDistCalTargetTag;
        distanceTargetDefined = false;
        CheckForCamera();

    }

    //Toggle Visibility - Enable/Disable all active waypoints in scene at once
    void ToggleVisibility()
    {
        if (WPI_Manager.waypoint_indicators_are_visible)
        {
            if (enableStandardTracking)
            {
                wpParentGameObject.SetActive(true);
            }

            if (enableCenteredTracking)
            {
                wpCenteredParentGameObject.SetActive(true);
            }
        }
        else
        {
            if (enableStandardTracking)
            {
                wpParentGameObject.SetActive(false);
            }

            if (enableCenteredTracking)
            {
                wpCenteredParentGameObject.SetActive(false);
            }
        }
    }
    #endregion



    void Awake()
    {
        CheckForCamera();
        CheckForCanvas();


        if (mainCamera && mainCanvas)
        {
            //Standard Tracking
            if (enableStandardTracking)
            {
                InstantiateWaypointParent();

                if (textDescription == "")
                {
                    textDescription = gameObject.name;
                    //Debug.Log("There's no text");
                }

                iSpriteIndicator = 0;
                iGameObjectIndicator = 0;
                iText = 0;

                #region Fill missing fields for Sprite and Game Objects if left empty to avoid errors

                //SPRITE CHECK
                if (onScreenSprite != null || offScreenSprite != null) //At least one icon was defined for one slot
                {
                    //Check to see if either on or off-screen has been left empty and assign the sprite from the state that has been defined 
                    if (onScreenSprite == null)
                    {
                        onScreenSprite = offScreenSprite;
                    }
                    if (offScreenSprite == null)
                    {
                        offScreenSprite = onScreenSprite;
                    }

                    //Get the original size of the Sprite before alterations
                    onScreenSpriteOriginalSize.x = onScreenSprite.bounds.size.x;
                    onScreenSpriteOriginalSize.y = onScreenSprite.bounds.size.y;

                    offScreenSpriteOriginalSize.x = offScreenSprite.bounds.size.x;
                    offScreenSpriteOriginalSize.y = offScreenSprite.bounds.size.y;
                }
                else
                {
                    if (enableSprite)
                    {
                        Debug.LogWarning("Sprite Indicators are ENABLED, but NOT ASSIGNED on: " + gameObject.name + "\nAssign a Sprite to either On/Off Screen Sprite fields. - OnEnable()");
                    }
                    //Since there's no Sprite, we need to manually enter X,Y sizes so there's no error
                    onScreenSpriteOriginalSize.x = 1f;
                    onScreenSpriteOriginalSize.y = 1f;

                    offScreenSpriteOriginalSize.x = 1f;
                    offScreenSpriteOriginalSize.y = 1f;
                }


                //GAME OBJECT TYPE
                if (onScreenGameObject != null || offScreenGameObject != null) //At least one icon was defined for one slot
                {
                    //Check to see if either on or off-screen has been left empty and assign the sprite from the state that has been defined 
                    if (onScreenGameObject == null)
                    {
                        onScreenGameObject = offScreenGameObject;
                    }
                    if (offScreenGameObject == null)
                    {
                        offScreenGameObject = onScreenGameObject;
                    }
                }
                else
                {
                    if (enableGameObject)
                    {
                        Debug.LogWarning("Game Object Indicators are ENABLED, but NOT ASSIGNED on: " + gameObject.name + "\nAssign a Game Object to either On/Off Screen Game Object fields.");
                    }
                }

                #endregion


                //Check if parent is on or off-screen then set the game object status accordingly
                //This ensures the right Game Object indicator shows for on or off screen when enabled and disabled during run time
                SetGameObjectIndicatorStatus();

                iStandardTrackingEnabled = 1;
            }


            //Centered Tracking
            if (enableCenteredTracking)
            {
                InstantiateCenteredWaypointParent();
                //InstantiateCenteredWaypointIcon();


                #region Fill missing fields for Centered Sprite if left empty to avoid errors

                //CENTERED SPRITE CHECK
                if (onScreenCenteredSprite != null) //At least one icon was defined for one slot
                {
                    //Get the original size of the Sprite before alterations
                    onScreenCenteredSpriteOriginalSize.x = onScreenCenteredSprite.bounds.size.x;
                    onScreenCenteredSpriteOriginalSize.y = onScreenCenteredSprite.bounds.size.y;
                }
                else
                {
                    Debug.LogWarning("Centered Tracking Sprite Missing on: " + gameObject.name + "\nAssign a Sprite to the Sprite field. - OnEnable()");

                    //Get the original size of the Sprite before alterations
                    onScreenCenteredSpriteOriginalSize.x = 1f;
                    onScreenCenteredSpriteOriginalSize.y = 1f;
                }

                #endregion


                iCenteredTrackingEnabled = 1;
            }


            canvasRefRez = mainCanvas.GetComponent<CanvasScaler>().referenceResolution;
        }

    }

    void OnEnable()
    {
        #region Add Delagate listener from WPI Manager Script
        //Multi-Cam
        WPI_Manager.onSwitchCams += SwitchCams;
        //Toggle Visibility
        WPI_Manager.onToggleVisibility += ToggleVisibility;
        #endregion
    }


    void OnDisable()
    {
        #region Multi-Cam - Remove Delagate listener from WPI Manager Script
        //Multi-Cam
        WPI_Manager.onSwitchCams -= SwitchCams;
        //Toggle Visibility
        WPI_Manager.onToggleVisibility -= ToggleVisibility;
        #endregion

        if (enableStandardTracking)
        {
            spriteIndicatorCreated = false;
            gameObjectIndicatorCreated = false;
            waypointTextCreated = false;


            //Destroy wp ui using find from OnEnable() above
            if (wpParentGameObject != null)
            {
                //Destroys ENABLED WP Indicator from Canvas
                Destroy(wpParentGameObject);
            }
            else
            {
                if (mainCanvas != null)
                {
                    //Toggle Visibility
                    //Destroys DISABLED WP Indicators from Canvas
                    //This is needed if ToggleVisibility() is being called and the Indicator is disabled/turned off
                    //Because GameObject.Find() won't work on disabled objects, we use mainCanvas.transform.Find()
                    Destroy(mainCanvas.transform.Find(gameObject.name + "-WP-" + GetInstanceID()).gameObject);
                }
            }


            iStandardTrackingEnabled = 0;
        }

        if (enableCenteredTracking)
        {
            iCenteredSpriteIndicator = 0;
            iCenteredPrafabIndicator = 0;
            waypointIconCenteredCreated = false;
            enableRadiusGizmo = false;

            //Destroy wp ui using find from OnEnable() above
            if (wpCenteredParentGameObject != null)
            {
                Destroy(wpCenteredParentGameObject);
            }
            else
            {
                if (mainCanvas != null)
                {
                    //Toggle Visibility
                    //Destroys DISABLED WPC Indicators from Canvas
                    //This is needed if ToggleVisibility() is being called and the Indicator is disabled/turned off
                    //Because GameObject.Find() won't work on disabled objects, we use mainCanvas.transform.Find()
                    Destroy(mainCanvas.transform.Find(gameObject.name + "-WPC-" + GetInstanceID()).gameObject);
                }
            }

            iCenteredTrackingEnabled = 0;
        }

        mainCamera = null;
        mainCanvas = null;
    }



    // Update is called once per frame
    void Update()
    {

        if (mainCamera && mainCanvas)
        {
            if (canvasRefRez.x == 0 || canvasRefRez.y == 0)
            {
                canvasRefRez = mainCanvas.GetComponent<CanvasScaler>().referenceResolution;
            }


            #region ENABLE STANDARD TRACKING
            if (enableStandardTracking)
            {
                //Reset Standard Tracking as if it were Enable()
                if (iStandardTrackingEnabled == 0)
                {
                    //Run functions from Enable()
                    InstantiateWaypointParent();

                    if (textDescription == "")
                    {
                        textDescription = gameObject.name;
                        //Debug.Log("There's no text");
                    }

                    iSpriteIndicator = 0;
                    iGameObjectIndicator = 0;
                    iText = 0;


                    #region Fill missing fields for Sprite and Game Objects if left empty to avoid errors

                    //SPRITE TYPE
                    if (onScreenSprite != null || offScreenSprite != null) //At least one icon was defined for one slot
                    {
                        //Check to see if either on or off-screen has been left empty and assign the sprite from the state that has been defined 
                        if (onScreenSprite == null)
                        {
                            onScreenSprite = offScreenSprite;
                        }
                        if (offScreenSprite == null)
                        {
                            offScreenSprite = onScreenSprite;
                        }

                        //Get the original size of the Sprite before alterations
                        onScreenSpriteOriginalSize.x = onScreenSprite.bounds.size.x;
                        onScreenSpriteOriginalSize.y = onScreenSprite.bounds.size.y;

                        offScreenSpriteOriginalSize.x = offScreenSprite.bounds.size.x;
                        offScreenSpriteOriginalSize.y = offScreenSprite.bounds.size.y;
                    }
                    else
                    {
                        if (enableSprite)
                        {
                            Debug.LogWarning("Sprite Indicators are ENABLED, but NOT ASSIGNED on: " + gameObject.name + "\nAssign a Sprite to either On/Off Screen Sprite fields. - void Update()");
                        }
                        //Get the original size of the Sprite before alterations
                        onScreenSpriteOriginalSize.x = 1f;
                        onScreenSpriteOriginalSize.y = 1f;

                        offScreenSpriteOriginalSize.x = 1f;
                        offScreenSpriteOriginalSize.y = 1f;
                    }


                    //GAME OBJECT TYPE
                    if (onScreenGameObject != null || offScreenGameObject != null) //At least one icon was defined for one slot
                    {
                        //Check to see if either on or off-screen has been left empty and assign the sprite from the state that has been defined 
                        if (onScreenGameObject == null)
                        {
                            onScreenGameObject = offScreenGameObject;
                        }
                        if (offScreenGameObject == null)
                        {
                            offScreenGameObject = onScreenGameObject;
                        }
                    }
                    else
                    {
                        if (enableGameObject)
                        {
                            Debug.LogWarning("Game Object Indicators are ENABLED, but NOT ASSIGNED on: " + gameObject.name + "\nAssign a Game Object to either On/Off Screen Game Object fields.");
                        }
                    }

                    #endregion


                    //Check if parent is on or off-screen then set the game object status accordingly
                    //This ensures the right Game Object indicator shows for on or off screen when enabled and disabled during run time
                    SetGameObjectIndicatorStatus();

                    iStandardTrackingEnabled = 1;
                }


                //If true, other scripts can send text description copy via: WaypointIndicator.description = "My Text";
                if (isDynamic)
                {
                    textDescription = description;
                }


                #region CHECK TOGGLE STATES
                //SPRITE INDICATOR
                if (enableSprite) //ENABLE
                {
                    if (iSpriteIndicator == 0)
                    {
                        //Debug.Log("Display Sprite Ind");
                        InstantiateWaypointIcon();
                        iSpriteIndicator++;
                    }
                }
                if (!enableSprite) //DISABLE
                {
                    if (iSpriteIndicator == 1)
                    {
                        //Debug.Log("Hide Sprite Ind");
                        DestroyWaypointIcon();
                        iSpriteIndicator--;
                    }
                }


                //GAME OBJECT INDICATOR
                if (enableGameObject) //ENABLE
                {
                    if (iGameObjectIndicator == 0)
                    {
                        //Debug.Log("Display Game Object Ind");
                        SetGameObjectIndicatorStatus();
                        InstantiateWaypointGameObject();
                        iGameObjectIndicator++;
                    }
                }
                if (!enableGameObject) //DISABLE
                {
                    if (iGameObjectIndicator == 1)
                    {
                        //Debug.Log("Hide Game Object Ind");
                        DestroyWaypointGameObject();
                        iGameObjectIndicator--;
                    }
                }


                //TEXT
                if (enableText) //ENABLE
                {
                    if (iText == 0)
                    {
                        //Debug.Log("Display Text Field");
                        InstantiateWaypointText();
                        iText++;
                    }
                }
                if (!enableText) //DISABLE
                {
                    if (iText == 1)
                    {
                        //Debug.Log("Hide Text Field");
                        DestroyWaypointText();
                        iText--;
                    }
                }
                #endregion



                //Determine Distance from this Game Object to whatever the user has tagged "Dist Target Tag Name" in the inspector
                CalculateDistance();
                waypointDist = Vector3.Distance(distCalTarget.transform.position, transform.position);
                waypointDistInt = (int)waypointDist; //We convert this for the text readout so users dont see: Distance 23.547893m


                #region IF ENABLE PARENT (enabled on Start)
                //Make sure there is at least one sprite assigned to avoid nasty errors 
                if (wpParentGameObject && wpParentRectTransform != null)
                {
                    //Set Raycast Target
                    if (raycastTarget)
                    {
                        wpParentImage.raycastTarget = true;
                    }
                    else
                    {
                        wpParentImage.raycastTarget = false;
                    }

                    //Reset Scale to 1
                    wpParentRectTransform.localScale = new Vector3(1f, 1f, 1f);

                    //Show/Hide
                    if (showBoundaryBox)
                    {
                        wpParentImage.enabled = true;
                    }
                    else //Hide boundary box
                    {
                        wpParentImage.enabled = false;
                    }

                    //Match the PARENT UI pos to 3D MESH position (the object this script is attached to) via WorldToScreenPoint
                    if (mainCamera != null)
                    {

                        #region Screen Space Check
                        switch (mainCanvas.renderMode)
                        {
                            #region Overlay
                            case RenderMode.ScreenSpaceOverlay:
                                wpParentPos = mainCamera.WorldToScreenPoint(transform.position);


                                //This keeps wpParentPos.z value from going above 1 causing dissapearing if above 1000 or so
                                if (wpParentPos.z > 1)
                                {
                                    wpParentPos.z = 1f;
                                }
                                else
                                {
                                    wpParentPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
                                }
                                break;
                            #endregion

                            #region Camera
                            case RenderMode.ScreenSpaceCamera:
                                objectInWorldScreenPos = mainCamera.WorldToScreenPoint(transform.position); //0,0 bottom left | 0,1 top left | 1,1 top right | 1,0 bottom right
                                wpParentRectTransform.position = mainCamera.ScreenToWorldPoint(new Vector3(objectInWorldScreenPos.x, objectInWorldScreenPos.y, mainCanvas.planeDistance));
                                break;
                            #endregion

                            #region World
                            case RenderMode.WorldSpace:
                                uiOffset = new Vector3((float)mainCanvasRect.sizeDelta.x / 2f, (float)mainCanvasRect.sizeDelta.y / 2f, 0);

                                objectInViewportPos = mainCamera.WorldToViewportPoint(transform.position);
                                proportionalPosition = new Vector3(objectInViewportPos.x * mainCanvasRect.sizeDelta.x, objectInViewportPos.y * mainCanvasRect.sizeDelta.y, 0);

                                // Set the position and remove the screen offset
                                wpParentRectTransform.localPosition = proportionalPosition - uiOffset;

                                break;
                                #endregion
                        }
                        #endregion

                        canvasScaleFactor = mainCanvas.scaleFactor;
                    }


                    #region Screen Space Check
                    switch (mainCanvas.renderMode)
                    {
                        #region Overlay
                        case RenderMode.ScreenSpaceOverlay:
                            //ONSCREEN SNAP TO EDGE OFFSET
                            //This ensures the on screen icon boundary edge will trigger offscreen exactly when it touches screen edge
                            //Otherwise, the on screen icon will snap to edge before reaching them
                            //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align
                            onScreenSnapOffset_X = 22 - (Screen.width * .024f);
                            //onScreenSnapOffset_X = 0f;
                            //onScreenSnapOffset_Y = 22 - (Screen.height * .024f);
                            onScreenSnapOffset_Y = 0f;



                            //Onscreen
                            if (wpParentPos.z > 0f &&
                                (wpParentPos.x / canvasScaleFactor) > (wpParentRectTransform.sizeDelta.x / 2) && wpParentPos.x < (Screen.width - (wpParentRectTransform.sizeDelta.x / 2)) + onScreenSnapOffset_X &&
                                wpParentPos.y / canvasScaleFactor > (wpParentRectTransform.sizeDelta.y / 2) && wpParentPos.y < (Screen.height - (wpParentRectTransform.sizeDelta.y / 2)) + onScreenSnapOffset_Y)
                            {
                                //Debug.Log("On");
                                if (iScreenCheck == 1)
                                {
                                    //Debug.Log("On Screen");
                                    parentOnScreen = true;
                                    iScreenCheck--;
                                }

                                wpParentRectTransform.transform.position = wpParentPos;
                            }
                            else //(Offscreen)
                            {
                                //Check if the target is actually off-screen (at this point in the code, it is)
                                if (iScreenCheck == 0)
                                {
                                    //Debug.Log("Off Screen");
                                    parentOnScreen = false;
                                    iScreenCheck++;
                                }

                                //Change the coordinate space so the origin is at the center of the screen
                                screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
                                wpParentPos -= screenCenter;


                                if (wpParentPos.z < 0)
                                {
                                    //Flip coordinates when things are behind
                                    wpParentPos.x = -wpParentPos.x;
                                    wpParentPos.y = -wpParentPos.y;
                                    //Debug.Log("Behind perpindicular line!");

                                }



                                //Find angle from center of screen to mouse pos
                                angle = Mathf.Atan2(wpParentPos.y, wpParentPos.x);
                                angle -= 90f * Mathf.Deg2Rad;

                                cos = Mathf.Cos(angle);
                                sin = Mathf.Sin(angle);


                                //y = mx+b format
                                m = cos / sin;

                                screenBounds = screenCenter;


                                //Get Offscreen Padding Offset: (1 / Reference Resolution)
                                //This positions the boundary right along the edge once its gone offscreen
                                //This stops the bouncing back and passing beyond edges when icon reaches screen edge
                                //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align
                                parentPaddingX = 1 / canvasRefRez.x;
                                parentPaddingY = 1 / canvasRefRez.y;

                                screenBounds.x = screenCenter.x * (.999f - (parentSize.x * parentPaddingX));
                                screenBounds.y = screenCenter.y * (.999f - (parentSize.y * parentPaddingY));


                                //Check up and down first
                                if (cos > 0f)
                                {
                                    //up
                                    ScreenEdgeDetectTop();
                                    wpParentPos = new Vector3(-screenBounds.y / m, screenBounds.y, 0f);
                                }
                                else
                                {
                                    //down
                                    ScreenEdgeDetectBot();
                                    wpParentPos = new Vector3(screenBounds.y / m, -screenBounds.y, 0f);
                                }

                                //If out of bounds, get point on appropriate side
                                if (wpParentPos.x > screenBounds.x) //Out of bounds! Must be on the right
                                {
                                    //right
                                    ScreenEdgeDetectRight();
                                    wpParentPos = new Vector3(screenBounds.x, -screenBounds.x * m, 0f);
                                }
                                else if (wpParentPos.x < -screenBounds.x) //Out of bounds! Must be on the left
                                {
                                    //left
                                    ScreenEdgeDetectLeft();
                                    wpParentPos = new Vector3(-screenBounds.x, screenBounds.x * m, 0);
                                }


                                //Remove coordinate translation
                                wpParentPos += screenCenter;


                                wpParentRectTransform.transform.position = wpParentPos;
                                //Arrow point offscreen
                                //wpParentRectTransform.transform.localRotation = Quaternion.Euler(0f, 0f, angle*Mathf.Rad2Deg);

                            }
                            break;
                        #endregion


                        #region Camera
                        case RenderMode.ScreenSpaceCamera:

                            //ONSCREEN SNAP TO EDGE OFFSET
                            //This ensures the on screen icon boundary edge will trigger offscreen exactly when it touches screen edge
                            //Otherwise, the on screen icon will snap to edge before reaching them
                            //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align
                            onScreenSnapOffset_X = 22 - (mainCamera.pixelWidth * .024f);
                            //onScreenSnapOffset_Y = 22 - (Screen.height * .024f);
                            onScreenSnapOffset_Y = 0f;


                            if (objectInWorldScreenPos.z > 0f &&
                            (objectInWorldScreenPos.x / canvasScaleFactor) > (wpParentRectTransform.sizeDelta.x / 2) && objectInWorldScreenPos.x < (mainCamera.pixelWidth - (wpParentRectTransform.sizeDelta.x / 2)) + onScreenSnapOffset_X &&
                            objectInWorldScreenPos.y / canvasScaleFactor > (wpParentRectTransform.sizeDelta.y / 2) && objectInWorldScreenPos.y < (mainCamera.pixelHeight - (wpParentRectTransform.sizeDelta.y / 2)) + onScreenSnapOffset_Y)
                            {
                                if (iScreenCheck == 1)
                                {
                                    //Debug.Log("On Screen - Camera");
                                    parentOnScreen = true;
                                    iScreenCheck--;
                                }
                            }
                            else //Out of camera viewport
                            {
                                if (iScreenCheck == 0)
                                {
                                    //Debug.Log("Off Screen - Camera");
                                    parentOnScreen = false;
                                    iScreenCheck++;
                                }

                                //Find center of screen
                                screenCenter = new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, 0f) / 2;
                                //Set 0,0 DEAD CENTER from lower left
                                objectInWorldScreenPos -= screenCenter;


                                if (objectInWorldScreenPos.z < 0)
                                {
                                    //Flip coordinates when things are behind
                                    objectInWorldScreenPos *= -1;
                                }

                                //Find angle from center of screen to mouse pos
                                angle = Mathf.Atan2(objectInWorldScreenPos.y, objectInWorldScreenPos.x);
                                angle -= 90f * Mathf.Deg2Rad;

                                cos = Mathf.Cos(angle);
                                sin = Mathf.Sin(angle);

                                //wpParentPos = screenCenter + new Vector3(sin * 150f, cos * 150f, 0f);

                                //y = mx+b format
                                m = cos / sin;

                                screenBounds = screenCenter;


                                //Get Offscreen Padding Offset: (1 / Reference Resolution)
                                //This positions the boundary right along the edge once its gone offscreen
                                //This stops the bouncing back and passing beyond edges when icon reaches screen edge
                                //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align

                                parentPaddingX = 1 / canvasRefRez.x;
                                parentPaddingY = 1 / canvasRefRez.y;

                                screenBounds.x = screenCenter.x * (.999f - (parentSize.x * parentPaddingX));
                                screenBounds.y = screenCenter.y * (.999f - (parentSize.y * parentPaddingY));


                                //Check up and down first
                                if (cos > 0f)
                                {
                                    //up
                                    ScreenEdgeDetectTop();
                                    objectInWorldScreenPos = new Vector3(-screenBounds.y / m, screenBounds.y, 0f);
                                }
                                else
                                {
                                    //down
                                    ScreenEdgeDetectBot();
                                    objectInWorldScreenPos = new Vector3(screenBounds.y / m, -screenBounds.y, 0f);
                                }

                                //If out of bounds, get point on appropriate side
                                if (objectInWorldScreenPos.x > screenBounds.x) //Out of bounds! Must be on the right
                                {
                                    //right
                                    ScreenEdgeDetectRight();
                                    objectInWorldScreenPos = new Vector3(screenBounds.x, -screenBounds.x * m, 0f);
                                }
                                else if (objectInWorldScreenPos.x < -screenBounds.x) //Out of bounds! Must be on the left
                                {
                                    //left
                                    ScreenEdgeDetectLeft();
                                    objectInWorldScreenPos = new Vector3(-screenBounds.x, screenBounds.x * m, 0);
                                }

                                //Remove coordinate translation
                                objectInWorldScreenPos += screenCenter;


                                wpParentRectTransform.position = mainCamera.ScreenToWorldPoint(new Vector3(objectInWorldScreenPos.x, objectInWorldScreenPos.y, mainCanvas.planeDistance));

                            }
                            break;
                        #endregion


                        #region World
                        case RenderMode.WorldSpace:

                            //ONSCREEN SNAP TO EDGE OFFSET
                            //This ensures the on screen icon boundary edge will trigger offscreen exactly when it touches screen edge
                            //Otherwise, the on screen icon will snap to edge before reaching them
                            //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align
                            onScreenSnapOffset_X = 22 - (mainCanvasRect.sizeDelta.x * .024f);
                            //onScreenSnapOffset_Y = 22 - (Screen.height * .024f);
                            onScreenSnapOffset_Y = 0f;


                            if (objectInViewportPos.z > 0f &&
                            proportionalPosition.x > (wpParentRectTransform.sizeDelta.x / 2) && proportionalPosition.x < (mainCanvasRect.sizeDelta.x - (wpParentRectTransform.sizeDelta.x / 2)) + onScreenSnapOffset_X &&
                            proportionalPosition.y > (wpParentRectTransform.sizeDelta.y / 2) && proportionalPosition.y < (mainCanvasRect.sizeDelta.y - (wpParentRectTransform.sizeDelta.y / 2)) + onScreenSnapOffset_Y)
                            {
                                if (iScreenCheck == 1)
                                {
                                    //Debug.Log("On Screen - World");
                                    parentOnScreen = true;
                                    iScreenCheck--;
                                }
                            }
                            else //Out of camera viewport
                            {

                                if (iScreenCheck == 0)
                                {
                                    //Debug.Log("Off Screen - World");
                                    parentOnScreen = false;
                                    iScreenCheck++;
                                }


                                //Find center of screen
                                screenCenter = new Vector3(mainCanvasRect.rect.width, mainCanvasRect.rect.height, 0f) / 2;
                                //Set 0,0 DEAD CENTER from lower left
                                proportionalPosition -= screenCenter;


                                if (objectInViewportPos.z < 0)
                                {
                                    //Flip coordinates when things are behind
                                    proportionalPosition *= -1;
                                }



                                //Find angle from center of screen to mouse pos
                                angle = Mathf.Atan2(proportionalPosition.y, proportionalPosition.x);
                                angle -= 90f * Mathf.Deg2Rad;

                                cos = Mathf.Cos(angle);
                                sin = Mathf.Sin(angle);

                                //wpParentPos = screenCenter + new Vector3(sin * 150f, cos * 150f, 0f);

                                //y = mx+b format
                                m = cos / sin;

                                screenBounds = screenCenter;


                                //Get Offscreen Padding Offset: (1 / Reference Resolution)
                                //This positions the boundary right along the edge once its gone offscreen
                                //This stops the bouncing back and passing beyond edges when icon reaches screen edge
                                //As long as Canvas > Reference Resolution is the same aspect ratio as the play mode screen, this will always align

                                parentPaddingX = 1 / canvasRefRez.x;
                                parentPaddingY = 1 / canvasRefRez.y;

                                screenBounds.x = screenCenter.x * (.999f - (parentSize.x * parentPaddingX));
                                screenBounds.y = screenCenter.y * (.999f - (parentSize.y * parentPaddingY));


                                //Check up and down first
                                if (cos > 0f)
                                {
                                    //up
                                    ScreenEdgeDetectTop();
                                    proportionalPosition = new Vector3(-screenBounds.y / m, screenBounds.y, 0f);
                                }
                                else
                                {
                                    //down
                                    ScreenEdgeDetectBot();
                                    proportionalPosition = new Vector3(screenBounds.y / m, -screenBounds.y, 0f);
                                }

                                //If out of bounds, get point on appropriate side
                                if (proportionalPosition.x > screenBounds.x) //Out of bounds! Must be on the right
                                {
                                    //right
                                    ScreenEdgeDetectRight();
                                    proportionalPosition = new Vector3(screenBounds.x, -screenBounds.x * m, 0f);
                                }
                                else if (proportionalPosition.x < -screenBounds.x) //Out of bounds! Must be on the left
                                {
                                    //left
                                    ScreenEdgeDetectLeft();
                                    proportionalPosition = new Vector3(-screenBounds.x, screenBounds.x * m, 0);
                                }

                                //Remove coordinate translation
                                proportionalPosition += screenCenter;

                                wpParentRectTransform.localPosition = proportionalPosition - uiOffset;

                            }


                            break;
                            #endregion
                    }
                    #endregion




                    //Size
                    wpParentRectTransform.sizeDelta = parentSize;

                    //Color
                    wpParentGameObject.GetComponent<Image>().color = boundaryBoxColor;

                }
                #endregion


                #region IF ENABLE SPRITE INDICATOR
                if (enableSprite)
                {
                    //Set Size in real time
                    newOnScreenSize.x = onScreenSpriteOriginalSize.x * onScreenSpriteSize;
                    newOnScreenSize.y = onScreenSpriteOriginalSize.y * onScreenSpriteSize;

                    newOffScreenSize.x = offScreenSpriteOriginalSize.x * offScreenSpriteSize;
                    newOffScreenSize.y = offScreenSpriteOriginalSize.y * offScreenSpriteSize;

                    if (spriteIndicatorCreated && spriteIndicatorRect != null)
                    {
                        //Set Raycast Target
                        if (raycastTarget)
                        {
                            spriteIndicatorImage.raycastTarget = true;
                        }
                        else
                        {
                            spriteIndicatorImage.raycastTarget = false;
                        }


                        spriteIndicator.transform.SetSiblingIndex(spriteDepth);

                        if (waypointDist < displayRangeMax && waypointDist > (displayRangeMin + 1)) //ICON IN RANGE
                        {
                            //SPRITE ON-SCREEN -------------------------------------------------------------
                            if (parentOnScreen) //ICON ON-SCREEN
                            {
                                //Sprite
                                spriteIndicatorImage.sprite = onScreenSprite;

                                //Color
                                spriteIndicatorColor = onScreenSpriteColor;

                                //Position
                                spriteIndicatorRect.localPosition = onScreenSpriteOffset;


                                //Rotation
                                //spriteIndicatorRect.transform.eulerAngles = new Vector3(0f, 0f, onScreenSpriteRotation);
                                spriteIndicatorRect.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenSpriteRotation);

                                //Size
                                if (onScreenSpriteHide) //HIDE
                                {
                                    spriteIndicatorImage.rectTransform.localScale = new Vector3(0, 0, Sprite_Z_Scale);
                                }
                                else //SHOW
                                {
                                    //FADE WITH RANGE
                                    if (onScreenSpriteFadeWithRange)
                                    {
                                        FadeSpriteWithRange();
                                    }
                                    else //Use user Alpha
                                    {
                                        spriteIndicatorImage.color = spriteIndicatorColor;
                                    }

                                    //SCALE WITH RANGE
                                    if (onScreenSpriteScaleWithRange)
                                    {
                                        ScaleSpriteWithRange(newOnScreenSize, reverseOnScreenSpriteScaleWithRange);
                                    }
                                    else //Use user Scale
                                    {
                                        spriteIndicatorImage.rectTransform.localScale = new Vector3(newOnScreenSize.x, newOnScreenSize.y, Sprite_Z_Scale);
                                    }

                                }
                            }

                            //SPRITE OFF-SCREEN -------------------------------------------------------------
                            if (!parentOnScreen)
                            {
                                //Sprite
                                spriteIndicatorImage.sprite = offScreenSprite;

                                //Color
                                spriteIndicatorColor = offScreenSpriteColor;

                                //Position
                                spriteIndicatorRect.localPosition = offScreenSpriteOffset;

                                //Rotation
                                if (offScreenSpriteRotates)
                                {
                                    //spriteIndicatorRect.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + offScreenSpriteRotation);
                                    spriteIndicatorRect.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + offScreenSpriteRotation);
                                }
                                else
                                {
                                    //spriteIndicatorRect.transform.eulerAngles = new Vector3(0f, 0f, offScreenSpriteRotation);
                                    spriteIndicatorRect.transform.localRotation = Quaternion.Euler(0f, 0f, offScreenSpriteRotation);
                                }

                                //Size
                                if (offScreenSpriteHide) //HIDE
                                {
                                    spriteIndicatorImage.rectTransform.localScale = new Vector3(0, 0, Sprite_Z_Scale);
                                }
                                else //SHOW
                                {
                                    //FADE WITH RANGE
                                    if (offScreenSpriteFadeWithRange)
                                    {
                                        FadeSpriteWithRange();
                                    }
                                    else //Use user Alpha
                                    {
                                        spriteIndicatorImage.color = spriteIndicatorColor;
                                    }

                                    //SCALE WITH RANGE
                                    if (offScreenScaleWithRange)
                                    {
                                        ScaleSpriteWithRange(newOffScreenSize, reverseOffScreenSpriteScaleWithRange);
                                    }
                                    else //Use user Scale
                                    {
                                        spriteIndicatorImage.rectTransform.localScale = new Vector3(newOffScreenSize.x, newOffScreenSize.y, Sprite_Z_Scale);
                                    }
                                }
                            }
                        }
                        else //ICON OUT OF RANGE
                        {
                            spriteIndicatorImage.rectTransform.localScale = new Vector3(0, 0, Sprite_Z_Scale);
                        }


                    }
                }
                #endregion


                #region IF ENABLE OBJECT INDICATOR
                if (enableGameObject)
                {
                    if (gameObjectIndicatorCreated && gameObjectIndicator != null)
                    {

                        gameObjectIndicator.transform.SetSiblingIndex(gameObjectDepth);

                        if (waypointDist < displayRangeMax && waypointDist > (displayRangeMin + 1)) //PREFAB IN RANGE
                        {
                            //GAME OBJECT INDICATOR ON-SCREEN -------------------------------------------------------------
                            if (parentOnScreen) //ICON ON-SCREEN
                            {
                                //Object Prefab
                                if (gameObjectIndicatorOnOffScreenStatus == 0)
                                {
                                    Destroy(gameObjectIndicator);
                                    gameObjectIndicator = Instantiate(onScreenGameObject, wpParentPos, Quaternion.Euler(0, 0, 0)) as GameObject;
                                    gameObjectIndicator.layer = 2;
                                    gameObjectIndicator.transform.position = wpParentRectTransform.position;
                                    gameObjectIndicator.transform.SetParent(wpParentGameObject.transform);
                                    gameObjectIndicator.name = wpParentRectTransform.name + "-Prefab";
                                    gameObjectIndicatorCanvasGroup = gameObjectIndicator.AddComponent<CanvasGroup>();
                                    gameObjectIndicatorCanvasGroup.alpha = 1f;
                                    gameObjectIndicatorCanvasGroup.blocksRaycasts = false;
                                    gameObjectIndicatorCanvasGroup.interactable = false;
                                    gameObjectIndicatorOnOffScreenStatus = 1;

                                    //Get the game objcet's child that houses the sprite (index 0)
                                    gameObjectIndicatorChildGameObject = gameObjectIndicator.transform.GetChild(0).gameObject;
                                    gameObjectIndicatorChildRect = gameObjectIndicatorChildGameObject.GetComponent<RectTransform>();
                                    gameObjectIndicatorChildImg = gameObjectIndicatorChildRect.GetComponent<RawImage>();


                                    //Set CurvedUI effect manually
                                    //gameObjectIndicator.AddComponent<CurvedUIVertexEffect>();
                                    //gameObjectIndicatorChildGameObject.AddComponent<CurvedUIVertexEffect>();


                                    //Set Raycast Target
                                    if (raycastTarget)
                                    {
                                        gameObjectIndicatorChildImg.raycastTarget = true;
                                    }
                                    else
                                    {
                                        gameObjectIndicatorChildImg.raycastTarget = false;
                                    }

                                }

                                //Position
                                gameObjectIndicator.transform.localPosition = onScreenGameObjectOffset;

                                //Color
                                gameObjectIndicatorChildImg.color = onScreenGameObjectColor;

                                //Rotation
                                gameObjectIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenGameObjectRotation);

                                //Size
                                if (onScreenGameObjectHide) //HIDE
                                {
                                    gameObjectIndicator.transform.localScale = new Vector3(0, 0, Prefab_Z_Scale);
                                }
                                else //Show
                                {
                                    //FADE WITH RANGE
                                    if (onScreenGameObjectFadeWithRange)
                                    {
                                        FadeGameObjectWithRange();
                                    }
                                    else
                                    {
                                        //gameObjectIndicatorCanvasGroup.alpha = 1f;
                                        //User defined alpha
                                        gameObjectIndicatorCanvasGroup.alpha = onScreenGameObjectColor.a;
                                    }

                                    //SCALE WITH RANGE
                                    if (onScreenGameObjectScaleWithRange)
                                    {
                                        ScaleGameObjectWithRange(onScreenGameObjectSize, reverseOnScreenGameObjectScaleWithRange);
                                    }
                                    else //Use user Scale
                                    {
                                        gameObjectIndicator.transform.localScale = new Vector3(onScreenGameObjectSize, onScreenGameObjectSize, Prefab_Z_Scale);

                                    }

                                }

                            }

                            //GAME OBJECT INDICATOR OFF-SCREEN -------------------------------------------------------------
                            if (!parentOnScreen) //ICON OFF-SCREEN
                            {
                                //Object Prefab
                                if (gameObjectIndicatorOnOffScreenStatus == 1)
                                {
                                    Destroy(gameObjectIndicator);
                                    gameObjectIndicator = Instantiate(offScreenGameObject, wpParentPos, Quaternion.Euler(0, 0, 0)) as GameObject;
                                    gameObjectIndicator.layer = 2;
                                    gameObjectIndicator.transform.position = wpParentRectTransform.position;
                                    gameObjectIndicator.transform.SetParent(wpParentGameObject.transform);
                                    gameObjectIndicator.name = wpParentRectTransform.name + "-Prefab";
                                    gameObjectIndicatorCanvasGroup = gameObjectIndicator.AddComponent<CanvasGroup>();
                                    gameObjectIndicatorCanvasGroup.alpha = 1f;
                                    gameObjectIndicatorCanvasGroup.blocksRaycasts = false;
                                    gameObjectIndicatorCanvasGroup.interactable = false;
                                    gameObjectIndicatorOnOffScreenStatus = 0;
                                    gameObjectIndicator.transform.localRotation = Quaternion.Euler(0, 0, 0);

                                    //Get the game objcet's child that houses the sprite (index 0)
                                    gameObjectIndicatorChildGameObject = gameObjectIndicator.transform.GetChild(0).gameObject;
                                    gameObjectIndicatorChildRect = gameObjectIndicatorChildGameObject.GetComponent<RectTransform>();
                                    gameObjectIndicatorChildImg = gameObjectIndicatorChildRect.GetComponent<RawImage>();


                                    //Set CurvedUI effect manually
                                    //gameObjectIndicator.AddComponent<CurvedUIVertexEffect>();
                                    //gameObjectIndicatorChildGameObject.AddComponent<CurvedUIVertexEffect>();


                                    //Set Raycast Target
                                    if (raycastTarget)
                                    {
                                        gameObjectIndicatorChildImg.raycastTarget = true;
                                    }
                                    else
                                    {
                                        gameObjectIndicatorChildImg.raycastTarget = false;
                                    }
                                }

                                //Position
                                gameObjectIndicator.transform.localPosition = offScreenGameObjectOffset;

                                //Color
                                gameObjectIndicatorChildImg.color = offScreenGameObjectColor;

                                //Rotation
                                //gameObjectIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, offScreenGameObjectRotation);
                                //Rotation
                                if (offScreenObjectRotates)
                                {
                                    //gameObjectIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + offScreenGameObjectRotation);
                                    gameObjectIndicator.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + offScreenGameObjectRotation);
                                }
                                else
                                {
                                    //gameObjectIndicator.transform.eulerAngles = new Vector3(0f, 0f, offScreenGameObjectRotation);
                                    gameObjectIndicator.transform.localRotation = Quaternion.Euler(0, 0, offScreenGameObjectRotation);
                                }

                                //Size
                                if (offScreenGameObjectHide) //HIDE
                                {
                                    gameObjectIndicator.transform.localScale = new Vector3(0, 0, Prefab_Z_Scale);
                                }
                                else //Show
                                {
                                    //FADE WITH RANGE
                                    if (offScreenGameObjectFadeWithRange)
                                    {
                                        FadeGameObjectWithRange();
                                    }
                                    else
                                    {
                                        // gameObjectIndicatorCanvasGroup.alpha = 1f;
                                        //User defined alpha
                                        gameObjectIndicatorCanvasGroup.alpha = offScreenGameObjectColor.a;
                                    }

                                    //SCALE WITH RANGE
                                    if (offScreenGameObjectScaleWithRange)
                                    {
                                        ScaleGameObjectWithRange(offScreenGameObjectSize, reverseOffScreenGameObjectScaleWithRange);
                                    }
                                    else //Use user Scale
                                    {
                                        gameObjectIndicator.transform.localScale = new Vector3(offScreenGameObjectSize, offScreenGameObjectSize, Prefab_Z_Scale);

                                    }

                                }
                            }
                        }
                        else //GAME INDICATOR OUT OF RANGE
                        {
                            gameObjectIndicator.transform.localScale = new Vector3(0, 0, Prefab_Z_Scale);
                        }
                    }
                }
                #endregion


                #region IF ENABLE TEXT
                if (enableText)
                {
                    if (waypointTextCreated && textGameObject != null)
                    {
                        textGameObject.transform.SetSiblingIndex(textDepth);


                        //Set Raycast Target
                        if (raycastTarget)
                        {
                            textField.raycastTarget = true;
                        }
                        else
                        {
                            textField.raycastTarget = false;
                        }


                        if (waypointDist < displayRangeMax && waypointDist > (displayRangeMin + 1)) //TEXT IN RANGE
                        {
                            //GLOBAL-----
                            //FONT
                            textField.font = textFont;
                            //SIZE
                            textField.fontSize = textSize;
                            //COLOR
                            textField.color = textColor;
                            //ALIGN
                            switch (textAlign)
                            {
                                case textAlignValue.Left:
                                    //Debug.Log("Left was chosen.");
                                    textField.alignment = TextAlignmentOptions.Left;
                                    break;
                                case textAlignValue.Center:
                                    //Debug.Log("Center was chosen.");
                                    textField.alignment = TextAlignmentOptions.Center;
                                    break;
                                case textAlignValue.Right:
                                    //Debug.Log("Right was chosen.");
                                    textField.alignment = TextAlignmentOptions.Right;
                                    break;
                            }
                            //LINE SPACING
                            textField.lineSpacing = textLineSpacing;
                            //SIZE
                            //This is handled by the ContentSizeFitter.FitMode.PreferredSize


                            // TEXT ONSCREEN ---------------------------------------------------
                            if (parentOnScreen) //ICON ON-SCREEN
                            {
                                //POSITION
                                textGameObject.transform.localPosition = onScreenTextOffset;

                                //DESCRIPTION / DISTANCE
                                if (!onScreenSpriteHideDesc && !onScreenSpriteHideDist) //SHOW BOTH
                                {
                                    textField.text = textDescription + "\n" + waypointDistInt.ToString() + distIncrement;
                                }
                                if (onScreenSpriteHideDesc && !onScreenSpriteHideDist) //SHOW DISTNACE ONLY
                                {
                                    textField.text = waypointDistInt.ToString() + distIncrement;
                                }
                                if (!onScreenSpriteHideDesc && onScreenSpriteHideDist) //SHOW DESCRIPTION ONLY
                                {
                                    textField.text = textDescription;
                                }
                                if (onScreenSpriteHideDesc && onScreenSpriteHideDist) //SHOW NO TEXT
                                {
                                    textField.text = "";
                                }
                            }

                            // TEXT OFFSCREEN ---------------------------------------------------
                            if (!parentOnScreen)
                            {
                                //POSITION
                                //textGameObject.transform.localPosition = offScreenTextOffset;

                                //We comment out the line above because we know that one of these cases will be true 100% of the time while the waypoint is Offscreen. It will always be touching a screen edge.
                                if (topEdgeDetected)
                                {
                                    if (edgeDetectOffset.y > 0)
                                    {
                                        textGameObject.transform.localPosition = new Vector2(offScreenTextOffset.x, -edgeDetectOffset.y);
                                        textField.alignment = TextAlignmentOptions.Center;
                                    }
                                    else
                                    {
                                        textGameObject.transform.localPosition = offScreenTextOffset;
                                    }
                                }
                                if (botEdgeDetected)
                                {
                                    if (edgeDetectOffset.y > 0)
                                    {
                                        textGameObject.transform.localPosition = new Vector2(offScreenTextOffset.x, edgeDetectOffset.y);
                                        textField.alignment = TextAlignmentOptions.Center;
                                    }
                                    else
                                    {
                                        textGameObject.transform.localPosition = offScreenTextOffset;
                                    }
                                }
                                if (rightEdgeDetected)
                                {
                                    if (edgeDetectOffset.x > 0)
                                    {
                                        textGameObject.transform.localPosition = new Vector2(-edgeDetectOffset.x, offScreenTextOffset.y);
                                        textField.alignment = TextAlignmentOptions.Right;
                                    }
                                    else
                                    {
                                        textGameObject.transform.localPosition = offScreenTextOffset;
                                    }
                                }
                                if (leftEdgeDetected)
                                {
                                    if (edgeDetectOffset.x > 0)
                                    {
                                        textGameObject.transform.localPosition = new Vector2(edgeDetectOffset.x, offScreenTextOffset.y);
                                        textField.alignment = TextAlignmentOptions.Left;
                                    }
                                    else
                                    {
                                        textGameObject.transform.localPosition = offScreenTextOffset;
                                    }
                                }


                                //DESCRIPTION / DISTANCE
                                if (!offScreenSpriteHideDesc && !offScreenSpriteHideDist) //SHOW BOTH
                                {
                                    textField.text = textDescription + "\n" + waypointDistInt.ToString() + distIncrement;
                                }
                                if (offScreenSpriteHideDesc && !offScreenSpriteHideDist) //SHOW DISTNACE ONLY
                                {
                                    textField.text = waypointDistInt.ToString() + distIncrement;
                                }
                                if (!offScreenSpriteHideDesc && offScreenSpriteHideDist) //SHOW DESCRIPTION ONLY
                                {
                                    textField.text = textDescription;
                                }
                                if (offScreenSpriteHideDesc && offScreenSpriteHideDist) //SHOW NO TEXT
                                {
                                    textField.text = "";
                                }
                            }
                        }
                        else  //TEXT OUT OF RANGE
                        {
                            textField.text = "";
                        }


                    }
                }
                #endregion

            }
            else
            {
                iSpriteIndicator = 0;
                iGameObjectIndicator = 0;
                iText = 0;
                spriteIndicatorCreated = false;
                gameObjectIndicatorCreated = false;
                waypointTextCreated = false;

                //Destroy wp ui using find from OnEnable() above
                if (wpParentGameObject != null)
                {
                    Destroy(wpParentGameObject);
                }

                iStandardTrackingEnabled = 0;


                //Resets Calculate Dist Function
                distanceTargetDefined = false;
            }
            #endregion



            #region ENABLE CENTERED TRACKING
            if (enableCenteredTracking)
            {
                //Reset Centered Tracking as if it were Enable()
                if (iCenteredTrackingEnabled == 0)
                {

                    InstantiateCenteredWaypointParent();
                    //InstantiateCenteredWaypointIcon();

                    #region Fill missing fields for Centered Sprite if left empty to avoid errors

                    //CENTERED SPRITE CHECK
                    if (onScreenCenteredSprite != null) //At least one icon was defined for one slot
                    {
                        //Get the original size of the Sprite before alterations
                        onScreenCenteredSpriteOriginalSize.x = onScreenCenteredSprite.bounds.size.x;
                        onScreenCenteredSpriteOriginalSize.y = onScreenCenteredSprite.bounds.size.y;
                    }
                    else
                    {
                        Debug.LogWarning("Centered Tracking Sprite Missing on: " + gameObject.name + "\nAssign a Sprite to the Sprite field. - void Update()");

                        //Get the original size of the Sprite before alterations
                        onScreenCenteredSpriteOriginalSize.x = 1f;
                        onScreenCenteredSpriteOriginalSize.y = 1f;
                    }
                    #endregion

                    iCenteredTrackingEnabled = 1;
                }


                #region Check Toggle States
                //CENTERED SPRITE INDICATOR
                if (enableCenteredSprite) //ENABLE
                {

                    if (iCenteredSpriteIndicator == 0)
                    {
                        //Debug.Log("Enable Cenetered Sprite");
                        InstantiateCenteredWaypointIcon();
                        iCenteredSpriteIndicator++;
                    }
                }
                if (!enableCenteredSprite) //DISABLE
                {
                    if (iCenteredSpriteIndicator == 1)
                    {
                        //Debug.Log("Destroy Centered Sprite here");
                        DestroyCenteredSprite();
                        iCenteredSpriteIndicator--;
                    }
                }

                //CENTERED GAME OBJECT INDICATOR
                if (enableCenteredPrefab) //ENABLE
                {
                    if (iCenteredPrafabIndicator == 0)
                    {
                        //Debug.Log("Enable Cenetered Prefab");
                        InstantiateCenteredWaypointPrefab();
                        iCenteredPrafabIndicator++;
                    }
                }
                if (!enableCenteredPrefab) //DISABLE
                {
                    if (iCenteredPrafabIndicator == 1)
                    {
                        //Debug.Log("Destroy Centered Game Object here");
                        DestroyCenteredPrefab();
                        iCenteredPrafabIndicator--;
                    }
                }
                #endregion



                //Determine Distance from Game Object (that this script is attached to) Distance is from Camera or DCFT
                CalculateDistance();
                waypointDist = Vector3.Distance(distCalTarget.transform.position, transform.position);
                waypointCenteredDist = (int)waypointDist; //We convert this for the text readout so users dont see: Distance 23.547893m


                #region IF ENABLE CENTERED PARENT (enabled on Start)
                if (wpCenteredParentGameObject && wpCenteredParentRectTransform != null)
                {
                    //Reset Scale to 1
                    wpCenteredParentRectTransform.localScale = new Vector3(1f, 1f, 1f);

                    //Show/Hide
                    if (showDiameter)
                    {
                        wpCenteredParentImage.enabled = true;
                    }
                    else
                    {
                        wpCenteredParentImage.enabled = false;
                    }

                    //Match the PARENT UI pos to 3D MESH position (the object this script is attached to) via WorldToScreenPoint
                    if (mainCamera != null)
                    {


                        #region Screen Space Check
                        switch (mainCanvas.renderMode)
                        {
                            #region Overlay
                            case RenderMode.ScreenSpaceOverlay:
                                wpCenteredParentPos = mainCamera.WorldToScreenPoint(transform.position);

                                //This keeps wpCenteredParentPos.z value from going above 1 causing dissapearing if above 1000 or so
                                if (wpCenteredParentPos.z > 1)
                                {
                                    wpCenteredParentPos.z = 1f;
                                }
                                else
                                {
                                    wpCenteredParentPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
                                }
                                break;
                            #endregion

                            #region Camera
                            case RenderMode.ScreenSpaceCamera:
                                objectInWorldScreenPosCentered = mainCamera.WorldToScreenPoint(transform.position); //0,0 bottom left | 0,1 top left | 1,1 top right | 1,0 bottom right
                                wpCenteredParentRectTransform.position = mainCamera.ScreenToWorldPoint(new Vector3(objectInWorldScreenPosCentered.x, objectInWorldScreenPosCentered.y, mainCanvas.planeDistance));
                                break;
                            #endregion

                            #region World
                            case RenderMode.WorldSpace:
                                uiOffsetCentered = new Vector3((float)mainCanvasRect.sizeDelta.x / 2f, (float)mainCanvasRect.sizeDelta.y / 2f, 0);

                                objectInWorldScreenPosCentered = mainCamera.WorldToViewportPoint(transform.position);
                                proportionalPositionCentered = new Vector3(objectInWorldScreenPosCentered.x * mainCanvasRect.sizeDelta.x, objectInWorldScreenPosCentered.y * mainCanvasRect.sizeDelta.y, 0);

                                // Set the position and remove the screen offset
                                wpCenteredParentRectTransform.localPosition = proportionalPositionCentered - uiOffsetCentered;

                                break;
                                #endregion
                        }
                        #endregion
                    }



                    #region Screen Space Check
                    switch (mainCanvas.renderMode)
                    {
                        #region Overlay
                        case RenderMode.ScreenSpaceOverlay:
                            #region ON/OFF SCREEN CHECK
                            if (wpCenteredParentPos.z > 0f &&
                            wpCenteredParentPos.x > wpCenteredParentRectTransform.sizeDelta.x && wpCenteredParentPos.x < Screen.width &&
                            wpCenteredParentPos.y > wpCenteredParentRectTransform.sizeDelta.x && wpCenteredParentPos.y < Screen.height)
                            {
                                //On screen
                                centeredParentOnScreen = true;
                            }
                            else
                            {
                                //Off screen
                                centeredParentOnScreen = false;
                            }
                            #endregion


                            //Find center of screen
                            screenCenteredCenter = new Vector3(Screen.width, Screen.height, 0f) / 2;
                            //Set 0,0 DEAD CENTER from lower left
                            wpCenteredParentPos -= screenCenteredCenter;


                            if (wpCenteredParentPos.z < 0)
                            {
                                //Flip coordinates when things are behind
                                wpCenteredParentPos *= -1;
                            }


                            //Find angleCentered from center of screen to mouse pos
                            angleCentered = Mathf.Atan2(wpCenteredParentPos.y, wpCenteredParentPos.x);
                            angleCentered -= 90f * Mathf.Deg2Rad;

                            cosCentered = Mathf.Cos(angleCentered);
                            sinCentered = Mathf.Sin(angleCentered);


                            //y = mx+b format
                            mCentered = cosCentered / sinCentered;

                            Vector3 screenBoundsCentered = screenCenteredCenter;
                            screenBoundsCentered.x = 0f;
                            screenBoundsCentered.y = 0f;


                            //Check up and down first
                            if (cosCentered > 0f)
                            {
                                //up
                                ScreenEdgeDetectTop(); //This toggles screen edge detect bools for future use
                                wpCenteredParentPos = new Vector3(-screenBoundsCentered.y / mCentered, screenBoundsCentered.y, 0f);
                            }
                            else
                            {
                                //down
                                ScreenEdgeDetectBot(); //This toggles screen edge detect bools for future use
                                wpCenteredParentPos = new Vector3(screenBoundsCentered.y / mCentered, -screenBoundsCentered.y, 0f);
                            }

                            //If out of bounds, get point on appropriate side
                            if (wpCenteredParentPos.x > screenBoundsCentered.x) //Out of bounds! Must be on the right
                            {
                                //left
                                ScreenEdgeDetectLeft(); //This toggles screen edge detect bools for future use
                                wpCenteredParentPos = new Vector3(screenBoundsCentered.x, -screenBoundsCentered.x * mCentered, 0f);
                            }
                            else if (wpCenteredParentPos.x < -screenBoundsCentered.x) //Out of bounds! Must be on the left
                            {
                                //right
                                ScreenEdgeDetectRight(); //This toggles screen edge detect bools for future use
                                wpCenteredParentPos = new Vector3(-screenBoundsCentered.x, screenBoundsCentered.x * mCentered, 0);
                            }

                            //Remove coordinate translation
                            wpCenteredParentPos += screenCenteredCenter;

                            //Position
                            wpCenteredParentRectTransform.transform.position = wpCenteredParentPos;

                            break;
                        #endregion

                        #region Camera
                        case RenderMode.ScreenSpaceCamera:

                            //Get the object's World to Viewport pos so we can easily deifne on and off screen (0,0 and 1,1)
                            objectWorldToViewportPosCentered = mainCamera.WorldToViewportPoint(transform.position);

                            //We still need a World to Screen to check for z depth
                            if (objectInWorldScreenPosCentered.z > 0f &&
                                objectWorldToViewportPosCentered.x > 0 && objectWorldToViewportPosCentered.x < 1 &&
                                objectWorldToViewportPosCentered.y > 0 && objectWorldToViewportPosCentered.y < 1)
                            {
                                //On screen
                                centeredParentOnScreen = true;
                            }
                            else
                            {
                                //Off screen
                                centeredParentOnScreen = false;
                            }


                            //Find center of screen
                            screenCenteredCenter = new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, 0f) / 2;
                            //Set 0,0 DEAD CENTER from lower left
                            objectInWorldScreenPosCentered -= screenCenteredCenter;


                            if (objectInWorldScreenPosCentered.z < 0)
                            {
                                //Flip coordinates when things are behind
                                objectInWorldScreenPosCentered *= -1;
                            }


                            //Find angleCentered from center of screen to mouse pos
                            angleCentered = Mathf.Atan2(objectInWorldScreenPosCentered.y, objectInWorldScreenPosCentered.x);
                            angleCentered -= 90f * Mathf.Deg2Rad;

                            cosCentered = Mathf.Cos(angleCentered);
                            sinCentered = Mathf.Sin(angleCentered);


                            //y = mx+b format
                            mCentered = cosCentered / sinCentered;

                            screenBoundsCentered = screenCenteredCenter;
                            screenBoundsCentered.x = 0f;
                            screenBoundsCentered.y = 0f;


                            //Check up and down first
                            if (cosCentered > 0f)
                            {
                                //up
                                ScreenEdgeDetectTop(); //This toggles screen edge detect bools for future use
                                objectInWorldScreenPosCentered = new Vector3(-screenBoundsCentered.y / mCentered, screenBoundsCentered.y, 0f);
                            }
                            else
                            {
                                //down
                                ScreenEdgeDetectBot(); //This toggles screen edge detect bools for future use
                                objectInWorldScreenPosCentered = new Vector3(screenBoundsCentered.y / mCentered, -screenBoundsCentered.y, 0f);
                            }

                            //If out of bounds, get point on appropriate side
                            if (objectInWorldScreenPosCentered.x > screenBoundsCentered.x) //Out of bounds! Must be on the right
                            {
                                //left
                                ScreenEdgeDetectLeft(); //This toggles screen edge detect bools for future use
                                objectInWorldScreenPosCentered = new Vector3(screenBoundsCentered.x, -screenBoundsCentered.x * mCentered, 0f);
                            }
                            else if (objectInWorldScreenPosCentered.x < -screenBoundsCentered.x) //Out of bounds! Must be on the left
                            {
                                //right
                                ScreenEdgeDetectRight(); //This toggles screen edge detect bools for future use
                                objectInWorldScreenPosCentered = new Vector3(-screenBoundsCentered.x, screenBoundsCentered.x * mCentered, 0);
                            }

                            //Remove coordinate translation
                            objectInWorldScreenPosCentered += screenCenteredCenter;

                            //Position
                            //Initial Centered Set version
                            //objectInWorldScreenPosCentered = mainCamera.WorldToScreenPoint(transform.position); //0,0 bottom left | 0,1 top left | 1,1 top right | 1,0 bottom right
                            wpCenteredParentRectTransform.position = mainCamera.ScreenToWorldPoint(new Vector3(objectInWorldScreenPosCentered.x, objectInWorldScreenPosCentered.y, mainCanvas.planeDistance));

                            break;
                        #endregion

                        #region World
                        case RenderMode.WorldSpace:

                            //Get the object's World to Viewport pos so we can easily deifne on and off screen (0,0 and 1,1)
                            objectWorldToViewportPosCentered = mainCamera.WorldToViewportPoint(transform.position);

                            //We still need a World to Screen to check for z depth
                            if (objectInWorldScreenPosCentered.z > 0f &&
                                objectWorldToViewportPosCentered.x > 0 && objectWorldToViewportPosCentered.x < 1 &&
                                objectWorldToViewportPosCentered.y > 0 && objectWorldToViewportPosCentered.y < 1)
                            {
                                //On screen
                                centeredParentOnScreen = true;
                            }
                            else
                            {
                                //Off screen
                                centeredParentOnScreen = false;
                            }


                            //Find center of screen
                            screenCenteredCenter = new Vector3(mainCanvasRect.sizeDelta.x, mainCanvasRect.sizeDelta.y, 0f) / 2;
                            //Set 0,0 DEAD CENTER from lower left
                            proportionalPositionCentered -= screenCenteredCenter;


                            if (objectInWorldScreenPosCentered.z < 0)
                            {
                                //Flip coordinates when things are behind
                                proportionalPositionCentered *= -1;
                            }


                            //Find angleCentered from center of screen to mouse pos
                            angleCentered = Mathf.Atan2(proportionalPositionCentered.y, proportionalPositionCentered.x);
                            angleCentered -= 90f * Mathf.Deg2Rad;

                            cosCentered = Mathf.Cos(angleCentered);
                            sinCentered = Mathf.Sin(angleCentered);


                            //y = mx+b format
                            mCentered = cosCentered / sinCentered;

                            screenBoundsCentered = screenCenteredCenter;
                            screenBoundsCentered.x = 0f;
                            screenBoundsCentered.y = 0f;


                            //Check up and down first
                            if (cosCentered > 0f)
                            {
                                //up
                                ScreenEdgeDetectTop(); //This toggles screen edge detect bools for future use
                                proportionalPositionCentered = new Vector3(-screenBoundsCentered.y / mCentered, screenBoundsCentered.y, 0f);
                            }
                            else
                            {
                                //down
                                ScreenEdgeDetectBot(); //This toggles screen edge detect bools for future use
                                proportionalPositionCentered = new Vector3(screenBoundsCentered.y / mCentered, -screenBoundsCentered.y, 0f);
                            }

                            //If out of bounds, get point on appropriate side
                            if (proportionalPositionCentered.x > screenBoundsCentered.x) //Out of bounds! Must be on the right
                            {
                                //left
                                ScreenEdgeDetectLeft(); //This toggles screen edge detect bools for future use
                                proportionalPositionCentered = new Vector3(screenBoundsCentered.x, -screenBoundsCentered.x * mCentered, 0f);
                            }
                            else if (proportionalPositionCentered.x < -screenBoundsCentered.x) //Out of bounds! Must be on the left
                            {
                                //right
                                ScreenEdgeDetectRight(); //This toggles screen edge detect bools for future use
                                proportionalPositionCentered = new Vector3(-screenBoundsCentered.x, screenBoundsCentered.x * mCentered, 0);
                            }

                            //Remove coordinate translation
                            proportionalPositionCentered += screenCenteredCenter;

                            //Position
                            wpCenteredParentRectTransform.localPosition = proportionalPositionCentered - uiOffsetCentered;

                            break;
                            #endregion
                    }
                    #endregion





                    //Rotation
                    //wpCenteredParentRectTransform.transform.rotation = Quaternion.Euler(0, 0, angleCentered * Mathf.Rad2Deg);
                    wpCenteredParentRectTransform.transform.localRotation = Quaternion.Euler(0f, 0f, angleCentered * Mathf.Rad2Deg);

                    //Size
                    if (diameterSize > 15f)
                    {
                        wpCenteredParentRectTransform.sizeDelta = new Vector2(diameterSize / 10, diameterSize);
                    }
                    else
                    {
                        wpCenteredParentRectTransform.sizeDelta = new Vector2(diameterSize, diameterSize);
                    }


                    //Color
                    wpCenteredParentGameObject.GetComponent<Image>().color = diameterColor;





                }
                #endregion





                #region IF ENABLE CENTERED ICON
                if (enableCenteredSprite)
                {
                    //Set Size in real time
                    newOnScreenCenteredSize.x = onScreenCenteredSpriteOriginalSize.x * onScreenCenteredSpriteSize;
                    newOnScreenCenteredSize.y = onScreenCenteredSpriteOriginalSize.y * onScreenCenteredSpriteSize;


                    if (waypointIconCenteredCreated && iconCentered != null)
                    {
                        //Set Raycast Target
                        if (raycastTargetCentered)
                        {
                            iconCenteredImage.raycastTarget = true;
                        }
                        else
                        {
                            iconCenteredImage.raycastTarget = false;
                        }

                        //Add SPRITE depth value here:
                        iconCenteredGameObject.transform.SetSiblingIndex(onScreenCenteredSpriteDepth);

                        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin) //ICON IN RANGE
                        {

                            //Sprite
                            iconCenteredImage.sprite = onScreenCenteredSprite;

                            //Position
                            iconCentered.localPosition = new Vector2(0f, diameterSize / 2f);

                            //Color - This is handled in the else section of Fade with Range
                            //iconCenteredImage.color = onScreenCenteredSpriteColor;

                            //Rotation
                            //iconCentered.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenCenteredSpriteRotation);
                            iconCentered.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenCenteredSpriteRotation);

                            //Size
                            //SCALE WITH RANGE
                            if (onScreenSpriteCenteredScaleWithRange)
                            {
                                ScaleWithRangeCentered(newOnScreenCenteredSize, onScreenSpriteCenteredScaleReverse);
                            }
                            else //Use user Scale
                            {
                                iconCenteredImage.rectTransform.localScale = new Vector2(newOnScreenCenteredSize.x, newOnScreenCenteredSize.y);
                            }


                            //FADE WITH RANGE
                            if (onScreenSpriteCenteredFadeWithRange)
                            {
                                FadeWithRangeCentered();
                            }
                            else
                            {
                                //Alpha set to 0 - We use SCALE to "Hide" iconCentereds, so no need to do any adjustments here
                                iconCenteredAlphaValue = 1;
                                iconCenteredColor.a = iconCenteredAlphaValue;
                                iconCenteredImage.color = onScreenCenteredSpriteColor;
                            }



                            //ICON ON-SCREEN -------------------------------------------------------------
                            if (centeredParentOnScreen)
                            {
                                //On screen
                                if (hideOnScreenCenteredSprite)
                                {
                                    iconCenteredImage.rectTransform.localScale = new Vector2(0, 0);
                                }
                                else
                                {
                                    //iconCenteredImage.rectTransform.localScale = new Vector2(newOnScreenCenteredSize.x, newOnScreenCenteredSize.y);
                                }

                            }
                            //ICON OFF-SCREEN -------------------------------------------------------------
                            if (!centeredParentOnScreen)
                            {
                                //Off screen
                                if (hideOffScreenCenteredSprite)
                                {
                                    iconCenteredImage.rectTransform.localScale = new Vector2(0, 0);
                                }
                                else
                                {
                                    //iconCenteredImage.rectTransform.localScale = new Vector2(newOnScreenCenteredSize.x, newOnScreenCenteredSize.y);
                                }
                            }


                        }
                        else //ICON OUT OF RANGE
                        {
                            iconCenteredImage.rectTransform.localScale = new Vector2(0, 0);
                        }


                    }
                }

                #endregion





                #region IF ENABLE CENTERED PREFAB
                if (enableCenteredPrefab)
                {
                    //Set Size in real time
                    newOnScreenPrefabCenteredSize.x = onScreenCenteredPrefabSize;
                    newOnScreenPrefabCenteredSize.y = onScreenCenteredPrefabSize;


                    if (centeredPrefabIndicatorCreated && centeredPrefabIndicatorChildRect != null)
                    {
                        //Set Raycast Target for Ceneterd
                        if (raycastTargetCentered)
                        {
                            centeredPrefabIndicatorChildImg.raycastTarget = true;
                        }
                        else
                        {
                            centeredPrefabIndicatorChildImg.raycastTarget = false;
                        }

                        //Add PREFAB depth value here:
                        centeredPrefabIndicator.transform.SetSiblingIndex(onScreenCenteredPrefabDepth);

                        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin) //ICON IN RANGE
                        {

                            //Position
                            centeredPrefabIndicator.transform.localPosition = new Vector2(0f, diameterSize / 2f);

                            //Rotation
                            //centeredPrefabIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenCenteredPrefabRotation);
                            centeredPrefabIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, onScreenCenteredPrefabRotation);

                            //Color
                            centeredPrefabIndicatorChildImg.color = onScreenCenteredPrefabColor;


                            //Size - User Defined
                            centeredPrefabIndicator.transform.localScale = new Vector3(onScreenCenteredPrefabSize, onScreenCenteredPrefabSize, 0f);


                            if (!centeredParentOnScreen) //OFF-SCREEN
                            {
                                if (hideOffScreenCenteredPrefab) //HIDE
                                {
                                    centeredPrefabIndicator.transform.localScale = new Vector3(0, 0, 0);
                                }
                                else
                                {
                                    //FADE WITH RANGE
                                    if (onScreenPrefabCenteredFadeWithRange)
                                    {
                                        FadeCenteredPrefabWithRange();
                                    }
                                    else
                                    {
                                        //User defined alpha
                                        centeredPrefabIndicatorCanvasGroup.alpha = onScreenCenteredPrefabColor.a;
                                    }

                                    //SCALE WITH RANGE
                                    if (onScreenPrefabCenteredScaleWithRange)
                                    {
                                        ScaleCenteredPrefabWithRange(onScreenCenteredPrefabSize, onScreenPrefabCenteredScaleReverse);
                                    }
                                    else //Use user Scale
                                    {
                                        centeredPrefabIndicator.transform.localScale = new Vector3(onScreenCenteredPrefabSize, onScreenCenteredPrefabSize, 0f);
                                    }
                                }
                            }
                            else //ON-SCREEN
                            {
                                if (hideOnScreenCenteredPrefab) //SHOW
                                {
                                    centeredPrefabIndicator.transform.localScale = new Vector3(0, 0, 0);
                                }
                                else
                                {
                                    //FADE WITH RANGE
                                    if (onScreenPrefabCenteredFadeWithRange)
                                    {
                                        FadeCenteredPrefabWithRange();
                                    }
                                    else
                                    {
                                        //User defined alpha
                                        centeredPrefabIndicatorCanvasGroup.alpha = onScreenCenteredPrefabColor.a;
                                    }

                                    //SCALE WITH RANGE
                                    if (onScreenPrefabCenteredScaleWithRange)
                                    {
                                        ScaleCenteredPrefabWithRange(onScreenCenteredPrefabSize, onScreenPrefabCenteredScaleReverse);
                                    }
                                    else //Use user Scale
                                    {
                                        centeredPrefabIndicator.transform.localScale = new Vector3(onScreenCenteredPrefabSize, onScreenCenteredPrefabSize, 0f);
                                    }
                                }
                            }


                        }
                        else //ICON OUT OF RANGE
                        {
                            //centeredPrefabIndicatorChildImg.rectTransform.localScale = new Vector2(0, 0);
                            centeredPrefabIndicator.transform.localScale = new Vector3(0, 0, 0);
                        }


                    }
                }

                #endregion


            }
            else
            {
                waypointIconCenteredCreated = false;
                centeredPrefabIndicatorCreated = false;
                enableRadiusGizmo = false;
                iCenteredSpriteIndicator = 0; //sets toggles to 0
                iCenteredPrafabIndicator = 0; //sets toggles to 0


                //Destroy wp ui using find from OnEnable() above
                if (wpCenteredParentGameObject != null)
                {
                    Destroy(wpCenteredParentGameObject);
                }

                iCenteredTrackingEnabled = 0;
            }
            #endregion

        }
        else
        {
            //In case the camera or canvas was not found on awake, we do another check here
            //Debug.Log("Waypoint not showing up because you are trying to spawn this game object before spawning a canvas or camera in the scene first. Attempting to check again.");
            //Do another check for the camera
            CheckForCamera();
            CheckForCanvas();
            //Debug.Log("There is no cam");
        }

        //This can find the WP Parent UI even if this is disabled! - Delete later
        //GameObject pee = mainCanvas.transform.Find(gameObject.name + "-WP-" + GetInstanceID()).gameObject;
        //Debug.Log("Game Object Name: " + pee.name);



    }



    #region Standard Tracking Functions

    #region Create Parent
    //CREATE PARENT UI 
    void InstantiateWaypointParent()
    {
        if (mainCanvas != null)
        {
            wpParentGameObject = new GameObject();
            wpParentGameObject.layer = 2;
            wpParentRectTransform = wpParentGameObject.AddComponent<RectTransform>();
            wpParentRectTransform.transform.SetParent(mainCanvas.transform);
            wpParentRectTransform.name = gameObject.name + "-WP-" + GetInstanceID();
            wpParentImage = wpParentRectTransform.gameObject.AddComponent<Image>();
            wpParentRectTransform.position = new Vector3(-1000f, -1000f, 0f); //This make sure the blinking box doesn't appear in bottom left at spawn
            wpParentRectTransform.localRotation = Quaternion.Euler(0, 0, 0);

            //Set on or offscreen sprites, objects, text on load
            #region Screen Space Check
            switch (mainCanvas.renderMode)
            {
                #region Overlay
                case RenderMode.ScreenSpaceOverlay:
                    if (wpParentPos.z > 0f &&
                        wpParentPos.x > (wpParentRectTransform.sizeDelta.x / 2) && wpParentPos.x < Screen.width - (wpParentRectTransform.sizeDelta.x / 2) &&
                        wpParentPos.y > (wpParentRectTransform.sizeDelta.x / 2) && wpParentPos.y < Screen.height - (wpParentRectTransform.sizeDelta.x / 2))
                    {
                        //On screen
                        if (iScreenCheck == 1)
                        {
                            //Debug.Log("On Screen - Overlay");
                            gameObjectIndicatorOnOffScreenStatus = 0;
                            parentOnScreen = true;
                            iScreenCheck--;
                        }
                    }
                    else
                    {
                        //Off screen
                        if (iScreenCheck == 0)
                        {
                            //Debug.Log("Off Screen- Overlay");
                            gameObjectIndicatorOnOffScreenStatus = 1;
                            parentOnScreen = false;
                            iScreenCheck++;
                        }
                    }
                    break;
                #endregion

                #region Camera
                case RenderMode.ScreenSpaceCamera:
                    if (objectInWorldScreenPos.z > 0f &&
                        (objectInWorldScreenPos.x / canvasScaleFactor) > (wpParentRectTransform.sizeDelta.x / 2) && objectInWorldScreenPos.x < (mainCamera.pixelWidth - (wpParentRectTransform.sizeDelta.x / 2)) &&
                        objectInWorldScreenPos.y / canvasScaleFactor > (wpParentRectTransform.sizeDelta.y / 2) && objectInWorldScreenPos.y < (mainCamera.pixelHeight - (wpParentRectTransform.sizeDelta.y / 2)))
                    {
                        if (iScreenCheck == 1)
                        {
                            //Debug.Log("On Screen - Camera!");
                            parentOnScreen = true;
                            iScreenCheck--;
                        }
                    }
                    else //Out of camera viewport
                    {
                        if (iScreenCheck == 0)
                        {
                            //Debug.Log("On Screen - Camera!");
                            parentOnScreen = false;
                            iScreenCheck++;
                        }
                    }
                    break;
                #endregion

                #region World
                case RenderMode.WorldSpace:
                    if (objectInViewportPos.z > 0f &&
                        proportionalPosition.x > (wpParentRectTransform.sizeDelta.x / 2) && proportionalPosition.x < (mainCanvasRect.sizeDelta.x - (wpParentRectTransform.sizeDelta.x / 2)) &&
                        proportionalPosition.y > (wpParentRectTransform.sizeDelta.y / 2) && proportionalPosition.y < (mainCanvasRect.sizeDelta.y - (wpParentRectTransform.sizeDelta.y / 2)))
                    {
                        if (iScreenCheck == 1)
                        {
                            //Debug.Log("On Screen - World!");
                            parentOnScreen = true;
                            iScreenCheck--;
                        }
                    }
                    else //Out of camera viewport
                    {
                        if (iScreenCheck == 0)
                        {
                            //Debug.Log("Off Screen - World!");
                            parentOnScreen = false;
                            iScreenCheck++;
                        }
                    }
                    break;
                    #endregion

            }
            #endregion


            //Set CurvedUI effect manually
            //wpParentGameObject.AddComponent<CurvedUIVertexEffect>();


            //Toggle Visibility - Checks WPI_Manager to see if this Waypoint should be enabled or disabled after being created
            if (WPI_Manager.waypoint_indicators_are_visible)
            {
                wpParentGameObject.SetActive(true);
            }
            else
            {
                wpParentGameObject.SetActive(false);
            }
        }

    }
    #endregion


    #region Create Sprite
    //SPRITE ICON
    void InstantiateWaypointIcon()
    {
        if (onScreenSprite != null && offScreenSprite != null) //Make sure BOTH icons are defined before making
        {
            spriteIndicator = new GameObject();
            spriteIndicator.layer = 2;
            spriteIndicatorRect = spriteIndicator.AddComponent<RectTransform>();
            spriteIndicatorRect.position = wpParentRectTransform.position;
            spriteIndicator.transform.SetParent(wpParentGameObject.transform);
            spriteIndicatorRect.name = wpParentRectTransform.name + "-Sprite";
            spriteIndicatorImage = spriteIndicatorRect.gameObject.AddComponent<Image>();
            spriteIndicatorImage.sprite = onScreenSprite;
            spriteIndicatorImage.rectTransform.localScale = new Vector3(newOnScreenSize.x, newOnScreenSize.y, Sprite_Z_Scale); //user set scale
            spriteIndicatorRect.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //spriteIndicator.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //spriteIndicatorImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);

            //Set the spriteIndicatorColorarency of an image component that conatains sprites/textures
            //spriteIndicatorColor = spriteIndicatorImage.color;
            spriteIndicatorColor = new Color32(0, 0, 0, 255);
            spriteIndicatorColor.a = 1f; //Default is transparent on spawn
            spriteIndicatorImage.color = spriteIndicatorColor;

            spriteIndicator.transform.SetSiblingIndex(spriteDepth);


            //Set CurvedUI effect manually
            //spriteIndicator.AddComponent<CurvedUIVertexEffect>();


            spriteIndicatorCreated = true;
        }
    }
    #endregion


    #region Create Prefab
    //GAME OBJECT ICON
    void InstantiateWaypointGameObject()
    {
        if (onScreenGameObject != null && offScreenGameObject != null)
        {
            //Onscreen
            if (gameObjectIndicatorOnOffScreenStatus == 0)
            {
                Destroy(gameObjectIndicator);
                gameObjectIndicator = Instantiate(onScreenGameObject, wpParentPos, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg)) as GameObject;
                gameObjectIndicatorOnOffScreenStatus = 1;
            }
            if (gameObjectIndicatorOnOffScreenStatus == 1)
            {
                Destroy(gameObjectIndicator);
                gameObjectIndicator = Instantiate(offScreenGameObject, wpParentPos, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg)) as GameObject;
                gameObjectIndicatorOnOffScreenStatus = 0;
            }

            //gameObjectIndicator = onScreenGameObject;
            gameObjectIndicator.layer = 2;
            gameObjectIndicator.transform.position = wpParentRectTransform.position;
            gameObjectIndicator.transform.SetParent(wpParentGameObject.transform);
            gameObjectIndicator.name = wpParentRectTransform.name + "-Prefab";
            gameObjectIndicator.transform.localRotation = Quaternion.Euler(0, 0, 0);

            #region Set Child Object's Sprite Color - (Ref only) This means nothing as it re-instantiates form on to off screen
            //Get the game objcet's child that houses the sprite (index 0)
            gameObjectIndicatorChildGameObject = gameObjectIndicator.transform.GetChild(0).gameObject;
            gameObjectIndicatorChildRect = gameObjectIndicatorChildGameObject.GetComponent<RectTransform>();
            gameObjectIndicatorChildImg = gameObjectIndicatorChildRect.GetComponent<RawImage>();

            //Set the color of the Image
            gameObjectIndicatorChildIndicatorColor = new Color32(0, 0, 0, 255);
            gameObjectIndicatorChildIndicatorColor.a = 1f; //Default is transparent on spawn
            gameObjectIndicatorChildImg.color = gameObjectIndicatorChildIndicatorColor;
            #endregion

            gameObjectIndicatorCanvasGroup = gameObjectIndicator.AddComponent<CanvasGroup>();
            gameObjectIndicatorCanvasGroup.alpha = 1f;
            gameObjectIndicatorCanvasGroup.blocksRaycasts = false;
            gameObjectIndicatorCanvasGroup.interactable = false;

            gameObjectIndicator.transform.SetSiblingIndex(gameObjectDepth);


            //Set CurvedUI effect manually
            //gameObjectIndicator.AddComponent<CurvedUIVertexEffect>();
            //gameObjectIndicatorChildGameObject.AddComponent<CurvedUIVertexEffect>();


            gameObjectIndicatorCreated = true;
        }

    }
    #endregion


    #region Create Text
    //CREATE TEXT UI 
    void InstantiateWaypointText()
    {
        textGameObject = new GameObject();
        textGameObject.layer = 2;
        textGameObject.transform.SetParent(wpParentGameObject.transform);
        textField = textGameObject.AddComponent<TextMeshProUGUI>();

        //This allows the text boundary to scale with font szie automatically
        textContentSizeFitter = textGameObject.AddComponent<ContentSizeFitter>();
        textContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        textContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        textField.transform.position = wpParentRectTransform.position;
        textField.name = wpParentRectTransform.name + "-Text";
        textField.fontSize = textSize;
        textField.font = textFont;
        textField.color = textColor;

        textField.rectTransform.localScale = new Vector3(1, 1, 1);
        textGameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        textGameObject.transform.SetSiblingIndex(textDepth);


        //Set CurvedUI effect manually
        //textGameObject.AddComponent<CurvedUIVertexEffect>();
        //textGameObject.AddComponent<CurvedUITMP>();


        waypointTextCreated = true;
    }
    #endregion


    #region Fade Sprite with Range
    void FadeSpriteWithRange()
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        displayRangeDifference = displayRangeMax - displayRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointDist < displayRangeMax && waypointDist > displayRangeMin)
        {
            //Alpha in real time based off dist
            //iconAlphaValue = 1 - (waypointDist / displayRangeMax); //Derrived from "Waypoint Math" on Google Sheets
            iconAlphaValue = 1 - ((waypointDist - displayRangeMin) / displayRangeDifference);

            //Adjust Alpha
            spriteIndicatorColor.a = iconAlphaValue;
            spriteIndicatorImage.color = spriteIndicatorColor;
        }
        else //Waypoint is beyond the player preset, so set scale
        {
            //Alpha set to 0
            iconAlphaValue = 0;
            spriteIndicatorColor.a = iconAlphaValue;
            spriteIndicatorImage.color = spriteIndicatorColor;
        }
    }
    #endregion


    #region Fade Game Object with Range
    void FadeGameObjectWithRange()
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        displayRangeDifference = displayRangeMax - displayRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointDist < displayRangeMax && waypointDist > displayRangeMin)
        {
            //Alpha in real time based off dist
            //gameObjectIndicatorCanvasGroup.alpha = 1 - (waypointDist / displayRangeMax); //Derrived from "Waypoint Math" on Google Sheets
            gameObjectIndicatorCanvasGroup.alpha = 1 - ((waypointDist - displayRangeMin) / displayRangeDifference);
        }
        else //Waypoint is beyond the player preset, so set scale
        {
            //Alpha set to 0
            gameObjectIndicatorCanvasGroup.alpha = 0f;
        }

    }
    #endregion


    #region Scale Sprite with Range
    void ScaleSpriteWithRange(Vector2 OnOffScreenSize, bool reverseScalingSprite) //This should take onScreenSpriteSize and offScreenSpriteSize arguments
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        displayRangeDifference = displayRangeMax - displayRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointDist < displayRangeMax && waypointDist > displayRangeMin)
        {
            //Scale in real time based off dist
            //scaleValueX = OnOffScreenSize.x - ((OnOffScreenSize.x / displayRangeMax) * waypointDist); //Derrived from "Waypoint Math" on Google Sheets
            //scaleValueY = OnOffScreenSize.y - ((OnOffScreenSize.y / displayRangeMax) * waypointDist);
            if (reverseScalingSprite)
            {
                //Gets larger as you get away from it and smaller as you get closer
                scaleValueX = OnOffScreenSize.x * ((waypointDist - displayRangeMin) / displayRangeDifference);
                scaleValueY = OnOffScreenSize.y * ((waypointDist - displayRangeMin) / displayRangeDifference);
            }
            else
            {
                //Gets smaller as you get away from it and larger as you get closer
                scaleValueX = OnOffScreenSize.x * (1 - ((waypointDist - displayRangeMin) / displayRangeDifference));
                scaleValueY = OnOffScreenSize.y * (1 - ((waypointDist - displayRangeMin) / displayRangeDifference));
            }


            //spriteIndicatorRect.localScale = new Vector2(scaleValueX, scaleValueY);
            spriteIndicatorImage.rectTransform.localScale = new Vector3(scaleValueX, scaleValueY, Sprite_Z_Scale);
        }
        else //Waypoint is beyond the player preset, so set scale and alpha to 0
        {
            //Scale set to 0
            spriteIndicatorImage.rectTransform.localScale = new Vector3(0f, 0f, Sprite_Z_Scale);
        }
    }
    #endregion


    #region Scale Game Object with Range
    void ScaleGameObjectWithRange(float OnOffScreenSize, bool reverseScalingGameObject) //This should take onScreenSpriteSize and offScreenSpriteSize arguments
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        displayRangeDifference = displayRangeMax - displayRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointDist < displayRangeMax && waypointDist > displayRangeMin)
        {
            //Scale in real time based off dist
            //scaleValueGameObjectX = OnOffScreenSize - ((OnOffScreenSize / displayRangeMax) * waypointDist); //Derrived from "Waypoint Math" on Google Sheets
            //scaleValueGameObjectY = OnOffScreenSize - ((OnOffScreenSize / displayRangeMax) * waypointDist);
            if (reverseScalingGameObject)
            {
                //Gets larger as you get away from it and smaller as you get closer
                scaleValueGameObjectX = OnOffScreenSize * ((waypointDist - displayRangeMin) / displayRangeDifference);
                scaleValueGameObjectY = OnOffScreenSize * ((waypointDist - displayRangeMin) / displayRangeDifference);
            }
            else
            {
                //Gets smaller as you get away from it and larger as you get closer
                scaleValueGameObjectX = OnOffScreenSize * (1 - ((waypointDist - displayRangeMin) / displayRangeDifference));
                scaleValueGameObjectY = OnOffScreenSize * (1 - ((waypointDist - displayRangeMin) / displayRangeDifference));
            }

            gameObjectIndicator.transform.localScale = new Vector3(scaleValueGameObjectX, scaleValueGameObjectY, Prefab_Z_Scale);
        }
        else //Waypoint is beyond the player preset, so set scale and alpha to 0
        {
            //Scale set to 0
            gameObjectIndicator.transform.localScale = new Vector3(0, 0, Prefab_Z_Scale);
        }
    }
    #endregion


    #region Destroy Parent, Sprite, Game Object & Text
    //DESTROY WAYPOINT PARENT
    void DestroyWaypointParent()
    {
        if (wpParentGameObject != null)
        {
            Destroy(wpParentGameObject);
        }
    }

    //DESTROY WAYPOINT SPRITE
    void DestroyWaypointIcon()
    {
        GameObject waypointIconSprite = GameObject.Find(wpParentRectTransform.name + "-Sprite");
        if (waypointIconSprite != null)
        {
            Destroy(waypointIconSprite);
        }
    }

    //DESTROY WAYPOINT GAME OBJECT
    void DestroyWaypointGameObject()
    {
        GameObject waypointIconGameObject = GameObject.Find(wpParentRectTransform.name + "-Prefab");
        if (waypointIconGameObject != null)
        {
            Destroy(waypointIconGameObject);
        }
    }

    //DESTROY WAYPOINT TEXT
    void DestroyWaypointText()
    {
        GameObject waypointText = GameObject.Find(wpParentRectTransform.name + "-Text");
        if (waypointText != null)
        {
            Destroy(waypointText);
        }
    }
    #endregion


    #region Check is Parent is On/Off screen
    //Check if parent is on or off-screen then set the game object status accordingly
    //This ensures the right Game Object indicator shows for on or off screen when enabled and disabled during run time
    void SetGameObjectIndicatorStatus()
    {
        if (parentOnScreen)
        {
            gameObjectIndicatorOnOffScreenStatus = 0;
        }

        if (!parentOnScreen)
        {
            gameObjectIndicatorOnOffScreenStatus = 1;
        }
    }
    #endregion

    #endregion




    #region Centered Tracking Functions

    #region Draw Gizmos
    private void OnDrawGizmos()
    {
        if (enableRadiusGizmo)
        {
            Gizmos.color = radiusGizmoColor;
            Gizmos.DrawWireSphere(wpCenteredParentPos, radiusGizmoSize);
        }


    }
    #endregion


    #region Centered Parent
    //CREATE PARENT UI 
    void InstantiateCenteredWaypointParent()
    {
        if (mainCanvas != null)
        {
            wpCenteredParentGameObject = new GameObject();
            wpCenteredParentGameObject.layer = 2;
            wpCenteredParentRectTransform = wpCenteredParentGameObject.AddComponent<RectTransform>();
            wpCenteredParentRectTransform.transform.SetParent(mainCanvas.transform);
            wpCenteredParentRectTransform.name = gameObject.name + "-WPC-" + GetInstanceID();
            wpCenteredParentImage = wpCenteredParentRectTransform.gameObject.AddComponent<Image>();
            wpCenteredParentImage.color = diameterColor;
            wpCenteredParentRectTransform.position = new Vector3(-1000f, -1000f, 0f); //This make sure the blinking box doesn't appear in bottom left at spawn


            //Set CurvedUI effect manually
            //wpCenteredParentGameObject.AddComponent<CurvedUIVertexEffect>();


            //Toggle Visibility - Checks WPI_Manager to see if this Waypoint should be enabled or disabled after being created
            if (WPI_Manager.waypoint_indicators_are_visible)
            {
                wpCenteredParentGameObject.SetActive(true);
            }
            else
            {
                wpCenteredParentGameObject.SetActive(false);
            }

        }

    }
    #endregion


    #region Centerd Sprite
    //CREATE ICON UI 
    void InstantiateCenteredWaypointIcon()
    {
        if (onScreenCenteredSprite != null)
        {
            iconCenteredGameObject = new GameObject();
            iconCenteredGameObject.layer = 2;
            iconCentered = iconCenteredGameObject.AddComponent<RectTransform>();
            iconCentered.position = wpCenteredParentRectTransform.position;
            iconCenteredGameObject.transform.SetParent(wpCenteredParentGameObject.transform);
            iconCentered.name = wpCenteredParentRectTransform.name + "-Sprite";
            iconCenteredImage = iconCentered.gameObject.AddComponent<Image>();
            iconCenteredImage.sprite = onScreenCenteredSprite;
            iconCenteredImage.rectTransform.localScale = new Vector2(newOnScreenCenteredSize.x, newOnScreenCenteredSize.y); //user set scale

            //Set the iconCenteredColorarency of an image component that conatains sprites/textures
            iconCenteredColor = iconCenteredImage.color;
            iconCenteredColor.a = 1f; //Default is transparent on spawn
            iconCenteredImage.color = iconCenteredColor;


            //Set CurvedUI effect manually
            //iconCenteredGameObject.AddComponent<CurvedUIVertexEffect>();


            waypointIconCenteredCreated = true;
        }
    }
    #endregion


    #region Centered Prefab
    //CENTERED PREFAB
    void InstantiateCenteredWaypointPrefab()
    {
        if (onScreenCenteredPrefab != null)
        {
            centeredPrefabIndicator = Instantiate(onScreenCenteredPrefab, wpCenteredParentPos, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg)) as GameObject;
            centeredPrefabIndicator.layer = 2;
            centeredPrefabIndicator.transform.position = wpCenteredParentRectTransform.position;
            centeredPrefabIndicator.transform.SetParent(wpCenteredParentGameObject.transform);
            centeredPrefabIndicator.name = wpCenteredParentRectTransform.name + "-Prefab";

            #region Set Child Object's Sprite Color - (Ref only) This means nothing as it re-instantiates form on to off screen
            //Get the game objcet's child that houses the sprite (index 0)
            centeredPrefabIndicatorChildGameObject = centeredPrefabIndicator.transform.GetChild(0).gameObject;
            centeredPrefabIndicatorChildRect = centeredPrefabIndicatorChildGameObject.GetComponent<RectTransform>();
            centeredPrefabIndicatorChildImg = centeredPrefabIndicatorChildRect.GetComponent<RawImage>();

            //Set the color of the Image
            centeredPrefabIndicatorChildIndicatorColor = new Color32(0, 0, 0, 255);
            centeredPrefabIndicatorChildIndicatorColor.a = 1f; //Default is transparent on spawn
            centeredPrefabIndicatorChildImg.color = gameObjectIndicatorChildIndicatorColor;
            #endregion

            centeredPrefabIndicatorCanvasGroup = centeredPrefabIndicator.AddComponent<CanvasGroup>();
            centeredPrefabIndicatorCanvasGroup.alpha = 1f;
            centeredPrefabIndicatorCanvasGroup.blocksRaycasts = false;
            centeredPrefabIndicatorCanvasGroup.interactable = false;

            centeredPrefabIndicator.transform.SetSiblingIndex(onScreenCenteredPrefabDepth);


            //Set CurvedUI effect manually
            //centeredPrefabIndicator.AddComponent<CurvedUIVertexEffect>();
            //centeredPrefabIndicatorChildGameObject.AddComponent<CurvedUIVertexEffect>();


            centeredPrefabIndicatorCreated = true;
        }

    }
    #endregion


    #region Fade Centered Sprite with Range
    void FadeWithRangeCentered()
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        onScreenCenteredDisplayRangeDifference = onScreenCenteredRangeMax - onScreenCenteredRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin)
        {
            //Alpha in real time based off dist
            //iconCenteredAlphaValue = 1 - (waypointCenteredDist / onScreenCenteredRangeMax); //Derrived from "Waypoint Math" on Google Sheets
            iconCenteredAlphaValue = 1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);

            //Adjust Alpha
            iconCenteredColor.a = iconCenteredAlphaValue;
            iconCenteredImage.color = iconCenteredColor;
        }
        else //Waypoint is beyond the player preset, so set scale
        {
            //Alpha set to 0
            iconCenteredAlphaValue = 0;
            iconCenteredColor.a = iconCenteredAlphaValue;
            iconCenteredImage.color = iconCenteredColor;
        }

    }
    #endregion


    #region Fade Centered Prefab with Range
    void FadeCenteredPrefabWithRange()
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        onScreenCenteredDisplayRangeDifference = onScreenCenteredRangeMax - onScreenCenteredRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin)
        {
            //Alpha in real time based off dist
            centeredPrefabIndicatorCanvasGroup.alpha = 1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);
        }
        else //Waypoint is beyond the player preset, so set scale
        {
            //Alpha set to 0
            centeredPrefabIndicatorCanvasGroup.alpha = 0f;
        }

    }
    #endregion


    #region Scale Centered Sprite with Range
    void ScaleWithRangeCentered(Vector2 OnOffScreenSize, bool reverseScalingSprite) //This should take onScreenCenteredSpriteSize and offScreenSize arguments
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        onScreenCenteredDisplayRangeDifference = onScreenCenteredRangeMax - onScreenCenteredRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin)
        {
            //Scale in real time based off dist
            //centeredScaleValueX = OnOffScreenSize.x - ((OnOffScreenSize.x / onScreenCenteredRangeMax) * waypointCenteredDist); //Derrived from "Waypoint Math" on Google Sheets
            //centeredScaleValueY = OnOffScreenSize.y - ((OnOffScreenSize.y / onScreenCenteredRangeMax) * waypointCenteredDist);

            if (reverseScalingSprite)
            {
                //Gets larger as you get away from it and smaller as you get closer
                centeredScaleValueX = OnOffScreenSize.x * ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);
                centeredScaleValueY = OnOffScreenSize.y * ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);
            }
            else
            {
                //Gets smaller as you get away from it and larger as you get closer
                centeredScaleValueX = OnOffScreenSize.x * (1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference));
                centeredScaleValueY = OnOffScreenSize.y * (1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference));
            }

            //iconCentered.localScale = new Vector2(centeredScaleValueX, centeredScaleValueY);
            iconCenteredImage.rectTransform.localScale = new Vector2(centeredScaleValueX, centeredScaleValueY);
        }
        else //Waypoint is beyond the player preset, so set scale and alpha to 0
        {
            //Scale set to 0
            iconCenteredImage.rectTransform.localScale = new Vector2(0f, 0f);
        }
    }
    #endregion


    #region Scale Centered Prefab with Range
    void ScaleCenteredPrefabWithRange(float OnOffScreenSize, bool reverseScalingPrefab)
    {
        //Formula
        //1. Get displayRangeDifference (displayRangeMax - displayRangeMin)
        onScreenCenteredDisplayRangeDifference = onScreenCenteredRangeMax - onScreenCenteredRangeMin;
        //2. Use this to determine 1-0 values for alpha/scale
        //1 - ((waypointDist - displayRangeMin) / displayRangeDifference)

        if (waypointCenteredDist < onScreenCenteredRangeMax && waypointCenteredDist > onScreenCenteredRangeMin)
        {
            //Scale in real time based off dist
            //scaleValueGameObjectX = OnOffScreenSize - ((OnOffScreenSize / displayRangeMax) * waypointDist); //Derrived from "Waypoint Math" on Google Sheets
            //scaleValueGameObjectY = OnOffScreenSize - ((OnOffScreenSize / displayRangeMax) * waypointDist);
            if (reverseScalingPrefab)
            {
                //Gets larger as you get away from it and smaller as you get closer
                centeredScaleValueX = OnOffScreenSize * ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);
                centeredScaleValueY = OnOffScreenSize * ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference);
            }
            else
            {
                //Gets smaller as you get away from it and larger as you get closer
                centeredScaleValueX = OnOffScreenSize * (1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference));
                centeredScaleValueY = OnOffScreenSize * (1 - ((waypointCenteredDist - onScreenCenteredRangeMin) / onScreenCenteredDisplayRangeDifference));
            }

            centeredPrefabIndicator.transform.localScale = new Vector3(centeredScaleValueX, centeredScaleValueY, 0);
        }
        else //Waypoint is beyond the player preset, so set scale and alpha to 0
        {
            //Scale set to 0
            centeredPrefabIndicator.transform.localScale = new Vector3(0, 0, 0);
        }
    }
    #endregion


    #region Destroy Sprite, Prefab
    //DESTROY CENETERED SPRITE
    void DestroyCenteredSprite()
    {
        GameObject destroyCenteredSprite = GameObject.Find(wpCenteredParentRectTransform.name + "-Sprite");
        //Debug.Log("Destroy: " + wpCenteredParentRectTransform.name);
        if (destroyCenteredSprite != null)
        {
            Destroy(destroyCenteredSprite);
        }
    }

    //DESTROY CENTERED PREFAB
    void DestroyCenteredPrefab()
    {
        GameObject destroyCenteredPrefab = GameObject.Find(wpCenteredParentRectTransform.name + "-Prefab");
        if (destroyCenteredPrefab != null)
        {
            Destroy(destroyCenteredPrefab);
        }
    }
    #endregion

    #endregion


    #region Distance Calculation
    void CalculateDistance()
    {
        #region Check for Distance Calculation Tag Duplicates or Empty Field
        if (!distanceTargetDefined)
        {
            //Check to see if the Tag Name is blank
            if (distCalTargetTag == "" || distCalTargetTag == null || distCalTargetTag == " ")
            {
                //Tag name is blank please add a tag
                Debug.LogWarning("Distance Calculation Tag was left blank on Game Object: \"" + gameObject.name + "\". Using the Camera tagged: \"" + camera_tag_name + "\" for distance calculations by default.");

                distCalTargetTag = camera_tag_name; //Reset tag name to MainCamera
                distCalTarget = GameObject.FindGameObjectWithTag(distCalTargetTag);


            }
            else //Tag name has a value now lets check to see if that value exsists as a tag somewhere
            {

                distCalTargets = GameObject.FindGameObjectsWithTag(distCalTargetTag);


                if (distCalTargets.Length == 1)
                {
                    //We have a match and all is good!
                    //Debug.Log("We have a match and all is good!");
                    distCalTarget = GameObject.FindGameObjectWithTag(distCalTargetTag);

                }
                if (distCalTargets.Length > 1)
                {
                    //There are more than one object of this tag name and here is a list (please make sure just one is labeled)
                    Debug.LogWarning("The tag name \"" + distCalTargetTag + "\" on waypoint_indicator.cs on Game Object: \"" + gameObject.name + "\" is already being used in your scene " + distCalTargets.Length + " times: (see below)");
                    foreach (GameObject distCalTarget in distCalTargets)
                    {
                        Debug.LogWarning("\nGame Object: " + distCalTarget.name + " - Tag: " + distCalTargetTag);
                    }
                    Debug.LogWarning("Using the Camera tagged: \"" + camera_tag_name + "\" for distance calculations by default.");

                    distCalTargetTag = camera_tag_name; //Reset tag name to MainCamera
                    distCalTarget = GameObject.FindGameObjectWithTag(distCalTargetTag);
                }
                if (distCalTargets.Length < 1)
                {
                    //There are no objects in the scene that match this tag, please add one in
                    Debug.LogWarning("Distance Calculation Tag was left blank on Game Object: \"" + gameObject.name + "\". Using the Camera tagged: \"" + camera_tag_name + "\" for distance calculations by default.");

                    distCalTargetTag = camera_tag_name; //Reset tag name to MainCamera
                    distCalTarget = GameObject.FindGameObjectWithTag(distCalTargetTag);
                }

                //Debug.Log("We defined a distance target!");
            }
            distanceTargetDefined = true;
        }
        #endregion
    }
    #endregion


    #region Screen Edge Detect Functions
    void ScreenEdgeDetectTop()
    {
        //Debug.Log("TOP area");
        topEdgeDetected = true;
        botEdgeDetected = false;
        rightEdgeDetected = false;
        leftEdgeDetected = false;
    }

    void ScreenEdgeDetectBot()
    {
        //Debug.Log("BOT area");
        topEdgeDetected = false;
        botEdgeDetected = true;
        rightEdgeDetected = false;
        leftEdgeDetected = false;
    }

    void ScreenEdgeDetectRight()
    {
        //Debug.Log("RIGHT area");
        topEdgeDetected = false;
        botEdgeDetected = false;
        rightEdgeDetected = true;
        leftEdgeDetected = false;
    }

    void ScreenEdgeDetectLeft()
    {
        //Debug.Log("LEFT area");
        topEdgeDetected = false;
        botEdgeDetected = false;
        rightEdgeDetected = false;
        leftEdgeDetected = true;
    }
    #endregion

}
