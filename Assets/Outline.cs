using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour {
    [SerializeField] bool outline;
    List<Vector2Int> outlineCoordinates = new List<Vector2Int>();
    
    public void OutlineTexture(ref Texture2D texture, Color backgroundColor, Color outlineColor) {
        if (!outline)
            return;
        
        outlineCoordinates.Clear();
        for (var row = 0; row < texture.height; row++) {
            for (var column = 0; column < texture.width; column++) {
                var col = texture.GetPixel(column, row);
                if (col == backgroundColor)
                    AttemptToOutline(ref texture, backgroundColor, new Vector2Int(column, row));
            }
        }

        foreach (var outlineCoordinate in outlineCoordinates)
            texture.SetPixel(outlineCoordinate.x, outlineCoordinate.y, outlineColor);
    }

    void AttemptToOutline(ref Texture2D texture, Color backgroundColor, Vector2Int coordinates) {
        var pixelContext = GetContext(coordinates, texture, backgroundColor);
        if (pixelContext.AnyAdjacentNonBackgroundPixels()) //TODO: or has a color and adjacent to end of texture
            outlineCoordinates.Add(new Vector2Int(coordinates.x, coordinates.y));
    }

    BooleanPixel4WayContext GetContext(Vector2Int coordinates, Texture2D texture, Color backgroundColor) =>
        new BooleanPixel4WayContext(
            coordinates.y > 0 && texture.GetPixel(coordinates.x, coordinates.y-1) != backgroundColor,
            coordinates.x > 0 && texture.GetPixel(coordinates.x-1,coordinates.y) !=backgroundColor,
            coordinates.y < texture.height && texture.GetPixel(coordinates.x, coordinates.y+1) != backgroundColor,
            coordinates.x < texture.width && texture.GetPixel(coordinates.x+1,coordinates.y) != backgroundColor);
}

class BooleanPixel4WayContext {
    readonly bool upPixel, downPixel, leftPixel, rightPixel;

    public BooleanPixel4WayContext(bool up, bool down, bool left, bool right) {
        upPixel = up;
        downPixel = down;
        leftPixel = left;
        rightPixel = right;
    }

    public bool AnyAdjacentNonBackgroundPixels() => upPixel | downPixel | leftPixel | rightPixel;
}
