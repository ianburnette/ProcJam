using UnityEngine;

public class Cleanup : MonoBehaviour
{
    public void Despeckle(ref Texture2D tex, Color backgroundColor,
        LonePixelEvaluationMode configurationLonePixelEvaluationMode) {
        for (var column = 0; column < tex.width; column++) {
            for (var row = 0; row < tex.height; row++) {
                switch (configurationLonePixelEvaluationMode) {
                    case LonePixelEvaluationMode.CardinalDirectionsOnly:
                        if (BooleanPixel4WayContext.GetContext(new Vector2Int(column, row), tex, backgroundColor).IsSpeckle())
                            tex.SetPixel(column, row, backgroundColor);
                        break;
                    case LonePixelEvaluationMode.IncludeDiagonals:
                        if (BooleanPixel8WayContext.GetPixelContext(tex, column, row, backgroundColor).IsSpeckle())
                            tex.SetPixel(column, row, backgroundColor);
                        break;
                }
            }
        }
    }
}
