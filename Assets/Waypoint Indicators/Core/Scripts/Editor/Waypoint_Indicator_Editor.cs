using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;


#region Custom Inspector
[ExecuteInEditMode] //Runs in real time
[CustomEditor(typeof(Waypoint_Indicator)), CanEditMultipleObjects]
public class Waypoint_Indicator_Editor : Editor
{
    //Reference MonoBehaviour class in this script
    Waypoint_Indicator targetScript;

    public override void OnInspectorGUI()
    {
        //Do this first to make sure you have the latest version
        serializedObject.Update();
        targetScript = (Waypoint_Indicator)target;

        

        EditorGUILayout.Separator();


        //GUILayout.BeginHorizontal("box");
        //GUILayout.EndHorizontal();
        //GUILayout.BeginVertical("box");
        //GUILayout.EndVertical();
        //EditorGUI.indentLevel++;



        #region Canvas & Camera Setup
        GUILayout.BeginVertical("box");
        EditorGUILayout.Separator();
        EditorGUI.indentLevel++;

        serializedObject.FindProperty("toggleSetupOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleSetupOptions, "Setup Options", true, EditorStyles.foldout);

        if (targetScript.toggleSetupOptions)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();


            //Declaring an ARRAY
            //SerializedProperty canvastagnames = serializedObject.FindProperty("canvas_tag_name_array");
            //EditorGUILayout.PropertyField(canvastagnames, new GUIContent() { text = "Canvas Tag Names" }, true);

            serializedObject.FindProperty("canvas_tag_name").stringValue = EditorGUILayout.TextField("Canvas Tag Name", targetScript.canvas_tag_name);

            serializedObject.FindProperty("camera_tag_name").stringValue = EditorGUILayout.TextField("Camera Tag Name", targetScript.camera_tag_name);


            serializedObject.FindProperty("multiCam").boolValue = EditorGUILayout.Toggle("Multiple Cameras", targetScript.multiCam);
            if (targetScript.multiCam)
            {
                EditorGUILayout.HelpBox("The Multiple Camera feature requires the WPI_Manager.cs script to be included in your project. See User Guide for details.", MessageType.Info);
            }
                


            serializedObject.FindProperty("distCalTargetTag").stringValue = EditorGUILayout.TextField("DCFT Tag Name", targetScript.distCalTargetTag);

            EditorGUILayout.HelpBox("DCFT (Distance Calculation From Target) is the game object that this game object will track its distance against. In most cases, this would be the same tag name as your main camera (above). In a 3rd person game, this might be the tag name of the player object instead.", MessageType.Info);

            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }
        else
        {

        }


        EditorGUI.indentLevel--;
        EditorGUILayout.Separator();
        GUILayout.EndVertical();
        #endregion





        #region Screen Edge Tracking
        //Screen Edge Tracking
        GUILayout.BeginVertical("box");
        serializedObject.FindProperty("enableStandardTracking").boolValue = EditorGUILayout.ToggleLeft(" Standard Tracking", targetScript.enableStandardTracking, EditorStyles.boldLabel);

        //Disaply these elements based off wether or not Standard Tracking is checked or not
        if (targetScript.enableStandardTracking)
        {
            EditorGUILayout.Separator();



            #region//PARENT OPTIONS
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleParentOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleParentOptions, "Parent Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleParentOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleParentOptions, parentOptionsTitle, EditorStyles.largeLabel);

            if (targetScript.toggleParentOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();
                serializedObject.FindProperty("showBoundaryBox").boolValue = EditorGUILayout.Toggle("Show Boundary", targetScript.showBoundaryBox);
                serializedObject.FindProperty("boundaryBoxColor").colorValue = EditorGUILayout.ColorField("Boundary Color", targetScript.boundaryBoxColor);
                serializedObject.FindProperty("parentSize").vector2IntValue = EditorGUILayout.Vector2IntField("Boundary Size", targetScript.parentSize);
                serializedObject.FindProperty("displayRangeMin").floatValue = EditorGUILayout.FloatField("Min Display Range", targetScript.displayRangeMin);
                serializedObject.FindProperty("displayRangeMax").floatValue = EditorGUILayout.FloatField("Max Display Range", targetScript.displayRangeMax);
                serializedObject.FindProperty("raycastTarget").boolValue = EditorGUILayout.Toggle("Raycast Target", targetScript.raycastTarget);
                EditorGUILayout.Separator();
                EditorGUI.indentLevel--;
            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region SPRITE OPTIONS
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleSpriteOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleSpriteOptions, "Sprite Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleSpriteOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleSpriteOptions, spriteOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleSpriteOptions)
            {
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                serializedObject.FindProperty("enableSprite").boolValue = EditorGUILayout.ToggleLeft("Enable Sprite", targetScript.enableSprite);
                
                //Disaply these elements based off Display drop down (onScreenIconType)
                if (targetScript.enableSprite)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    serializedObject.FindProperty("spriteDepth").intValue = EditorGUILayout.IntField("Depth", targetScript.spriteDepth);
                    serializedObject.FindProperty("offScreenSpriteRotates").boolValue = EditorGUILayout.Toggle("Off Screen Rotates", targetScript.offScreenSpriteRotates);
                    EditorGUILayout.Separator();

                    EditorGUILayout.Separator();
                    //ICON ON-SCREEN
                    EditorGUILayout.LabelField("Sprite On-Screen", EditorStyles.boldLabel);


                    serializedObject.FindProperty("onScreenSprite").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sprite"), targetScript.onScreenSprite, typeof(Sprite), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    serializedObject.FindProperty("onScreenSpriteColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.onScreenSpriteColor);
                    serializedObject.FindProperty("onScreenSpriteSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.onScreenSpriteSize);
                    serializedObject.FindProperty("onScreenSpriteOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.onScreenSpriteOffset);
                    serializedObject.FindProperty("onScreenSpriteRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.onScreenSpriteRotation, 0, 360);
                    serializedObject.FindProperty("onScreenSpriteFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.onScreenSpriteFadeWithRange);
                    serializedObject.FindProperty("onScreenSpriteScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.onScreenSpriteScaleWithRange);
                    serializedObject.FindProperty("reverseOnScreenSpriteScaleWithRange").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.reverseOnScreenSpriteScaleWithRange);
                    serializedObject.FindProperty("onScreenSpriteHide").boolValue = EditorGUILayout.Toggle("Hide", targetScript.onScreenSpriteHide);


                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();



                    //ICON OFF-SCREEN
                    EditorGUILayout.LabelField("Sprite Off-Screen", EditorStyles.boldLabel);


                    serializedObject.FindProperty("offScreenSprite").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sprite"), targetScript.offScreenSprite, typeof(Sprite), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    serializedObject.FindProperty("offScreenSpriteColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.offScreenSpriteColor);
                    serializedObject.FindProperty("offScreenSpriteSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.offScreenSpriteSize);
                    serializedObject.FindProperty("offScreenSpriteOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.offScreenSpriteOffset);
                    serializedObject.FindProperty("offScreenSpriteRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.offScreenSpriteRotation, 0, 360);
                    serializedObject.FindProperty("offScreenSpriteFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.offScreenSpriteFadeWithRange);
                    serializedObject.FindProperty("offScreenScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.offScreenScaleWithRange);
                    serializedObject.FindProperty("reverseOffScreenSpriteScaleWithRange").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.reverseOffScreenSpriteScaleWithRange);
                    serializedObject.FindProperty("offScreenSpriteHide").boolValue = EditorGUILayout.Toggle("Hide", targetScript.offScreenSpriteHide);


                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Check Enable Sprite for options.", MessageType.Info);
                }
                EditorGUI.indentLevel--;

            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region GAME OBJECT OPTIONS (Prefabs)
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleGameObjectOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleGameObjectOptions, "Prefab Indicator Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleGameObjectOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleGameObjectOptions, objectOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleGameObjectOptions)
            {
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                serializedObject.FindProperty("enableGameObject").boolValue = EditorGUILayout.ToggleLeft("Enable Object", targetScript.enableGameObject);
                //Disaply these elements based off Display drop down (onScreenIconType)
                if (targetScript.enableGameObject)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    serializedObject.FindProperty("gameObjectDepth").intValue = EditorGUILayout.IntField("Depth", targetScript.gameObjectDepth);
                    serializedObject.FindProperty("offScreenObjectRotates").boolValue = EditorGUILayout.Toggle("Off Screen Rotates", targetScript.offScreenObjectRotates);

                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();

                    //OBJECT ON-SCREEN
                    EditorGUILayout.LabelField("Prefab On-Screen", EditorStyles.boldLabel);


                    serializedObject.FindProperty("onScreenGameObject").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Prefab"), targetScript.onScreenGameObject, typeof(GameObject), true);
                    serializedObject.FindProperty("onScreenGameObjectColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.onScreenGameObjectColor);
                    serializedObject.FindProperty("onScreenGameObjectSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.onScreenGameObjectSize);
                    serializedObject.FindProperty("onScreenGameObjectOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.onScreenGameObjectOffset);
                    serializedObject.FindProperty("onScreenGameObjectRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.onScreenGameObjectRotation, 0, 360);
                    serializedObject.FindProperty("onScreenGameObjectFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.onScreenGameObjectFadeWithRange);
                    serializedObject.FindProperty("onScreenGameObjectScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.onScreenGameObjectScaleWithRange);
                    serializedObject.FindProperty("reverseOnScreenGameObjectScaleWithRange").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.reverseOnScreenGameObjectScaleWithRange);
                    serializedObject.FindProperty("onScreenGameObjectHide").boolValue = EditorGUILayout.Toggle("Hide", targetScript.onScreenGameObjectHide);


                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();


                    //OBJECT OFF-SCREEN
                    EditorGUILayout.LabelField("Prefab Off-Screen", EditorStyles.boldLabel);


                    serializedObject.FindProperty("offScreenGameObject").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Prefab"), targetScript.offScreenGameObject, typeof(GameObject), true);
                    serializedObject.FindProperty("offScreenGameObjectColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.offScreenGameObjectColor);
                    serializedObject.FindProperty("offScreenGameObjectSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.offScreenGameObjectSize);
                    serializedObject.FindProperty("offScreenGameObjectOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.offScreenGameObjectOffset);
                    serializedObject.FindProperty("offScreenGameObjectRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.offScreenGameObjectRotation, 0, 360);
                    serializedObject.FindProperty("offScreenGameObjectFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.offScreenGameObjectFadeWithRange);
                    serializedObject.FindProperty("offScreenGameObjectScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.offScreenGameObjectScaleWithRange);
                    serializedObject.FindProperty("reverseOffScreenGameObjectScaleWithRange").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.reverseOffScreenGameObjectScaleWithRange);
                    serializedObject.FindProperty("offScreenGameObjectHide").boolValue = EditorGUILayout.Toggle("Hide", targetScript.offScreenGameObjectHide);


                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Check Enable Object for options.", MessageType.Info);
                }
                EditorGUI.indentLevel--;

            }
            else
            {

            }

            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region Text Options
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleTextOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleTextOptions, "Text Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleTextOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleTextOptions, textOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleTextOptions)
            {
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                serializedObject.FindProperty("enableText").boolValue = EditorGUILayout.ToggleLeft("Enable Text", targetScript.enableText);


                //Disaply these elements based off Display drop down (onScreenIconType)
                if (targetScript.enableText)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();

                    serializedObject.FindProperty("textDepth").intValue = EditorGUILayout.IntField("Depth", targetScript.textDepth);
                    serializedObject.FindProperty("textDescription").stringValue = EditorGUILayout.TextField("Description", targetScript.textDescription);
                    serializedObject.FindProperty("distIncrement").stringValue = EditorGUILayout.TextField("Dist Increment", targetScript.distIncrement);
                    serializedObject.FindProperty("textFont").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Font Face"), targetScript.textFont, typeof(TMP_FontAsset), true);
                    serializedObject.FindProperty("textSize").floatValue = EditorGUILayout.FloatField("Font Size", targetScript.textSize);
                    serializedObject.FindProperty("textColor").colorValue = EditorGUILayout.ColorField("Font Color", targetScript.textColor);

                    serializedObject.FindProperty("textAlign").enumValueIndex = (int)(Waypoint_Indicator.textAlignValue)EditorGUILayout.EnumPopup("Text Align", targetScript.textAlign);
                    serializedObject.FindProperty("textLineSpacing").floatValue = EditorGUILayout.FloatField("Line Spacing", targetScript.textLineSpacing);
                    serializedObject.FindProperty("edgeDetectOffset").vector2Value = EditorGUILayout.Vector2Field("Edge Detect Offset", targetScript.edgeDetectOffset);
                    //edgeDetectOffset


                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Text On-Screen", EditorStyles.boldLabel);
                    serializedObject.FindProperty("onScreenSpriteHideDesc").boolValue = EditorGUILayout.Toggle("Hide Desc", targetScript.onScreenSpriteHideDesc);
                    serializedObject.FindProperty("onScreenSpriteHideDist").boolValue = EditorGUILayout.Toggle("Hide Dist", targetScript.onScreenSpriteHideDist);
                    serializedObject.FindProperty("onScreenTextOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.onScreenTextOffset);

                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Text Off-Screen", EditorStyles.boldLabel);
                    serializedObject.FindProperty("offScreenSpriteHideDesc").boolValue = EditorGUILayout.Toggle("Hide Desc", targetScript.offScreenSpriteHideDesc);
                    serializedObject.FindProperty("offScreenSpriteHideDist").boolValue = EditorGUILayout.Toggle("Hide Dist", targetScript.offScreenSpriteHideDist);
                    serializedObject.FindProperty("offScreenTextOffset").vector2Value = EditorGUILayout.Vector2Field("Position", targetScript.offScreenTextOffset);
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Check Enable Text for options.", MessageType.Info);
                }
                EditorGUI.indentLevel--;

            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            //Footer Spacers
            EditorGUILayout.Separator();
            
        }
        else
        {
            EditorGUILayout.HelpBox("Tracks objects in camera view to the edge of the screen.", MessageType.Info);
        }


        //EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.EndVertical();
        #endregion







        #region Screen Centered Tracking
        
        //SCREEN CENTERED TRACKING -----------------------------------------------------------------------
        EditorGUILayout.Separator();
        GUILayout.BeginVertical("box");
        serializedObject.FindProperty("enableCenteredTracking").boolValue = EditorGUILayout.ToggleLeft(" Centered Tracking", targetScript.enableCenteredTracking, EditorStyles.boldLabel);

        //Disaply these elements based off wether or not Centered Tracking is checked or not
        if (targetScript.enableCenteredTracking)
        {
            EditorGUILayout.Separator();



            #region CENTERED PARENT OPTIONS
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleDiameterOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleDiameterOptions, "Parent Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleDiameterOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleDiameterOptions, diameterOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleDiameterOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();
                serializedObject.FindProperty("showDiameter").boolValue = EditorGUILayout.Toggle("Show Diameter", targetScript.showDiameter);
                serializedObject.FindProperty("diameterColor").colorValue = EditorGUILayout.ColorField("Diameter Color", targetScript.diameterColor);
                serializedObject.FindProperty("diameterSize").floatValue = EditorGUILayout.FloatField("Diameter Size", targetScript.diameterSize);
                serializedObject.FindProperty("onScreenCenteredRangeMin").floatValue = EditorGUILayout.FloatField("Min Display Range", targetScript.onScreenCenteredRangeMin);
                serializedObject.FindProperty("onScreenCenteredRangeMax").floatValue = EditorGUILayout.FloatField("Max Display Range", targetScript.onScreenCenteredRangeMax);
                serializedObject.FindProperty("raycastTargetCentered").boolValue = EditorGUILayout.Toggle("Raycast Target", targetScript.raycastTargetCentered);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUI.indentLevel--;
            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region CENTERED SPRITE
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleCenteredSpriteOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleCenteredSpriteOptions, "Sprite Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleCenteredSpriteOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleCenteredSpriteOptions, spriteCenteredOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleCenteredSpriteOptions)
            {
                EditorGUILayout.Separator();
                EditorGUI.indentLevel++;
                serializedObject.FindProperty("enableCenteredSprite").boolValue = EditorGUILayout.ToggleLeft("Enable Sprite", targetScript.enableCenteredSprite);

                //Disaply these elements based off Display drop down (onScreenIconType)
                if (targetScript.enableCenteredSprite)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    serializedObject.FindProperty("onScreenCenteredSpriteDepth").intValue = EditorGUILayout.IntField("Depth", targetScript.onScreenCenteredSpriteDepth);
                    serializedObject.FindProperty("onScreenCenteredSprite").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sprite"), targetScript.onScreenCenteredSprite, typeof(Sprite), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    serializedObject.FindProperty("onScreenCenteredSpriteColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.onScreenCenteredSpriteColor);
                    serializedObject.FindProperty("onScreenCenteredSpriteSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.onScreenCenteredSpriteSize);
                    serializedObject.FindProperty("onScreenCenteredSpriteRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.onScreenCenteredSpriteRotation, 0, 360);
                    serializedObject.FindProperty("onScreenSpriteCenteredFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.onScreenSpriteCenteredFadeWithRange);
                    serializedObject.FindProperty("onScreenSpriteCenteredScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.onScreenSpriteCenteredScaleWithRange);
                    serializedObject.FindProperty("onScreenSpriteCenteredScaleReverse").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.onScreenSpriteCenteredScaleReverse);
                    serializedObject.FindProperty("hideOnScreenCenteredSprite").boolValue = EditorGUILayout.Toggle("Hide On-Screen", targetScript.hideOnScreenCenteredSprite);
                    serializedObject.FindProperty("hideOffScreenCenteredSprite").boolValue = EditorGUILayout.Toggle("Hide Off-Screen", targetScript.hideOffScreenCenteredSprite);
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Check Enable Sprite for options.", MessageType.Info);
                }
                EditorGUI.indentLevel--;

            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region CENTERED GAME OBJECT (Prefab)
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleCenteredPrefabOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleCenteredPrefabOptions, "Prefab Indicator Options", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleCenteredSpriteOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleCenteredSpriteOptions, spriteCenteredOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleCenteredPrefabOptions)
            {
                EditorGUILayout.Separator();
                EditorGUI.indentLevel++;
                serializedObject.FindProperty("enableCenteredPrefab").boolValue = EditorGUILayout.ToggleLeft("Enable Prefab", targetScript.enableCenteredPrefab);

                //Disaply these elements based off Display drop down (onScreenIconType)
                if (targetScript.enableCenteredPrefab)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    serializedObject.FindProperty("onScreenCenteredPrefabDepth").intValue = EditorGUILayout.IntField("Depth", targetScript.onScreenCenteredPrefabDepth);
                    serializedObject.FindProperty("onScreenCenteredPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Prefab"), targetScript.onScreenCenteredPrefab, typeof(GameObject), true);
                    serializedObject.FindProperty("onScreenCenteredPrefabColor").colorValue = EditorGUILayout.ColorField("Color", targetScript.onScreenCenteredPrefabColor);
                    serializedObject.FindProperty("onScreenCenteredPrefabSize").floatValue = EditorGUILayout.FloatField("Size", targetScript.onScreenCenteredPrefabSize);
                    serializedObject.FindProperty("onScreenCenteredPrefabRotation").floatValue = EditorGUILayout.Slider("Rotation", targetScript.onScreenCenteredPrefabRotation, 0, 360);
                    serializedObject.FindProperty("onScreenPrefabCenteredFadeWithRange").boolValue = EditorGUILayout.Toggle("Fade with Range", targetScript.onScreenPrefabCenteredFadeWithRange);
                    serializedObject.FindProperty("onScreenPrefabCenteredScaleWithRange").boolValue = EditorGUILayout.Toggle("Scale with Range", targetScript.onScreenPrefabCenteredScaleWithRange);
                    serializedObject.FindProperty("onScreenPrefabCenteredScaleReverse").boolValue = EditorGUILayout.Toggle("Reverse Scale", targetScript.onScreenPrefabCenteredScaleReverse);
                    serializedObject.FindProperty("hideOnScreenCenteredPrefab").boolValue = EditorGUILayout.Toggle("Hide On-Screen", targetScript.hideOnScreenCenteredPrefab);
                    serializedObject.FindProperty("hideOffScreenCenteredPrefab").boolValue = EditorGUILayout.Toggle("Hide Off-Screen", targetScript.hideOffScreenCenteredPrefab);
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Check Enable Prefab for options.", MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }
            else
            {

            }
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion



            EditorGUILayout.Separator();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            #region RADIUS GIZMO OPTIONS
            EditorGUI.indentLevel++;
            serializedObject.FindProperty("toggleRadiusGizmoOptions").boolValue = EditorGUILayout.Foldout(targetScript.toggleRadiusGizmoOptions, "2D Circle Gizmo", true, EditorStyles.foldout);
            //serializedObject.FindProperty("toggleRadiusGizmoOptions").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(targetScript.toggleRadiusGizmoOptions, circleGizmoOptionsTitle, EditorStyles.largeLabel);
            if (targetScript.toggleRadiusGizmoOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Use this tool to align pointers that rotate around the exterior of a circular path. Scene View only. Must have Gizmos and 2D enabled.", MessageType.Info);
                EditorGUILayout.Separator();
                serializedObject.FindProperty("enableRadiusGizmo").boolValue = EditorGUILayout.Toggle("Show Gizmo", targetScript.enableRadiusGizmo);
                serializedObject.FindProperty("radiusGizmoColor").colorValue = EditorGUILayout.ColorField("Gizmo Color", targetScript.radiusGizmoColor);
                serializedObject.FindProperty("radiusGizmoSize").floatValue = EditorGUILayout.FloatField("Gizmo Size", targetScript.radiusGizmoSize);
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUI.indentLevel--;
            }
            else
            {

            }
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            //EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;

            #endregion

        }
        else
        {
            EditorGUILayout.HelpBox("Tracks objects along a circular perimeter originating from the center of the screen.", MessageType.Info);
        }

        GUILayout.EndVertical();
        
        #endregion
        


        //Footer Spacers
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        //do this last!  it will loop over the properties on your object and apply any it needs to, no if necessary!
        serializedObject.ApplyModifiedProperties();
    }
}
#endregion
