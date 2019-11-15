using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ScalingMode;

public class Scaling : MonoBehaviour
{
    public static void ScaleTexture(ref Texture2D tex, ScalingMode[] configurationScalingModes) {
        if (configurationScalingModes.Length == 0)
            return;
        foreach (var scalingMode in configurationScalingModes) {
            tex = Scale(tex, ScalingFactorSingle(scalingMode), scalingMode == eagle2 || scalingMode == eagle3);
        }
    }

    public static int ScalingFactorMultiple(ScalingMode[] scalingModes) {
        if (scalingModes.Length == 0)
            return 1;
        var factors = new List<int>();
        foreach (var scalingMode in scalingModes) 
            factors.Add(ScalingFactorSingle(scalingMode));
        var multiple = factors[0];
        for (var index = 1; index < factors.Count; index++) multiple *= factors[index];
        return multiple;
    }

    static int ScalingFactorSingle(ScalingMode scalingMode) {
        switch (scalingMode) {
            case x2:
                return 2;
            case x4:
                return 4;
            case x10:
                return 10;
            case eagle2:
                return 2;
            case eagle3:
                return 3;
        }

        return 1;
    }

    static Texture2D Scale(Texture2D tex, int scale, bool eagle = false) {
        var newTex = new Texture2D(tex.width * scale, tex.height * scale);
        for (var column = 0; column < newTex.width; column++) {
            for (var row = 0; row < newTex.height; row++) {
                if (eagle)
                    Eagle.SetPixelSampled(tex, ref newTex, column, row, scale);
                else
                    SetPixelSampled(tex, ref newTex, column, row, scale);
            }
        }
        return newTex;
    }

    static void SetPixelSampled(Texture2D tex, ref Texture2D newTex, int column, int row, float sampleScale) =>
        newTex.SetPixel(column, row, tex.GetPixelDownsampled(column, row, sampleScale));

}

public static class Texture2DExtensions {
    public static Color GetPixelDownsampled(this Texture2D texture2D, int x, int y, float sampleScale) =>
        texture2D.GetPixel(x.Downsample(sampleScale), y.Downsample(sampleScale));
}

public static class IntExtensions {
    public static int Downsample(this int toBeDownsampled, float sampleScale) =>
        Mathf.FloorToInt(toBeDownsampled / sampleScale);
    
}
