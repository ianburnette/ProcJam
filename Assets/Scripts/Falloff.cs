using UnityEngine;

public class Falloff : MonoBehaviour
{
    public void ApplyFalloff(ref Texture2D tex, FalloffConfig config) {
        for (var column = 0; column <= tex.width; column++) {
            for (var row = 0; row <= tex.height; row++) {
                var horEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.width, column));
                var vertEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.height, row));
                horEval = column >= tex.width / 2
                    ? config.falloffCurve.Evaluate(horEval)
                    : config.falloffCurve.Evaluate(1f - horEval);
                vertEval = row >= tex.height / 2
                    ? config.falloffCurve.Evaluate(vertEval)
                    : config.falloffCurve.Evaluate(1f - vertEval);
                if (tex.GetPixel(column, row).grayscale <= horEval) tex.SetPixel(column, row, Color.black);
                if (tex.GetPixel(column, row).grayscale <= vertEval) tex.SetPixel(column, row, Color.black);
            }
        }
    }
    
    public void ApplyFalloff(ref Texture3D tex, FalloffConfig config) {
        for (var depth = 0; depth < tex.depth; depth++) {
            for (var column = 0; column <= tex.width; column++) {
                for (var row = 0; row <= tex.height; row++) {
                    //var horEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.width, column));
                    //var vertEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.height, row));
                    //var depthEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.depth, depth));


                    var falloffCurveValue = config.falloffCurve.Evaluate((float)column / (float)tex.width);
                    var curveValue = config.falloffCurve.Evaluate(1f - (float)column / (float)tex.width);
                    var horizontalFalloffCurveValue = column >= tex.width / 2
                        ? falloffCurveValue
                        : curveValue;
                    var verticalFalloffCurveValue = row >= tex.height / 2
                        ? config.falloffCurve.Evaluate((float)row / tex.height)
                        : config.falloffCurve.Evaluate(1f - (float)row / tex.height);
                    var depthFalloffCurveValue = depth >= tex.depth / 2
                        ? config.falloffCurve.Evaluate((float)depth / tex.depth)
                        : config.falloffCurve.Evaluate(1f - (float)depth / tex.depth);
                    
                    if (tex.GetPixel(column, row, depth).grayscale <= horizontalFalloffCurveValue ||
                        tex.GetPixel(column, row, depth).grayscale <= verticalFalloffCurveValue ||
                        tex.GetPixel(column, row, depth).grayscale <= depthFalloffCurveValue) {
                        tex.SetPixel(column, row, depth, Color.black);
                    }
                }
            }
        }
    }
}    
