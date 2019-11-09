using System;
using UnityEngine;

public class Scaling : MonoBehaviour
{
    public void ScaleTexture(ref Texture2D tex, ScalingMode configurationScalingMode) {
        switch (configurationScalingMode) {
            case ScalingMode.none: break;
            case ScalingMode.x2:
                tex = Scale(tex, 2); break;
            case ScalingMode.x4:
                tex = Scale(tex, 4); break;
            case ScalingMode.eagle2:
                tex = Scale(tex, 2, true);
                break;
            case ScalingMode.eagle3:
                tex = Scale(tex, 3, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(configurationScalingMode), configurationScalingMode, null);
        }
    }

    Texture2D Scale(Texture2D tex, int scale, bool eagle = false) {
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
