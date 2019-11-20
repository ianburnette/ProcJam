using System.Collections.Generic;
using UnityEngine;

public static class Shading {
    public static void Shade(ref Texture2D tex, Color backgroundColor, float shadingIntensity, bool shadingByColor) {
        List<(Vector2Int, Color)> toDarken = new List<(Vector2Int, Color)>();
        for (var column = 0; column < tex.width; column++) {
            for (var row = 0; row < tex.height; row++) {
                var thisPixel = tex.GetPixel(column, row);
                if(thisPixel==backgroundColor) continue;
                var pixelContext = ColorPixel9WayContext.GetPixelContext(tex, column, row);
                if (shadingByColor && 
                    (pixelContext.down != thisPixel ||
                    pixelContext.downLeft != thisPixel || 
                    pixelContext.left != thisPixel)) {
                    toDarken.Add((new Vector2Int(column, row), thisPixel));
                }
                else if (!shadingByColor &&
                         (pixelContext.down == backgroundColor ||
                         pixelContext.downLeft == backgroundColor || 
                         pixelContext.left == backgroundColor)) {
                    tex.SetPixel(column, row, thisPixel - Color.white * shadingIntensity);
                }
            }
        }

        if (shadingByColor) {
            foreach (var pixel in toDarken)
                tex.SetPixel(pixel.Item1.x, pixel.Item1.y, pixel.Item2 - Color.white * shadingIntensity);
        }
    }

    public static void Highlight(ref Texture2D tex, Color backgroundColor, float highlightIntensity, bool highlightByColor) {
        List<(Vector2Int, Color)> toHighlight = new List<(Vector2Int, Color)>();

        for (var column = 0; column < tex.width; column++) {
            for (var row = 0; row < tex.height; row++) {
                var thisPixel = tex.GetPixel(column, row);
                if (thisPixel == backgroundColor) continue;
                var pixelContext = ColorPixel9WayContext.GetPixelContext(tex, column, row);
                if (highlightByColor) {
                    if (pixelContext.upLeft == thisPixel &&
                        pixelContext.up == thisPixel &&
                        pixelContext.upRight == thisPixel &&
                        pixelContext.left == thisPixel &&
                        pixelContext.right == thisPixel &&
                        pixelContext.downLeft == thisPixel &&
                        pixelContext.down == thisPixel &&
                        pixelContext.downRight == thisPixel) {
                        toHighlight.Add((new Vector2Int(column, row), thisPixel));
                    }
                }
                else {
                    if (pixelContext.upLeft != backgroundColor &&
                        pixelContext.up != backgroundColor &&
                        pixelContext.upRight != backgroundColor &&
                        pixelContext.left != backgroundColor &&
                        pixelContext.right != backgroundColor &&
                        pixelContext.downLeft != backgroundColor &&
                        pixelContext.down != backgroundColor &&
                        pixelContext.downRight != backgroundColor) {
                        tex.SetPixel(column, row, thisPixel + Color.white * highlightIntensity);
                    }
                }
            }
        }
        foreach (var pixel in toHighlight)
            tex.SetPixel(pixel.Item1.x, pixel.Item1.y, pixel.Item2 + Color.white * highlightIntensity);
    }
}