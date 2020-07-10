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


                    var xNormalized = (float)row / tex.width;
                    var vNormalized = (float)column / tex.height;
                    var zNormalized = (float)depth / tex.depth;
                    var horizontalFalloffCurveValue = column >= tex.width / 2
                        ? config.xFalloffCurve.Evaluate(xNormalized)
                        : config.xFalloffCurve.Evaluate(1f - xNormalized);
                    var verticalFalloffCurveValue = row >= tex.height / 2
                        ? config.yFalloffCurve.Evaluate(vNormalized)
                        : config.yFalloffCurve.Evaluate(1f - vNormalized);
                    var depthFalloffCurveValue = depth >= tex.depth / 2
                        ? config.zFalloffCurve.Evaluate(zNormalized)
                        : config.zFalloffCurve.Evaluate(1f - zNormalized);
                    
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
