using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    [System.Serializable]
    public class IDEnable<T> where T : IDs
    {
        public T ID;
        public bool enable = true;
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IDEnable<>),true)]
    public class IDEnableDrawer : PropertyDrawer
    {
        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        List<IDs> Instances;
        List<string> popupOptions;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
            {
                popupStyle = new(GUI.skin.GetStyle("PaneOptions"))
                {
                    imagePosition = ImagePosition.ImageOnly
                };
            }

            var positionRect = new Rect(position);

            var ID = property.FindPropertyRelative("ID");
            var enable = property.FindPropertyRelative("enable");

            label = EditorGUI.BeginProperty(position, label, property);

            if (ID.objectReferenceValue)
            {
                label.tooltip += $"\n ID Value: [{(ID.objectReferenceValue as IDs).ID}]";
                //  if (label.text.Contains("Element")) label.text = property.objectReferenceValue.name;
            }

            if (label.text.Contains("Element"))
            {
                position.x += 12;
                position.width -= 12;
            }
            else
                position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            float height = EditorGUIUtility.singleLineHeight;


            // Calculate rect for configuration button
            Rect buttonRect = new(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            //position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            // EditorGUI.indentLevel = 0;


            if (Instances == null || Instances.Count == 0)
            {
                var NameOfType = GetPropertyType(ID);
                string[] guids = AssetDatabase.FindAssets("t:" + NameOfType);  //FindAssets uses tags check documentation for more info

                Instances = new List<IDs>();
                popupOptions = new List<string>();
                popupOptions.Add("None");

                for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var inst = AssetDatabase.LoadAssetAtPath<IDs>(path);
                    Instances.Add(inst);
                }

                Instances = Instances.OrderBy(x => x.ID).ToList(); //Order by ID

                for (int i = 0; i < Instances.Count; i++)         //probably could get optimized 
                {
                    var inst = Instances[i];
                    var displayname = inst.name;
                    var idString = "[" + Instances[i].ID.ToString() + "] ";

                    if (Instances[i] is Tag) idString = ""; //Do not show On tag 

                    if (!string.IsNullOrEmpty(inst.DisplayName))
                    {
                        displayname = inst.DisplayName;
                        int pos = displayname.LastIndexOf("/") + 1;
                        displayname = displayname.Insert(pos, idString);
                    }
                    else
                    {
                        displayname = idString + displayname;
                    }

                    popupOptions.Add(displayname);
                }
            }
            var PropertyValue = ID.objectReferenceValue;

            //  Debug.Log(PropertyValue);
            int result = 0;

            if (PropertyValue != null && Instances.Count > 0)
            {
                result = Instances.FindIndex(i => i.name == PropertyValue.name) + 1; //Plus 1 because 0 is None
            }


            result = EditorGUI.Popup(buttonRect, result, popupOptions.ToArray(), popupStyle);

            if (result == 0)
            {
                ID.objectReferenceValue = null;
            }
            else
            {
                var NewSelection = Instances[result - 1];
                ID.objectReferenceValue = NewSelection;
            }

            position.height = EditorGUIUtility.singleLineHeight;
            var IDRect = new Rect(position);
            IDRect.width -= 25;


            var toogleRect = new Rect(positionRect);
            toogleRect.x = IDRect.x + IDRect.width + 5;
            toogleRect.width = 20;

            EditorGUI.PropertyField(IDRect, ID, GUIContent.none, false);
            EditorGUI.PropertyField(toogleRect, enable, GUIContent.none, false);


            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public static string GetPropertyType(SerializedProperty property)
        {
            var type = property.type;
            var match = System.Text.RegularExpressions.Regex.Match(type, @"PPtr<\$(.*?)>");
            if (match.Success)
                type = match.Groups[1].Value;
            return type;
        }
    }
#endif
}
