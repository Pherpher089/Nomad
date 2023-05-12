using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    public static HeightMap GenerateHeightMap(int width, int height, BiomeData biomeData, Vector2 sampleCentre)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, biomeData.heightMapSettings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(biomeData.heightMapSettings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * biomeData.heightMapSettings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }
}




public struct HeightMap
{
    public float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
