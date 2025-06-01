using UnityEditor;
using UnityEngine;

public class LightmapUtility : MonoBehaviour
{
    [MenuItem("Tools/Smart Lightmap UV Setup")]
    static void SetupLightmapUVs()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogWarning("Please select a root GameObject in the hierarchy.");
            return;
        }

        int updatedCount = 0;

        foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            GameObject go = mr.gameObject;

            // Skip non-static or non-GI-contributing objects
            if ((GameObjectUtility.GetStaticEditorFlags(go) & StaticEditorFlags.ContributeGI) == 0)
                continue;

            // Skip tiny props (optional logic)
            if (mr.bounds.size.magnitude < 0.5f)
            {
                GameObjectUtility.SetStaticEditorFlags(go, GameObjectUtility.GetStaticEditorFlags(go) & ~StaticEditorFlags.ContributeGI);
                Debug.Log($"❌ Removed GI static flag from: {go.name}");
                continue;
            }

            // Generate Lightmap UVs if the mesh is imported
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
                if (!string.IsNullOrEmpty(path))
                {
                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer != null && !importer.generateSecondaryUV)
                    {
                        importer.generateSecondaryUV = true;
                        importer.SaveAndReimport();
                        updatedCount++;
                        Debug.Log($"✅ Enabled Lightmap UVs for: {path}");
                    }
                }
            }
        }

        Debug.Log($"✨ Smart Lightmap setup complete. {updatedCount} mesh assets updated.");
    }
}
