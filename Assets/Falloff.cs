using UnityEngine;

public class Falloff : MonoBehaviour
{
  //  [SerializeField] AnimationCurve falloffCurve;
    
    public void ApplyFalloff(ref Texture2D tex, Configuration config) {
        for (var i = 0; i <= tex.width; i++) {
            for (var j = 0; j <= tex.height; j++) {
                var horEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.width, i));
                var vertEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.height, j));
                horEval = i >= tex.width / 2 ? config.falloffCurve.Evaluate(horEval) : config.falloffCurve.Evaluate(1f - horEval);
                vertEval = j >= tex.height / 2 ? config.falloffCurve.Evaluate(vertEval) : config.falloffCurve.Evaluate(1f - vertEval);
                if (tex.GetPixel(i, j).grayscale <= horEval) tex.SetPixel(i, j, Color.black);
                if (tex.GetPixel(i, j).grayscale <= vertEval) tex.SetPixel(i, j, Color.black);
            }
        }
    }
}    
