using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    private SoundManager soundManager;
    private SerializedObject soSoundManager;
    private SerializedProperty m_MusicVolume;
    private SerializedProperty m_AmbientVolume;
    private SerializedProperty m_SfxVolume;

    // Add other serialized properties you want to edit here

    private void OnEnable()
    {
        soundManager = (SoundManager)target;
        soSoundManager = new SerializedObject(soundManager);
        m_MusicVolume = soSoundManager.FindProperty("m_MusicVolume");
        m_AmbientVolume = soSoundManager.FindProperty("m_AmbientVolume");
        m_SfxVolume = soSoundManager.FindProperty("m_SfxVolume");

        // Find other serialized properties you want to edit here
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(m_MusicVolume);
        EditorGUILayout.PropertyField(m_AmbientVolume);
        EditorGUILayout.PropertyField(m_SfxVolume);
        // Display other serialized properties you want to edit here

        if (EditorGUI.EndChangeCheck())
        {
            soSoundManager.ApplyModifiedProperties();
            soundManager.UpdateVolume();
        }
    }
}