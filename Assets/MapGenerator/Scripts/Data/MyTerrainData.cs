using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class MyTerrainData : UpdatableData
{

    public float uniformScale = 2.5f;

    public bool useFlatShading;
    public bool useFalloff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
}