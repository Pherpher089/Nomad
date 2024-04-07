#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MalbersAnimations.SerializeReferenceExtensions.Editor
{

    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        struct TypePopupCache
        {
            public AdvancedTypePopup TypePopup { get; }
            public AdvancedDropdownState State { get; }
            public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state)
            {
                TypePopup = typePopup;
                State = state;
            }
        }

        const int k_MaxTypePopupLineCount = 13;
        static readonly Type k_UnityObjectType = typeof(UnityEngine.Object);
        static readonly GUIContent k_NullDisplayName = new(TypeMenuUtility.k_NullDisplayName);
        static readonly GUIContent k_IsNotManagedReferenceLabel = new("The property type is not manage reference.");

        readonly Dictionary<string, TypePopupCache> m_TypePopups = new();
        readonly Dictionary<string, GUIContent> m_TypeNameCaches = new();

        SerializedProperty m_TargetProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    //if (label.text.Contains("Element"))
                    //{ label.text = label.text.Replace("Element", property.name); } //????

                    // Draw the subclass selector popup.
                    Rect popupPosition = new(position);
                    popupPosition.width -= EditorGUIUtility.labelWidth;
                    popupPosition.x += EditorGUIUtility.labelWidth;
                    popupPosition.height = EditorGUIUtility.singleLineHeight;

                   // var indent = EditorGUI.indentLevel;
                  //  EditorGUI.indentLevel = 0;

                    var hasActive = property.FindPropertyRelative("Active");
                  //  var delay = property.FindPropertyRelative("delay");


                    

                    if (hasActive != null)
                    {
                        var pos = EditorGUI.PrefixLabel(position, new GUIContent(" "));
                        Rect buttonRect = new(pos)
                        {
                            width = 20f,
                            height = EditorGUIUtility.singleLineHeight,
                            x = pos.x - 20f,
                        };

                        //  hasActive.boolValue = GUI.Toggle(buttonRect, hasActive.boolValue,new GUIContent(""));
                        hasActive.boolValue = GUI.Toggle(buttonRect, hasActive.boolValue, GUIContent.none);
                    }
                     

                    if (EditorGUI.DropdownButton(popupPosition, GetTypeName(property), FocusType.Keyboard))
                    {
                        TypePopupCache popup = GetTypePopup(property);
                        m_TargetProperty = property;
                        popup.TypePopup.Show(popupPosition);
                    }

                  


                   


                    // Draw the managed reference property.
                    EditorGUI.PropertyField(position, property, label, true);

                     
                 //   EditorGUI.indentLevel = indent;
                }
                else
                {
                    EditorGUI.LabelField(position, label, k_IsNotManagedReferenceLabel);
                }
            }
            EditorGUI.EndProperty();
        }

        TypePopupCache GetTypePopup(SerializedProperty property)
        {
            // Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
            string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

            if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename, out TypePopupCache result))
            {
                var state = new AdvancedDropdownState();

                Type baseType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);
                var popup = new AdvancedTypePopup(
                    TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
                        (p.IsPublic || p.IsNestedPublic) &&
                        !p.IsAbstract &&
                        !p.IsGenericType &&
                        !k_UnityObjectType.IsAssignableFrom(p) &&
                        Attribute.IsDefined(p, typeof(SerializableAttribute))
                    ),
                    k_MaxTypePopupLineCount,
                    state
                );
                popup.OnItemSelected += item =>
                {
                    Type type = item.Type;
                    object obj = m_TargetProperty.SetManagedReference(type);
                    m_TargetProperty.isExpanded = (obj != null);
                    m_TargetProperty.serializedObject.ApplyModifiedProperties();
                    m_TargetProperty.serializedObject.Update();
                };

                result = new TypePopupCache(popup, state);
                m_TypePopups.Add(managedReferenceFieldTypename, result);
            }
            return result;
        }

        GUIContent GetTypeName(SerializedProperty property)
        {
            // Cache this string.
            string managedReferenceFullTypename = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedReferenceFullTypename))
            {
                return k_NullDisplayName;
            }
            if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out GUIContent cachedTypeName))
            {
                return cachedTypeName;
            }

            Type type = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
            string typeName = null;

            AddTypeMenuAttribute typeMenu = TypeMenuUtility.GetAttribute(type);
            if (typeMenu != null)
            {
                typeName = typeMenu.GetTypeNameWithoutPath();
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = ObjectNames.NicifyVariableName(typeName);
                }
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = ObjectNames.NicifyVariableName(type.Name);
            }

            GUIContent result = new(typeName);
            m_TypeNameCaches.Add(managedReferenceFullTypename, result);
            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

    }

    public static class TypeMenuUtility
    {
        public const string k_NullDisplayName = "[Null]";

        public static AddTypeMenuAttribute GetAttribute(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(AddTypeMenuAttribute)) as AddTypeMenuAttribute;
        }

        public static string[] GetSplittedTypePath(Type type)
        {
            AddTypeMenuAttribute typeMenu = GetAttribute(type);
            if (typeMenu != null)
            {
                return typeMenu.GetSplittedMenuName();
            }
            else
            {
                int splitIndex = type.FullName.LastIndexOf('.');
                if (splitIndex >= 0)
                {
                    return new string[] { type.FullName.Substring(0, splitIndex), type.FullName.Substring(splitIndex + 1) };
                }
                else
                {
                    return new string[] { type.Name };
                }
            }
        }

        public static IEnumerable<Type> OrderByType(this IEnumerable<Type> source)
        {
            return source.OrderBy(type =>
            {
                if (type == null)
                {
                    return -999;
                }
                return GetAttribute(type)?.Order ?? 0;
            }).ThenBy(type =>
            {
                if (type == null)
                {
                    return null;
                }
                return GetAttribute(type)?.MenuName ?? type.Name;
            });
        }

    }
}
#endif