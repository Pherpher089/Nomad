using UnityEditor;
using UnityEngine;

public class LightmapUtility : MonoBehaviour
{
    [MenuItem("Tools/Smart Lightmap Setup (UV + Static Flags)")]
    static void GenerateUVsFromSelected()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("Please select a root GameObject in the hierarchy.");
            return;
        }

        int uvUpdatedCount = 0;
        int staticFlagUpdated = 0;

        MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            GameObject go = mf.gameObject;

            // Skip tiny objects (optional)
            Bounds bounds = mf.sharedMesh.bounds;
            if (bounds.size.magnitude < 0.5f)
            {
                continue;
            }

            // Set Static Editor Flags
            StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(go);
            StaticEditorFlags desiredFlags = currentFlags | StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic;

            if (currentFlags != desiredFlags)
            {
                GameObjectUtility.SetStaticEditorFlags(go, desiredFlags);
                staticFlagUpdated++;
                Debug.Log($"📌 Set static flags on: {go.name}");
            }

            // Generate Lightmap UVs (only for imported models)
            string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (string.IsNullOrEmpty(path)) continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null && !importer.generateSecondaryUV)
            {
                importer.generateSecondaryUV = true;
                importer.SaveAndReimport();
                uvUpdatedCount++;
                Debug.Log($"✅ Enabled Lightmap UVs for: {path}");
            }
        }

        Debug.Log($"✅ Lightmap utility complete.\n🔸 {uvUpdatedCount} mesh assets updated with UVs.\n🔹 {staticFlagUpdated} objects updated with static flags.");
    }
}
