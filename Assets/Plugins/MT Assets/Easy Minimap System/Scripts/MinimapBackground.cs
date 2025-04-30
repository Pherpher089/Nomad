#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.EasyMinimapSystem
{
    /*
     This class is responsible for the functioning of the "Minimap Background" component, and all its functions.
    */
    /*
     * The Easy Minimap System was developed by Marcos Tomaz in 2019.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [AddComponentMenu("MT Assets/Easy Minimap System/Minimap Background")] //Add this component in a category of addComponent menu
    public class MinimapBackground : MonoBehaviour
    {
        //Private constants
        private const float BASE_HEIGHT_IN_3D_WORLD = 98998; //The default minimum height for all EMS components is 99000, but this one has this minimum height to ALWAYS be in the background of the minimap!

        //Private variables
        private GameObject minimapDataHolderObj;
        private MinimapDataHolder minimapDataHolder;
        private Transform minimapBackgroundHolder;
        private GameObject tempSpriteObj;
        private SpriteRenderer tempSprite;

        //Cache variables
        private Texture2D cacheTexture;
        private BackgroundArea lastBackgroundArea = BackgroundArea.Units2;
        private bool alreadyDidTheFirstResize = false;

        //Enums of script
        public enum BackgroundArea
        {
            Units2,
            Units4,
            Units16,
            Units20,
            Units32,
            Units40,
            Units60,
            Units64,
            Units80,
            Units100,
            Units128,
            Units150,
            Units200,
            Units250,
            Units256,
            Units300,
            Units350,
            Units400,
            Units450,
            Units500,
            Units512,
        }
        public enum CreateBackgroundInSide
        {
            Forward,
            Back,
            Left,
            Right
        }

        //Public variables
        [HideInInspector]
        public BackgroundArea backgroundArea = BackgroundArea.Units40;
        [HideInInspector]
        public Color colorOfBackground = new Color(210.0f / 255.0f, 210.0f / 255.0f, 210.0f / 255.0f, 1);

#if UNITY_EDITOR
        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(MinimapBackground))]
        public class CustomInspector : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                MinimapBackground script = (MinimapBackground)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Support reminder
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Remember to read the Easy Minimap System documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

                GUILayout.Space(10);

                //Set height in 0
                script.gameObject.transform.position = new Vector3(script.gameObject.transform.position.x, 0, script.gameObject.transform.position.z);
                script.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                script.gameObject.transform.localScale = new Vector3(1, 1, 1);

                //Start of settings
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Settings For Minimap Background", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.backgroundArea = (BackgroundArea)EditorGUILayout.EnumPopup(new GUIContent("Background Area",
                        "Size of area to be covered by the background. 1 Unity = 1 Meter."),
                        script.backgroundArea);

                script.colorOfBackground = EditorGUILayout.ColorField(new GUIContent("Color of Background",
                        "The color of this background in minimap."),
                        script.colorOfBackground);
                if (script.colorOfBackground.a < 1.0f)
                    script.colorOfBackground.a = 1.0f;

                //Final space
                GUILayout.Space(10);

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {

                }
            }

            protected virtual void OnSceneGUI()
            {
                //Draw the controls of this Background
                MinimapBackground script = (MinimapBackground)target;

                //If application is running, not render the arrows
                if (Application.isPlaying == true)
                    return;

                //Set the color of controls
                Handles.color = Color.yellow;

                //Get the current position
                Vector3 position = script.gameObject.transform.position;

                //Distance after border of scan
                float distanceAfterEdge = 4;

                //Calculate the 4 points of area
                Vector3 left = new Vector3(position.x - distanceAfterEdge, 0, position.z + script.GetSelectedBackgroundArea() / 2);
                Vector3 right = new Vector3(position.x + script.GetSelectedBackgroundArea() + distanceAfterEdge, 0, position.z + script.GetSelectedBackgroundArea() / 2);
                Vector3 top = new Vector3(position.x + script.GetSelectedBackgroundArea() / 2, 0, position.z + script.GetSelectedBackgroundArea() + distanceAfterEdge);
                Vector3 bottom = new Vector3(position.x + script.GetSelectedBackgroundArea() / 2, 0, position.z - distanceAfterEdge);

                //Calculate the size of button, taking account the current camera distance
                float size = Vector3.Distance(Camera.current.transform.transform.position, new Vector3(position.x + script.GetSelectedBackgroundArea() / 2, 0, position.z + script.GetSelectedBackgroundArea() / 2)) * 0.08f;

                //The top buttom
                if (Handles.Button(top, Quaternion.Euler(0, 0, 0), size, size, Handles.ArrowHandleCap) == true)
                    script.CreateNewMinimapBackgroundBeside(CreateBackgroundInSide.Forward);

                //The right buttom
                if (Handles.Button(right, Quaternion.Euler(0, 90, 0), size, size, Handles.ArrowHandleCap) == true)
                    script.CreateNewMinimapBackgroundBeside(CreateBackgroundInSide.Right);

                //The left buttom
                if (Handles.Button(left, Quaternion.Euler(0, 270, 0), size, size, Handles.ArrowHandleCap) == true)
                    script.CreateNewMinimapBackgroundBeside(CreateBackgroundInSide.Left);

                //The bottom buttom
                if (Handles.Button(bottom, Quaternion.Euler(0, 180, 0), size, size, Handles.ArrowHandleCap) == true)
                    script.CreateNewMinimapBackgroundBeside(CreateBackgroundInSide.Back);
            }
        }

        public void OnDrawGizmosSelected()
        {
            //Set color of gizmos
            Gizmos.color = colorOfBackground;
            Handles.color = colorOfBackground;

            //Get the current position
            Vector3 position = this.gameObject.transform.position;

            //Calculate the 4 points of area
            Vector3 bottomLeft = new Vector3(position.x, 0, position.z);
            Vector3 topLeft = new Vector3(position.x, 0, position.z + GetSelectedBackgroundArea());
            Vector3 bottomRight = new Vector3(position.x + GetSelectedBackgroundArea(), 0, position.z);
            Vector3 topRight = new Vector3(position.x + GetSelectedBackgroundArea(), 0, position.z + GetSelectedBackgroundArea());
            Vector3 center = new Vector3(position.x + GetSelectedBackgroundArea() / 2, 0, position.z + GetSelectedBackgroundArea() / 2);

            //Show the 4 points
            Gizmos.DrawSphere(bottomLeft, 0.5f);
            Gizmos.DrawSphere(topLeft, 0.5f);
            Gizmos.DrawSphere(bottomRight, 0.5f);
            Gizmos.DrawSphere(topRight, 0.5f);
            Gizmos.DrawWireSphere(center, 0.5f);

            //Show the lines and background area
            Handles.DrawAAPolyLine(5f, new Vector3[] { bottomLeft, topLeft, topRight, bottomRight, bottomLeft });
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { bottomLeft, topLeft, topRight, bottomRight }, colorOfBackground, new Color(0, 0, 0, 0));

            //Draw meters indicator
            Handles.Label(topLeft, "(" + GetSelectedBackgroundArea().ToString() + " Units)");
            Handles.Label(bottomRight, "(" + GetSelectedBackgroundArea().ToString() + " Units)");
        }
        #endregion
#endif

        //Core methods

        private float GetSelectedBackgroundArea()
        {
            //Return the value of selected background area, in enum
            switch (backgroundArea)
            {
                case BackgroundArea.Units2:
                    return 2;
                case BackgroundArea.Units4:
                    return 4;
                case BackgroundArea.Units16:
                    return 16;
                case BackgroundArea.Units20:
                    return 20;
                case BackgroundArea.Units32:
                    return 32;
                case BackgroundArea.Units40:
                    return 40;
                case BackgroundArea.Units60:
                    return 60;
                case BackgroundArea.Units64:
                    return 64;
                case BackgroundArea.Units80:
                    return 80;
                case BackgroundArea.Units100:
                    return 100;
                case BackgroundArea.Units128:
                    return 128;
                case BackgroundArea.Units150:
                    return 150;
                case BackgroundArea.Units200:
                    return 200;
                case BackgroundArea.Units250:
                    return 250;
                case BackgroundArea.Units256:
                    return 256;
                case BackgroundArea.Units300:
                    return 300;
                case BackgroundArea.Units350:
                    return 350;
                case BackgroundArea.Units400:
                    return 400;
                case BackgroundArea.Units450:
                    return 450;
                case BackgroundArea.Units500:
                    return 500;
                case BackgroundArea.Units512:
                    return 512;
            }
            return 0;
        }

        public void Awake()
        {
            //Create the holder, if not exists
            minimapDataHolderObj = GameObject.Find("Minimap Data Holder");
            if (minimapDataHolderObj == null)
            {
                minimapDataHolderObj = new GameObject("Minimap Data Holder");
                minimapDataHolder = minimapDataHolderObj.AddComponent<MinimapDataHolder>();
            }
            if (minimapDataHolderObj != null)
                minimapDataHolder = minimapDataHolderObj.GetComponent<MinimapDataHolder>();
            minimapBackgroundHolder = minimapDataHolderObj.transform.Find("Minimap Backgrounds Holder");
            if (minimapBackgroundHolder == null)
            {
                GameObject obj = new GameObject("Minimap Backgrounds Holder");
                minimapBackgroundHolder = obj.transform;
                minimapBackgroundHolder.SetParent(minimapDataHolderObj.transform);
                minimapBackgroundHolder.localPosition = Vector3.zero;
                minimapBackgroundHolder.localEulerAngles = Vector3.zero;
            }
            if (minimapDataHolder.instancesOfMinimapBackgroundInThisScene.Contains(this) == false)
                minimapDataHolder.instancesOfMinimapBackgroundInThisScene.Add(this);

            //Create the background
            tempSpriteObj = new GameObject("Minimap Background (" + this.gameObject.name + ")");
            tempSpriteObj.transform.SetParent(minimapBackgroundHolder);
            tempSpriteObj.transform.position = new Vector3(this.gameObject.transform.position.x, BASE_HEIGHT_IN_3D_WORLD, this.gameObject.transform.position.z);
            tempSprite = tempSpriteObj.AddComponent<SpriteRenderer>();
            tempSprite.sortingOrder = 0;
            tempSpriteObj.layer = LayerMask.NameToLayer("UI");
            //Create the texture to use as background
            cacheTexture = new Texture2D(64, 64, TextureFormat.RGB24, 0, true);
            Color32[] pixels = new Color32[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(1, 1, 1, 1);
            cacheTexture.SetPixels32(0, 0, 64, 64, pixels, 0);
            //Create the sprite to use as background
            tempSprite.sprite = Sprite.Create(cacheTexture, new Rect(0.0f, 0.0f, cacheTexture.width, cacheTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

            //Add the activity monitor to the background
            ActivityMonitor activityMonitor = tempSpriteObj.AddComponent<ActivityMonitor>();
            activityMonitor.responsibleScriptComponentForThis = this;
        }

        public void Update()
        {
            //If the Minimap Background created by this component is disabled, enable it
            if (tempSpriteObj.activeSelf == false)
                tempSpriteObj.SetActive(true);

            //Resize the sprite of background to follow the background area defined
            if (alreadyDidTheFirstResize == false || backgroundArea != lastBackgroundArea)
            {
                //Calculates the size of background sprite, taking the pixels per unit into account
                float areaResolutionAspect = (float)GetSelectedBackgroundArea() / (float)cacheTexture.width;
                float finalSize = areaResolutionAspect * tempSprite.sprite.pixelsPerUnit;

                //Set the size of scan in minimap
                tempSpriteObj.transform.localScale = new Vector3(finalSize, finalSize, 1);

                //Save the cache
                alreadyDidTheFirstResize = true;
                lastBackgroundArea = backgroundArea;
            }

            //Updates a value, if changes
            if (colorOfBackground != tempSprite.color)
                tempSprite.color = colorOfBackground;

            //Set height and rotation in zero
            this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
            this.transform.rotation = Quaternion.Euler(0, 0, 0);
            this.transform.localScale = new Vector3(1, 1, 1);

            //Move the scan to follow this gameobject
            tempSpriteObj.transform.position = new Vector3(this.gameObject.transform.position.x + GetSelectedBackgroundArea() / 2, BASE_HEIGHT_IN_3D_WORLD, this.gameObject.transform.position.z + GetSelectedBackgroundArea() / 2);
            //Rotate the scan
            tempSpriteObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        //Public methods

        public MinimapBackground CreateNewMinimapBackgroundBeside(CreateBackgroundInSide sideToCreateNewBackground)
        {
            //Get the new position
            Vector3 positionForNewBackground = new Vector3(this.gameObject.transform.position.x, 0, this.gameObject.transform.position.z);
            switch (sideToCreateNewBackground)
            {
                case CreateBackgroundInSide.Forward:
                    positionForNewBackground.z += GetSelectedBackgroundArea();
                    break;
                case CreateBackgroundInSide.Right:
                    positionForNewBackground.x += GetSelectedBackgroundArea();
                    break;
                case CreateBackgroundInSide.Left:
                    positionForNewBackground.x -= GetSelectedBackgroundArea();
                    break;
                case CreateBackgroundInSide.Back:
                    positionForNewBackground.z -= GetSelectedBackgroundArea();
                    break;
            }

            //Create new GameObject, select then, and add Minimap Background
            GameObject newBg = new GameObject(this.gameObject.transform.name);
            newBg.transform.SetParent(this.gameObject.transform.parent);
            MinimapBackground mBackground = newBg.AddComponent<MinimapBackground>();
            mBackground.backgroundArea = this.backgroundArea;
            mBackground.colorOfBackground = this.colorOfBackground;

            //Set the position
            newBg.transform.position = positionForNewBackground;

#if UNITY_EDITOR
            //Register the gameobject created for undo
            Undo.RegisterCreatedObjectUndo(newBg, "Background Created");

            //Select the new Scanner
            if (Application.isPlaying == false)
                Selection.activeGameObject = newBg;
#endif

            //Return the created Minimap Background
            return mBackground;
        }

        public bool isThisMinimapBackgroundBeingVisibleByAnyMinimapCamera()
        {
            //This method will return true if this Minimap Background is being visualized by any minimap camera
            bool isVisible = false;
            if (tempSprite.isVisible == true)
                isVisible = true;
            return isVisible;
        }

        public SpriteRenderer GetGeneratedSpriteAtRunTime()
        {
            //Return the sprite generated at runtime by this component
            return tempSprite;
        }

        public MinimapBackground[] GetListOfAllMinimapBackgroundsInThisScene()
        {
            //If is not playing, cancel
            if (Application.isPlaying == false)
            {
                Debug.LogError("It is only possible to obtain the list of Minimap Backgrounds in this scene, if the application is being executed.");
                return null;
            }

            //Return a list that contains reference to all of this component in this scene
            return minimapDataHolder.instancesOfMinimapBackgroundInThisScene.ToArray();
        }
    }
}