using UnityEngine;

public static class Shading {
    public static void Shade(ref Texture2D tex, Color backgroundColor, float shadingIntensity) {
        for (var column = 0; column < tex.width; column++) {
            for (var row = 0; row < tex.height; row++) {
                var thisPixel = tex.GetPixel(column, row);
                if(thisPixel==backgroundColor) continue;
                var pixelContext = ColorPixel9WayContext.GetPixelContext(tex, column, row);
                if (pixelContext.down == backgroundColor ||
                    pixelContext.downLeft == backgroundColor || 
                    pixelContext.left == backgroundColor) {
                    tex.SetPixel(column, row, thisPixel - Color.white * shadingIntensity);
                }
            }
        }
    }

    public static void Highlight(ref Texture2D tex, Color backgroundColor, float highlightIntensity) {
        for (var column = 0; column < tex.width; column++) {
            for (var row = 0; row < tex.height; row++) {
                var thisPixel = tex.GetPixel(column, row);
                if (thisPixel == backgroundColor) continue;
                var pixelContext = ColorPixel9WayContext.GetPixelContext(tex, column, row);
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
}