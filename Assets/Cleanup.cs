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

    public void RemovePixelsAtEdgeOfSprite(ref Texture2D tex, Color backgroundColor) {
        var colors = new Color[tex.width];
        for (int i = 0; i < tex.width; i++) colors[i] = backgroundColor;

        tex.SetPixels(0, 0, tex.width - 1, 1, colors);
        tex.SetPixels(0, 0, 1, tex.height - 1, colors);
        tex.SetPixels(tex.width - 1, 0, 1, tex.height, colors);
        tex.SetPixels(0, tex.height - 1, tex.width, 1, colors);
    }
}
