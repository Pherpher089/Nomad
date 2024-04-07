using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    public abstract class IDs : ScriptableObject 
    {
        [Tooltip("Display name on the ID Selection Context Button")]
        public string DisplayName;

        [Tooltip("Integer value to Identify IDs")]
        public int ID;

        public static implicit operator int(IDs reference) => reference != null ? reference.ID : 0; //  =>  reference.ID;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = name;
        }

        protected void FindID<T>() where T : IDs
        {
            int newID = 0;
            var allAdd = MTools.GetAllInstances<T>();

            bool Found = true;

            while (Found)
            {
                newID++;
                Found = allAdd.Exists(x => (x.ID == newID && x != this));
            }
            ID = newID;
            DisplayName = name;
            MTools.SetDirty(this);
        }

        /// <summary> Returns if an ID is inside a list (Include or Exclude) </summary>
        /// <param name="list">the list to verify </param>
        /// <param name="include">true: check if is included. false check if is excluded</param>
        /// <returns></returns>
        public bool Included<T>(List<T> list, bool include) where T : IDs
        {
            bool isIncluded = list.Contains(this);
            return include ? isIncluded : !isIncluded;
        }

        public bool Included(List<IDs> list) => Included(list, true);
        public bool Excluded(List<IDs> list) => Included(list, false);


#if UNITY_EDITOR 
        [ContextMenu("Get ID <Hash>")]
        private void GetIDHash()
        {
            ID = Animator.StringToHash(name);
            MTools.SetDirty(this);
        } 
#endif
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IDs), true)]
    public class IDDrawer : PropertyDrawer
    {
        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        List<IDs> Instances;
        List<string> popupOptions;

       // private readonly Color Require= new Color(1, 0.4f, 0.4f, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }
             
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue)
            {
                label.tooltip += $"\n ID Value: [{(property.objectReferenceValue as IDs).ID}]";
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
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            //position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0; 


            if (Instances == null || Instances.Count == 0)
            {
                var NameOfType = GetPropertyType(property);
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
                        displayname = idString +  displayname;
                    }

                    popupOptions.Add(displayname);
                }
            }
            var PropertyValue = property.objectReferenceValue;

            //  Debug.Log(PropertyValue);
            int result = 0;

            if (PropertyValue != null && Instances.Count > 0)
            {
                result = Instances.FindIndex(i => i.name == PropertyValue.name) + 1; //Plus 1 because 0 is None
            }
             

            result = EditorGUI.Popup(buttonRect, result, popupOptions.ToArray(), popupStyle);

            if (result == 0) 
            {
                property.objectReferenceValue = null;
            }
            else
            {
                var NewSelection = Instances[result - 1];
                property.objectReferenceValue = NewSelection;
             
            }

            position.height = EditorGUIUtility.singleLineHeight;


            //if (property.objectReferenceValue != null)
            //{
            //    var id = (property.objectReferenceValue as IDs).ID;

            //    position.width-=50;
            //    var IntRect = new Rect(position);
            //    IntRect.x = position.width+80;
            //    IntRect.width = 50;

            //    using (new EditorGUI.DisabledGroupScope(true))
            //        EditorGUI.IntField(IntRect, GUIContent.none, id);
            //}

            EditorGUI.PropertyField(position, property, GUIContent.none, false);


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