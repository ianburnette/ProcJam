using System;
using UnityEngine;
using static ScalingMode;

public class Scaling : MonoBehaviour
{
    public static void ScaleTexture(ref Texture2D tex, ScalingMode configurationScalingMode) {
        tex = Scale(tex, ScalingFactor(configurationScalingMode), configurationScalingMode == eagle2 || configurationScalingMode == eagle3); 
    }

    public static int ScalingFactor(ScalingMode scalingMode) {
        switch (scalingMode) {
            case x2:
                return 2;
            case x4:
                return 4;
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
