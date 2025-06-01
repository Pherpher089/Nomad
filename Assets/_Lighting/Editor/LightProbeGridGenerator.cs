using UnityEditor;
using UnityEngine;

public class LightProbeGridGenerator
{
    [MenuItem("Tools/Generate Light Probe Grid")]
    static void GenerateProbeGrid()
    {
        GameObject groupGO = new GameObject("Auto_LightProbeGroup");
        LightProbeGroup lpg = groupGO.AddComponent<LightProbeGroup>();

        Vector3 min = new Vector3(-250, 0, -250); // adjust to your level bounds
        Vector3 max = new Vector3(250, 50, 250);
        float spacing = 10f;

        var probes = new System.Collections.Generic.List<Vector3>();
        for (float x = min.x; x <= max.x; x += spacing)
        {
            for (float y = min.y; y <= max.y; y += spacing)
            {
                for (float z = min.z; z <= max.z; z += spacing)
                {
                    probes.Add(new Vector3(x, y, z));
                }
            }
        }

        lpg.probePositions = probes.ToArray();
        Debug.Log($"✅ Placed {probes.Count} light probes.");
    }
}
