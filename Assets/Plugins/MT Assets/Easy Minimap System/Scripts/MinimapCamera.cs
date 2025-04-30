#if UNITY_EDITOR
using UnityEditor;
using System;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if EMS_URP_On
using UnityEngine.Rendering.Universal;
#endif
#if EMS_HDRP_On
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MTAssets.EasyMinimapSystem
{
    /*
     This class is responsible for the functioning of the "Minimap Camera" component, and all its functions.
    */
    /*
     * The Easy Minimap System was developed by Marcos Tomaz in 2019.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [AddComponentMenu("MT Assets/Easy Minimap System/Minimap Camera")] //Add this component in a category of addComponent menu
    public class MinimapCamera : MonoBehaviour
    {
        //Private constants
        private const float BASE_HEIGHT_IN_3D_WORLD = 99010;

        //Private variables
        private GameObject minimapDataHolderObj;
        private MinimapDataHolder minimapDataHolder;
        private Transform minimapCamerasHolder;
        private GameObject tempCameraObj;
        private Camera tempCamera;

        //Cache variables
        private CaptureShape lastCaptureShape = CaptureShape.Square;
        private CaptureResolutionSquare lastCaptureResolutionSquare = CaptureResolutionSquare.pixels256x256;
        private CaptureResolutionRectangular lastCaptureResolutionRectangular = CaptureResolutionRectangular.pixels426x240;
        private ImageProcessing lastImageProcessing = ImageProcessing.Disabled;
        private SkipShadow lastSkipShadow = SkipShadow.No;
        private SkipFog lastSkipFog = SkipFog.No;
        private SkipGlow lastSkipGlow = SkipGlow.No;
        private bool hasDefinedStartingConfigOfCaptureMode = false;
        private CaptureMode lastCaptureMode = CaptureMode.TopView2D;
        private int lastCaptureModeLayersToCaptureValue = -2;
#if EMS_URP_On
        private bool urp_currentShadowSetting = false;
        private bool urp_currentFogSetting = false;
        private Bloom urp_currentGlow = null;
        private bool urp_currentGlowSetting = false;
#endif
#if EMS_HDRP_On
        private HDAdditionalCameraData hdrp_additionalCameraData = null;
        private bool hdrp_currentShadowSetting = false;
        private Fog hdrp_currentFog = null;
        private bool hdrp_currentFogSetting = false;
#endif
        private UnityEngine.ShadowQuality birp_currentShadowsSetting = UnityEngine.ShadowQuality.Disable;
        private bool birp_currentFogSetting = false;
        private bool birp_currentGlowSetting = false;

        //Enums of script
        public enum CaptureShape
        {
            Square,
            Rectangular
        }
        public enum CaptureResolutionSquare
        {
            pixels256x256,
            pixels512x512,
            pixels1024x1024,
            pixels2048x2048,
            pixels4096x4096,
            pixels8192x8192
        }
        public enum CaptureResolutionRectangular
        {
            pixels426x240,
            pixels480x360,
            pixels640x480,
            pixels1280x720,
            pixels1920x1080,
            pixels2560x1440,
            pixels3840x2160,
            pixels7680x4320
        }
        public enum CaptureMode
        {
            TopView2D,
            TopView3D
        }
        public enum FollowRotationOf
        {
            ThisGameObject,
            CustomGameObject
        }
        public enum ImageProcessing
        {
            Disabled,
            Enabled
        }
        public enum SkipShadow
        {
            No,
            Yes
        }
        public enum SkipFog
        {
            No,
            Yes
        }
        public enum SkipGlow
        {
            No,
            Yes
        }
        public enum SkipColorGrading
        {
            No,
            Yes
        }
        public enum SkipTonemapping
        {
            No,
            Yes
        }

        //Public variables
        ///<summary>[WARNING] Do not change the value of this variable. This is a variable used for internal tool operations.</summary> 
        [HideInInspector]
        public RenderTexture renderTexture;
        [HideInInspector]
        public CaptureShape captureShape;
        [HideInInspector]
        public CaptureResolutionSquare captureResolutionSquare = CaptureResolutionSquare.pixels256x256;
        [HideInInspector]
        public CaptureResolutionRectangular captureResolutionRectangular = CaptureResolutionRectangular.pixels640x480;
        [HideInInspector]
        public ImageProcessing imageProcessing = ImageProcessing.Disabled;
#if EMS_URP_On || EMS_HDRP_On
        [HideInInspector]
        public Volume referenceVolume = null;
#endif
        [HideInInspector]
        public SkipShadow skipShadowInCapture = SkipShadow.No;
        [HideInInspector]
        public SkipFog skipFogInCapture = SkipFog.No;
        [HideInInspector]
        public SkipGlow skipGlowInCapture = SkipGlow.No;
        [HideInInspector]
        public SkipColorGrading skipColorGradingInCapture = SkipColorGrading.No;
        [HideInInspector]
        public SkipTonemapping skipTonemappingInCapture = SkipTonemapping.No;
        [HideInInspector]
        public CaptureMode captureMode = CaptureMode.TopView2D;
        [HideInInspector]
        public LayerMask layersToCaptureIn3d = new LayerMask();
        [HideInInspector]
        public float fieldOfView = 20;
        [HideInInspector]
        public FollowRotationOf followRotationOf = FollowRotationOf.ThisGameObject;
        [HideInInspector]
        public Transform customGameObjectToFollowRotation = null;
        [HideInInspector]
        public float movementsSmoothing = 14;

#if UNITY_EDITOR
        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(MinimapCamera))]
        public class CustomInspector : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                MinimapCamera script = (MinimapCamera)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Support reminder
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Remember to read the Easy Minimap System documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

                //Now that all components are validated, execution continues. ----------
                GUILayout.Space(10);

                //Start of settings
                EditorGUILayout.LabelField("Capture Quality Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.captureShape = (CaptureShape)EditorGUILayout.EnumPopup(new GUIContent("Capture Shape",
                                    "The format in which this Minimap Camera will capture the image.\n\nIf your Minimap Renderer is square (same height and width), use the \"Square\" format. If your Minimap Renderer is wider than the width, use the \"Rectangular\" format."),
                                    script.captureShape);
                if (script.captureShape == CaptureShape.Square)
                    script.captureResolutionSquare = (CaptureResolutionSquare)EditorGUILayout.EnumPopup(new GUIContent("Capture Resolution",
                                    "The higher the resolution, the higher the image quality of this camera will capture. However, more performance will be spent.\n\nThe lower the resolution, the worse the image quality will be, but the performance will be higher."),
                                    script.captureResolutionSquare);
                if (script.captureShape == CaptureShape.Rectangular)
                    script.captureResolutionRectangular = (CaptureResolutionRectangular)EditorGUILayout.EnumPopup(new GUIContent("Capture Resolution",
                                    "The higher the resolution, the higher the image quality of this camera will capture. However, more performance will be spent.\n\nThe lower the resolution, the worse the image quality will be, but the performance will be higher."),
                                    script.captureResolutionRectangular);

                script.imageProcessing = (ImageProcessing)EditorGUILayout.EnumPopup(new GUIContent("Image Processing",
                                    "This parameter allows you to edit how some Elements or Image Effects appear in what is captured by this component.\n\nNOTE: Using this feature may impact performance."),
                                    script.imageProcessing);
                if (script.imageProcessing == ImageProcessing.Enabled)
                {
                    EditorGUI.indentLevel += 1;
#if EMS_URP_On || EMS_HDRP_On
                    script.referenceVolume = (Volume)EditorGUILayout.ObjectField(new GUIContent("Reference Volume",
                        "The Volume component to be used as a reference for Image Processing action. This variable only needs to be provided if you use URP or HDRP in your project. In addition, the Volume component provided must be of the Global type, which is capable of affecting the Minimap elements."),
                        script.referenceVolume, typeof(Volume), true, GUILayout.Height(16));
#endif

                    script.skipShadowInCapture = (SkipShadow)EditorGUILayout.EnumPopup(new GUIContent("Skip Shadow",
                        "This parameter tells whether what is captured by this component should skip Shadow processing.\n\nKeep in mind that even if this parameter is disabled, Shadow will only appear in what is captured if it is enabled in the Render Pipeline or Scene settings."),
                        script.skipShadowInCapture);

                    script.skipFogInCapture = (SkipFog)EditorGUILayout.EnumPopup(new GUIContent("Skip Fog",
                        "This parameter tells whether what is captured by this component should skip Fog processing.\n\nKeep in mind that even if this parameter is disabled, Fog will only appear in what is captured if it is enabled in the Render Pipeline or Scene settings."),
                        script.skipFogInCapture);

                    script.skipGlowInCapture = (SkipGlow)EditorGUILayout.EnumPopup(new GUIContent("Skip Glow",
                        "This parameter tells whether what is captured by this component should skip Glow processing.\n\nKeep in mind that even if this parameter is disabled, Glow will only appear in what is captured if it is enabled in the Render Pipeline or Scene settings."),
                        script.skipGlowInCapture);

                    script.skipColorGradingInCapture = (SkipColorGrading)EditorGUILayout.EnumPopup(new GUIContent("Skip Color Grading",
                        "This parameter tells whether what is captured by this component should skip Color Grading processing.\n\nKeep in mind that even if this parameter is disabled, Color Grading will only appear in what is captured if it is enabled in the Render Pipeline or Scene settings."),
                        script.skipColorGradingInCapture);

                    script.skipTonemappingInCapture = (SkipTonemapping)EditorGUILayout.EnumPopup(new GUIContent("Skip Tonemapping",
                        "This parameter tells whether what is captured by this component should skip Tonemapping processing.\n\nKeep in mind that even if this parameter is disabled, Tonemapping will only appear in what is captured if it is enabled in the Render Pipeline or Scene settings."),
                        script.skipTonemappingInCapture);
                    EditorGUI.indentLevel -= 1;
                }

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Settings For Minimap Camera", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.captureMode = (CaptureMode)EditorGUILayout.EnumPopup(new GUIContent("Capture Mode",
                                    "This variable defines the mode in which the camera will capture the scene, minimap items, etc. Any 2D capture mode will ALWAYS be faster to process than 3D capture modes.\n\nThe 3D capture modes work like a conventional camera and will capture minimap items and your scene like a normal camera. This means that it will capture 3D meshes of your scene, characters and so on.\n\nBriefly: The 2D capture modes can only capture items from Minimap, such as Minimap Scanners, Minimap Items, Minimap Text, etc. The 3D capture modes can capture items from Minimap and the scenario itens of your scene, in realtime as well, like a conventional camera. Note that Minimap items will always be rendered above the items in your scene."),
                                    script.captureMode);
                if (script.captureMode == CaptureMode.TopView3D)
                {
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.HelpBox("This capture mode can consume considerable performance, depending on the size of the field of view.", MessageType.Warning);
                    script.layersToCaptureIn3d = LayerMaskField("Layers To Capture", "This variable defines which layers of your scene will be captured by this camera.", script.layersToCaptureIn3d);
                    //If layermask is in nothing, select everything
                    if (script.layersToCaptureIn3d == 0)
                        script.layersToCaptureIn3d = -1;
                    //If UI layer is removed, add UI layer again
                    if (script.IsThisLayerIdInLayerMask(5, script.layersToCaptureIn3d) == false)
                    {
                        script.layersToCaptureIn3d |= (1 << 5);
                        Debug.LogWarning("The \"UI\" layer cannot be removed from the capture of Minimap Camera components.");
                    }
                    EditorGUI.indentLevel -= 1;
                }

                script.fieldOfView = EditorGUILayout.FloatField(new GUIContent("Field Of View",
                     "The size of field of view of this Minimap Camera.\n\nKeep in mind that the larger the field of view, the more items on the minimap will appear smaller. Adjust carefully. Usually a large field of view can be used for full screen maps."),
                     script.fieldOfView);

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Settings For Movement", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.followRotationOf = (FollowRotationOf)EditorGUILayout.EnumPopup(new GUIContent("Follow Rotation Of",
                                    "Choose a GameObject for this Minimap component to follow its rotation.\n\nThis GameObject - This minimap item will follow the rotation of this GameObject.\n\nCustom GameObject - This minimap item will follow the rotation of another GameObject of your choice."),
                                    script.followRotationOf);
                if (script.followRotationOf == FollowRotationOf.CustomGameObject)
                {
                    EditorGUI.indentLevel += 1;
                    script.customGameObjectToFollowRotation = (Transform)EditorGUILayout.ObjectField(new GUIContent("GameObject To Follow",
                        "This minimap item will follow the rotation of this GameObject."),
                        script.customGameObjectToFollowRotation, typeof(Transform), true, GUILayout.Height(16));
                    EditorGUI.indentLevel -= 1;
                }

                script.movementsSmoothing = EditorGUILayout.Slider(new GUIContent("Movement Smoothing",
                                "The speed at which this Minimap Item will follow GameObjects.\n\nThe higher the smoothing value, the faster this Item will rotate/move to the destination direction."), script.movementsSmoothing, 1f, 100f);

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
                //Draw the controls of this Scan
                MinimapCamera script = (MinimapCamera)target;

                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Get Y angle correspondent of option "Follow Rotation Of"
                float yAnle = script.gameObject.transform.eulerAngles.y;
                if (script.followRotationOf == FollowRotationOf.CustomGameObject && script.customGameObjectToFollowRotation != null)
                    yAnle = script.customGameObjectToFollowRotation.eulerAngles.y;

                //Set the color of controls
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(script.gameObject.transform.position,
                                                     Quaternion.Euler(0, yAnle - 90, 0),
                                                     new Vector3(1, 1, 1));
                Handles.matrix = rotationMatrix;
                Handles.color = Color.blue;

                //Create the resize handler
                script.fieldOfView = Handles.ScaleSlider(script.fieldOfView, new Vector3(0, 0, 0), Vector3.forward, Quaternion.identity, script.fieldOfView, 0);

                //If is negative vallue
                if (script.fieldOfView < 1.0f)
                    script.fieldOfView = 1.0f;

                //Get the center vector
                Vector3 center = new Vector3(0, 0, 0 + script.fieldOfView * 0.6f);

                //Show the field of view
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.contentOffset = new Vector2(-20, 0);
                Handles.Label(center, "Field of View\n(" + script.fieldOfView.ToString("F1") + " Units)", style);

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {
                    //Apply the change, if moved the handle
                    //script.transform.position = teste;
                }
                Repaint();
            }

            static LayerMask LayerMaskField(string label, string tooltip, LayerMask layerMask)
            {
                List<string> layers = new List<string>();
                List<int> layerNumbers = new List<int>();

                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (layerName != "")
                    {
                        layers.Add(layerName);
                        layerNumbers.Add(i);
                    }
                }
                int maskWithoutEmpty = 0;
                for (int i = 0; i < layerNumbers.Count; i++)
                {
                    if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                        maskWithoutEmpty |= (1 << i);
                }
                maskWithoutEmpty = EditorGUILayout.MaskField(new GUIContent(label, tooltip), maskWithoutEmpty, layers.ToArray());
                int mask = 0;
                for (int i = 0; i < layerNumbers.Count; i++)
                {
                    if ((maskWithoutEmpty & (1 << i)) > 0)
                        mask |= (1 << layerNumbers[i]);
                }
                layerMask.value = mask;
                return layerMask;
            }
        }

        public void OnDrawGizmosSelected()
        {
            //Get Y angle correspondent of option "Follow Rotation Of"
            float yAnle = this.gameObject.transform.eulerAngles.y;
            if (followRotationOf == FollowRotationOf.CustomGameObject && customGameObjectToFollowRotation != null)
                yAnle = customGameObjectToFollowRotation.eulerAngles.y;

            //Set color and data of gizmos
            Gizmos.color = Color.blue;
            Handles.color = Color.blue;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(this.gameObject.transform.position,
                                                     Quaternion.Euler(0, yAnle, 0),
                                                     new Vector3(1, 1, 1));
            Handles.matrix = rotationMatrix;
            Gizmos.matrix = rotationMatrix;

            //Get the current position
            Vector3 position = this.gameObject.transform.position;

            //Calculate the 4 points of area
            Vector3 bottomLeft = new Vector3(0 - fieldOfView, 0, 0 - fieldOfView);
            Vector3 topLeft = new Vector3(0 - fieldOfView, 0, 0 + fieldOfView);
            Vector3 bottomRight = new Vector3(0 + fieldOfView, 0, 0 - fieldOfView);
            Vector3 topRight = new Vector3(0 + fieldOfView, 0, 0 + fieldOfView);
            Vector3 center = new Vector3(0, 0, 0);

            //Show the 4 points
            Gizmos.DrawSphere(bottomLeft, 0.1f);
            Gizmos.DrawSphere(topLeft, 0.1f);
            Gizmos.DrawSphere(bottomRight, 0.1f);
            Gizmos.DrawSphere(topRight, 0.1f);
            Gizmos.DrawWireSphere(center, 0.1f);

            //Show the lines
            Handles.DrawAAPolyLine(2f, new Vector3[] { bottomLeft, topLeft, topRight, bottomRight, bottomLeft });
        }
        #endregion
#endif

        //Core methods

        private bool IsThisLayerIdInLayerMask(int layer, LayerMask layerMask)
        {
            //Return true if layer of ID, is in layer mask
            return layerMask == (layerMask | (1 << layer));
        }

        public void Awake()
        {
            //If the render texture are null, create them
            if (renderTexture == null)
            {
                //Calculate width and height of render texture
                Vector2Int resolution = Vector2Int.zero;
                if (captureShape == CaptureShape.Square)
                {
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels256x256)
                        resolution = new Vector2Int(256, 256);
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels512x512)
                        resolution = new Vector2Int(512, 512);
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels1024x1024)
                        resolution = new Vector2Int(1024, 1024);
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels2048x2048)
                        resolution = new Vector2Int(2048, 2048);
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels4096x4096)
                        resolution = new Vector2Int(4096, 4096);
                    if (captureResolutionSquare == CaptureResolutionSquare.pixels8192x8192)
                        resolution = new Vector2Int(8192, 8192);
                }
                if (captureShape == CaptureShape.Rectangular)
                {
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels426x240)
                        resolution = new Vector2Int(426, 240);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels480x360)
                        resolution = new Vector2Int(480, 360);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels640x480)
                        resolution = new Vector2Int(640, 480);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels1280x720)
                        resolution = new Vector2Int(1280, 720);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels1920x1080)
                        resolution = new Vector2Int(1920, 1080);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels2560x1440)
                        resolution = new Vector2Int(2560, 1440);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels3840x2160)
                        resolution = new Vector2Int(3840, 2160);
                    if (captureResolutionRectangular == CaptureResolutionRectangular.pixels7680x4320)
                        resolution = new Vector2Int(7680, 4320);
                }

                //Create a new temporary render texture to fill this minimap camera
                renderTexture = new RenderTexture(resolution.x, resolution.y, 24, RenderTextureFormat.ARGB32);
                renderTexture.anisoLevel = 0;
                renderTexture.antiAliasing = 1;
                renderTexture.autoGenerateMips = false;
                renderTexture.filterMode = FilterMode.Bilinear;
                renderTexture.memorylessMode = RenderTextureMemoryless.None;
                renderTexture.name = "Temp RenderTexture";
                renderTexture.useDynamicScale = false;
                renderTexture.useMipMap = false;
            }

            //If the render texture are not null, and camera is not created, create there
            if (renderTexture != null && tempCameraObj == null)
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
                minimapCamerasHolder = minimapDataHolderObj.transform.Find("Minimap Cameras Holder");
                if (minimapCamerasHolder == null)
                {
                    GameObject obj = new GameObject("Minimap Cameras Holder");
                    minimapCamerasHolder = obj.transform;
                    minimapCamerasHolder.SetParent(minimapDataHolderObj.transform);
                    minimapCamerasHolder.localPosition = Vector3.zero;
                    minimapCamerasHolder.localEulerAngles = Vector3.zero;
                }
                if (minimapDataHolder.instancesOfMinimapCameraInThisScene.Contains(this) == false)
                    minimapDataHolder.instancesOfMinimapCameraInThisScene.Add(this);

                //Create the camera of minimap
                tempCameraObj = new GameObject("Minimap Camera (" + this.gameObject.name + ")");
                tempCameraObj.transform.SetParent(minimapCamerasHolder);
                tempCameraObj.transform.position = new Vector3(this.gameObject.transform.position.x, BASE_HEIGHT_IN_3D_WORLD, this.gameObject.transform.position.z);
                tempCamera = tempCameraObj.AddComponent<Camera>();
                tempCamera.clearFlags = CameraClearFlags.Color;
                tempCamera.backgroundColor = new Color(1, 1, 1, 0);
                tempCamera.orthographic = true;
                tempCamera.nearClipPlane = 0.1f;
                tempCamera.useOcclusionCulling = false;
                tempCamera.allowHDR = false;
                tempCamera.allowMSAA = false;
                tempCamera.allowDynamicResolution = false;
                if (imageProcessing == ImageProcessing.Enabled)
                    InitializeTheImageProcessing();

                //Add the activity monitor to the camera
                ActivityMonitor activityMonitor = tempCameraObj.AddComponent<ActivityMonitor>();
                activityMonitor.responsibleScriptComponentForThis = this;

                //Save the current selected settings on cache
                lastCaptureShape = captureShape;
                lastCaptureResolutionSquare = captureResolutionSquare;
                lastCaptureResolutionRectangular = captureResolutionRectangular;
                hasDefinedStartingConfigOfCaptureMode = false;
            }
        }

        public void Update()
        {
            //If some quality settings was changed, update the render texture
            if (lastCaptureShape != captureShape || lastCaptureResolutionSquare != captureResolutionSquare || lastCaptureResolutionRectangular != captureResolutionRectangular)
            {
                //Remove the render texture of this camera
                renderTexture = null;
                Destroy(tempCameraObj);
                tempCameraObj = null;

                //Create a new one with the new settings (and save the new current selected settings on cache)
                Awake();
            }
            //If the render texture or temporary camera is null, stop
            if (renderTexture == null || tempCameraObj == null)
                return;

            //If image processing was changed, update it
            if (lastImageProcessing != imageProcessing || lastSkipShadow != skipShadowInCapture || lastSkipFog != skipFogInCapture || lastSkipGlow != skipGlowInCapture)
            {
                //If image processing is enabled
                if (imageProcessing == ImageProcessing.Enabled)
                    InitializeTheImageProcessing();
                //If image processing is disabled
                if (imageProcessing == ImageProcessing.Disabled)
                    StopImageProcessing();

                //Set data on cache
                lastImageProcessing = imageProcessing;
                lastSkipShadow = skipShadowInCapture;
                lastSkipFog = skipFogInCapture;
                lastSkipGlow = skipGlowInCapture;
            }

            //If the Minimap Camera created by this component is disabled, enable it
            if (tempCameraObj.activeSelf == false)
                tempCameraObj.SetActive(true);

            //Update the preferences of camera, if has changed
            if (tempCamera.orthographicSize != fieldOfView)
                tempCamera.orthographicSize = fieldOfView;
            //Set the target texture of camera, as the render texture of this component
            if (tempCamera.targetTexture != renderTexture)
                tempCamera.targetTexture = renderTexture;

            //If has changed the capture mode or not runned starting config
            if (captureMode != lastCaptureMode || layersToCaptureIn3d.value != lastCaptureModeLayersToCaptureValue || hasDefinedStartingConfigOfCaptureMode == false)
            {
                //If layermask is in nothing, select everything
                if (layersToCaptureIn3d == 0)
                    layersToCaptureIn3d = -1;
                //If UI layer is removed, add UI layer again
                if (IsThisLayerIdInLayerMask(5, layersToCaptureIn3d) == false)
                    layersToCaptureIn3d |= (1 << 5);

                //If capture mode is TopView2D
                if (captureMode == CaptureMode.TopView2D)
                {
                    tempCamera.cullingMask = (1 << LayerMask.NameToLayer("UI"));
                    tempCamera.farClipPlane = 15;
                }
                //If capture mode is TopView3D
                if (captureMode == CaptureMode.TopView3D)
                {
                    tempCamera.cullingMask = layersToCaptureIn3d;
                    tempCamera.farClipPlane = BASE_HEIGHT_IN_3D_WORLD * 3.0f;
                }

                //Set data on cache
                lastCaptureMode = captureMode;
                lastCaptureModeLayersToCaptureValue = layersToCaptureIn3d.value;
                hasDefinedStartingConfigOfCaptureMode = true;
            }
            //If field of view is minor than 1.0, reset
            if (fieldOfView < 1.0f)
                fieldOfView = 1.0f;

            //Move the camera to follow this gameobject
            tempCameraObj.transform.position = Vector3.Lerp(tempCameraObj.transform.position, new Vector3(this.gameObject.transform.position.x, BASE_HEIGHT_IN_3D_WORLD, this.gameObject.transform.position.z), movementsSmoothing * Time.deltaTime);
            //Rotate the camera
            if (followRotationOf == FollowRotationOf.ThisGameObject)
                tempCameraObj.transform.rotation = Quaternion.Lerp(tempCameraObj.transform.rotation, Quaternion.Euler(90, this.gameObject.transform.rotation.eulerAngles.y, 0), movementsSmoothing * Time.deltaTime);
            if (followRotationOf == FollowRotationOf.CustomGameObject && customGameObjectToFollowRotation != null)
                tempCameraObj.transform.rotation = Quaternion.Lerp(tempCameraObj.transform.rotation, Quaternion.Euler(90, customGameObjectToFollowRotation.rotation.eulerAngles.y, 0), movementsSmoothing * Time.deltaTime);
        }

        public void OnDestroy()
        {
            //Stop image processing, if needed
            StopImageProcessing();
        }

        //Public methods

        public Camera GetGeneratedCameraAtRunTime()
        {
            //Return the camera generated at runtime by this component
            return tempCamera;
        }

        public MinimapCamera[] GetListOfAllMinimapCamerasInThisScene()
        {
            //If is not playing, cancel
            if (Application.isPlaying == false)
            {
                Debug.LogError("It is only possible to obtain the list of Minimap Cameras in this scene, if the application is being executed.");
                return null;
            }

            //Return a list that contains reference to all of this component in this scene
            return minimapDataHolder.instancesOfMinimapCameraInThisScene.ToArray();
        }

        //Auxiliar methods

        private void InitializeTheImageProcessing()
        {
            //Stop image processing, if needed
            StopImageProcessing();

            //Explicitly use variables, to avoid errors of "variable never used"
            var obj1 = (object)birp_currentShadowsSetting;
            var obj2 = (object)birp_currentFogSetting;
            var obj3 = (object)birp_currentGlowSetting;
#if EMS_HDRP_On
            var obj4 = (object)hdrp_currentShadowSetting;
#endif

            //Add needed callbacks to handle camera
#if EMS_BIRP_On
            Camera.onPreRender += BIRP_OnCameraPreRender;
            Camera.onPostRender += BIRP_OnCameraPostRender;
#endif
#if EMS_URP_On
            RenderPipelineManager.beginCameraRendering += SRP_OnCameraPreRender;
            RenderPipelineManager.endCameraRendering += SRP_OnEndCameraPreRender;
#endif
#if EMS_HDRP_On
            RenderPipelineManager.beginCameraRendering += SRP_OnCameraPreRender;
            RenderPipelineManager.endCameraRendering += SRP_OnEndCameraPreRender;
#endif
        }

        private void BIRP_OnCameraPreRender(Camera currentCamera)
        {
            //Repass the callback to common method...
            Common_OnCameraPreRender(currentCamera);
        }

        private void BIRP_OnCameraPostRender(Camera currentCamera)
        {
            //Repass the callback to common method...
            Common_OnCameraPostRender(currentCamera);
        }

        private void SRP_OnCameraPreRender(ScriptableRenderContext context, Camera camera)
        {
            //Repass the callback to common method...
            Common_OnCameraPreRender(camera);
        }

        private void SRP_OnEndCameraPreRender(ScriptableRenderContext context, Camera camera)
        {
            //Repass the callback to common method...
            Common_OnCameraPostRender(camera);
        }

        private void Common_OnCameraPreRender(Camera targetCamera)
        {
            //If the target camera is not the generated temp camera, cancel
            if (targetCamera != tempCamera)
                return;

#if EMS_BIRP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes)
            {
                //Get current status of settings
                birp_currentShadowsSetting = QualitySettings.shadows;
                //Disable the shadow
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes)
            {
                //Get current status of settings
                birp_currentFogSetting = RenderSettings.fog;
                //Disable the fog
                RenderSettings.fog = false;
            }
            //If is desired to skip glow
            if (skipGlowInCapture == SkipGlow.Yes) { }
            //If is desired to skip color grading
            if (skipColorGradingInCapture == SkipColorGrading.Yes) { }
            //If is desired to skip tonemapping
            if (skipTonemappingInCapture == SkipTonemapping.Yes) { }
#endif
#if EMS_URP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes)
            {
                //Get current status of settings
                urp_currentShadowSetting = targetCamera.GetUniversalAdditionalCameraData().renderShadows;
                //Disable the shadow
                targetCamera.GetUniversalAdditionalCameraData().renderShadows = false;
            }
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes)
            {
                //Get current status of settings
                urp_currentFogSetting = RenderSettings.fog;
                //Disable the fog
                RenderSettings.fog = false;
            }
            //If is desired to skip glow
            if (skipGlowInCapture == SkipGlow.Yes && referenceVolume != null)
            {
                //Get current status of settings
                if (urp_currentGlow == null)
                    referenceVolume.profile.TryGet<Bloom>(out urp_currentGlow);
                if (urp_currentGlow != null)
                    urp_currentGlowSetting = urp_currentGlow.active;
                //Disable the glow
                if (urp_currentGlow != null)
                    urp_currentGlow.active = false;
            }
            //If is desired to skip color grading
            if (skipColorGradingInCapture == SkipColorGrading.Yes) { }
            //If is desired to skip tonemapping
            if (skipTonemappingInCapture == SkipTonemapping.Yes) { }
#endif
#if EMS_HDRP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes) { }
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes && referenceVolume != null)
            {
                //Get current status of settings
                if (hdrp_currentFog == null)
                    referenceVolume.profile.TryGet<Fog>(out hdrp_currentFog);
                if (hdrp_currentFog != null)
                    hdrp_currentFogSetting = hdrp_currentFog.active;
                //Disable the fog
                if (hdrp_currentFog != null)
                    hdrp_currentFog.active = false;
            }
            //If found a HD Additional Camera Data
            if (hdrp_additionalCameraData == null)
                hdrp_additionalCameraData = tempCamera.GetComponent<HDAdditionalCameraData>();
            if (hdrp_additionalCameraData == null)
                hdrp_additionalCameraData = tempCamera.gameObject.AddComponent<HDAdditionalCameraData>();
            if (hdrp_additionalCameraData != null)
            {
                //Enable the custom rendering frame
                hdrp_additionalCameraData.customRenderingSettings = true;

                //If is desired to skip glow
                if (skipGlowInCapture == SkipGlow.Yes)
                    if (hdrp_additionalCameraData.renderingPathCustomFrameSettings.IsEnabled(FrameSettingsField.Bloom) == true || hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] == false)
                    {
                        hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] = true;
                        hdrp_additionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Bloom, false);
                    }
                if (skipGlowInCapture == SkipGlow.No)
                    hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] = false;
                //If is desired to skip color grading
                if (skipColorGradingInCapture == SkipColorGrading.Yes)
                    if (hdrp_additionalCameraData.renderingPathCustomFrameSettings.IsEnabled(FrameSettingsField.ColorGrading) == true || hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ColorGrading] == false)
                    {
                        hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ColorGrading] = true;
                        hdrp_additionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ColorGrading, false);
                    }
                if (skipColorGradingInCapture == SkipColorGrading.No)
                    hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ColorGrading] = false;
                //If is desired to skip tonemapping
                if (skipTonemappingInCapture == SkipTonemapping.Yes)
                    if (hdrp_additionalCameraData.renderingPathCustomFrameSettings.IsEnabled(FrameSettingsField.Tonemapping) == true || hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Tonemapping] == false)
                    {
                        hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Tonemapping] = true;
                        hdrp_additionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Tonemapping, false);
                    }
                if (skipTonemappingInCapture == SkipTonemapping.No)
                    hdrp_additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Tonemapping] = false;
            }
#endif
        }

        private void Common_OnCameraPostRender(Camera targetCamera)
        {
            //If the target camera is not the generated temp camera, cancel
            if (targetCamera != tempCamera)
                return;

#if EMS_BIRP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes)
                QualitySettings.shadows = birp_currentShadowsSetting;
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes)
                RenderSettings.fog = birp_currentFogSetting;
            //If is desired to skip glow
            if (skipGlowInCapture == SkipGlow.Yes) { }
            //If is desired to skip color grading
            if (skipColorGradingInCapture == SkipColorGrading.Yes) { }
            //If is desired to skip tonemapping
            if (skipTonemappingInCapture == SkipTonemapping.Yes) { }
#endif
#if EMS_URP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes)
                targetCamera.GetUniversalAdditionalCameraData().renderShadows = urp_currentShadowSetting;
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes)
                RenderSettings.fog = urp_currentFogSetting;
            //If is desired to skip glow
            if (skipGlowInCapture == SkipGlow.Yes)
                if (urp_currentGlow != null)
                    urp_currentGlow.active = urp_currentGlowSetting;
            //If is desired to skip color grading
            if (skipColorGradingInCapture == SkipColorGrading.Yes) { }
            //If is desired to skip tonemapping
            if (skipTonemappingInCapture == SkipTonemapping.Yes) { }
#endif
#if EMS_HDRP_On
            //If is desired to skip shadows
            if (skipShadowInCapture == SkipShadow.Yes) { }
            //If is desired to skip fogs
            if (skipFogInCapture == SkipFog.Yes)
                if (hdrp_currentFog != null)
                    hdrp_currentFog.active = hdrp_currentFogSetting;
            //If is desired to skip glow
            if (skipGlowInCapture == SkipGlow.Yes) { }
            //If is desired to skip color grading
            if (skipColorGradingInCapture == SkipColorGrading.Yes) { }
            //If is desired to skip tonemapping
            if (skipTonemappingInCapture == SkipTonemapping.Yes) { }
#endif
        }

        private void StopImageProcessing()
        {
            //Remove any callbacks that could be registered
#if EMS_BIRP_On
            Camera.onPreRender -= BIRP_OnCameraPreRender;
            Camera.onPostRender -= BIRP_OnCameraPostRender;
#endif
#if EMS_URP_On
            RenderPipelineManager.beginCameraRendering -= SRP_OnCameraPreRender;
            RenderPipelineManager.endCameraRendering -= SRP_OnEndCameraPreRender;
#endif
#if EMS_HDRP_On
            RenderPipelineManager.beginCameraRendering -= SRP_OnCameraPreRender;
            RenderPipelineManager.endCameraRendering -= SRP_OnEndCameraPreRender;
#endif
        }
    }
}