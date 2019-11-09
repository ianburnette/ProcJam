using UnityEngine;

public class Falloff : MonoBehaviour
{
    [SerializeField] AnimationCurve falloffCurve;
    
    public void ApplyFalloff(ref Texture2D tex) {
        var half = tex.width / 2f;
      //  for (var i = 0; i < tex.width / 2; i++) {
      //      for (var j = 0; j < tex.height; j++) {
      //          var horizontalLikelihood = falloffCurve.Evaluate(1 - (float)i / tex.width * 2f);
      //          if (tex.GetPixel(i, j).grayscale < horizontalLikelihood) tex.SetPixel(i, j, Color.red);
      //      }
      //  }

        for (var i = 0; i <= tex.width; i++) {
            for (var j = 0; j <= tex.height; j++) {
                var horEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.width, i));
                var vertEval = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, tex.height, j));
                horEval = i >= tex.width / 2 ? falloffCurve.Evaluate(horEval) : falloffCurve.Evaluate(1f - horEval);
                vertEval = j >= tex.height / 2 ? falloffCurve.Evaluate(vertEval) : falloffCurve.Evaluate(1f - vertEval);
                if (tex.GetPixel(i, j).grayscale <= horEval) tex.SetPixel(i, j, Color.black);
                if (tex.GetPixel(i, j).grayscale <= vertEval) tex.SetPixel(i, j, Color.black);
            }
        }
    }
}
