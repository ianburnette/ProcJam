using System.Collections.Generic;
using UnityEngine;

public static class FrameInterpolation {
    public static List<GeneratedTexture> AddInterpolationFrames(
        List<GeneratedTexture> inputFrames, int interpolationFrameCount) {
        var interpolatedTextures = new List<GeneratedTexture>();
        for (var frame = 0; frame < inputFrames.Count - 1; frame++) {
            var comparisonTexture = inputFrames[frame + 1].texture;
            var thisFrame = inputFrames[frame];
            var thisTexture = thisFrame.texture;
            var interpolationTexture = new Texture2D(thisTexture.width, thisTexture.height);
            var unsetColor = interpolationTexture.GetPixel(0, 0);

            var newTextureColors = new List<(Vector2Int, Color)>();
            
            for (var column = 0; column < thisTexture.height; column++) {
                for (var row = 0; row < thisTexture.width; row++) {
                    var thisPixel = thisTexture.GetPixel(column, row);
                    var comparisonPixel = comparisonTexture.GetPixel(column, row);
                    if (thisPixel == comparisonPixel) {
                        newTextureColors.Add((new Vector2Int(column, row), thisPixel));
                        //interpolationTexture.SetPixel(column, row, thisPixel);
                        continue;
                    }
                    //if (interpolationTexture.GetPixel(column, row) != unsetColor)
                    //    continue;

                    var thisPixelCoordinates = new Vector2Int(column, row);
                    var nearestPixelsOfThisColorInComparisonTexture =
                        GetNearestPixelsOfColor(thisPixelCoordinates, thisPixel, comparisonTexture);

                    foreach (var nearPixelCoordinates in nearestPixelsOfThisColorInComparisonTexture) {
                        var averagePosition = (nearPixelCoordinates + thisPixelCoordinates) / 2;
                        if (interpolationTexture.GetPixel(averagePosition.x, averagePosition.y) != unsetColor)
                            newTextureColors.Add((averagePosition, thisPixel));
                        //    interpolationTexture.SetPixel(averagePosition.x, averagePosition.y, thisPixel);

                    }

                    newTextureColors.Add((new Vector2Int(column, row), thisFrame.colorOutcome.backgroundColor));

//                    interpolationTexture.SetPixel(column, row, thisFrame.colorOutcome.backgroundColor);
                }
            }

            foreach (var newTextureColor in newTextureColors) {
                interpolationTexture.SetPixel(newTextureColor.Item1.x, newTextureColor.Item1.y, newTextureColor.Item2);
            }
            interpolationTexture.filterMode = thisFrame.filterMode;
            interpolationTexture.Apply();
            interpolatedTextures.Add(inputFrames[frame]);
            interpolatedTextures.Add(new GeneratedTexture(interpolationTexture,
                thisFrame.normal, thisFrame.origin, thisFrame.symmetryOutcome, thisFrame.colorOutcome));
        }

        interpolatedTextures.Add(inputFrames[inputFrames.Count - 1]);

        return interpolatedTextures;
    }

    static List<Vector2Int> GetNearestPixelsOfColor(Vector2Int referenceCoordinates, Color thisPixel, Texture2D texture) {
        var pixelCoordinatesOfTargetColor = new List<Vector2Int>();
        for (var column = 0; column < texture.height; column++) {
            for (var row = 0; row < texture.width; row++) {
                if (texture.GetPixel(column, row) == thisPixel) {
                    pixelCoordinatesOfTargetColor.Add(new Vector2Int(column, row));
                }
            }
        }

        var closestPixelCoordinatesOfTargetColor = new List<Vector2Int>();
        var shortestDistance = 1000f;

        foreach (var pixel in pixelCoordinatesOfTargetColor) {
            var distance = Vector2Int.Distance(pixel, referenceCoordinates);
            if (!(distance < shortestDistance)) continue;
            
            closestPixelCoordinatesOfTargetColor.Clear();
            closestPixelCoordinatesOfTargetColor.Add(pixel);
            shortestDistance = distance;
        }

        return closestPixelCoordinatesOfTargetColor;
    }
} 
