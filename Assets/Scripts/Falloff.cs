using UnityEngine;

public class Falloff : MonoBehaviour
{
    public void ApplyFalloff(ref Texture2D tex, FalloffConfig config) {
        for (var column = 0; column <= tex.width; column++) {
            for (var row = 0; row <= tex.height; row++) {
                var horEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.width, column));
                var vertEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.height, row));
                horEval = column >= tex.width / 2 ? config.falloffCurve.Evaluate(horEval) : config.falloffCurve.Evaluate(1f - horEval);
                vertEval = row >= tex.height / 2 ? config.falloffCurve.Evaluate(vertEval) : config.falloffCurve.Evaluate(1f - vertEval);
                if (tex.GetPixel(column, row).grayscale <= horEval) tex.SetPixel(column, row, Color.black);
                if (tex.GetPixel(column, row).grayscale <= vertEval) tex.SetPixel(column, row, Color.black);
            }
        }
    }
}    
