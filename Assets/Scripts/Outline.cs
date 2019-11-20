using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour {
    List<Vector2Int> outlineCoordinates = new List<Vector2Int>();
    
    public void OutlineTexture(ref Texture2D texture, Color backgroundColor, Color outlineColor) {
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
        var pixelContext = BooleanPixel4WayContext.GetContext(coordinates, texture, backgroundColor);
        if (pixelContext.AnyAdjacentNonBackgroundPixels()) //TODO: or has a color and adjacent to end of texture
            outlineCoordinates.Add(new Vector2Int(coordinates.x, coordinates.y));
    }
}

class BooleanPixel4WayContext {
    readonly bool upPixel, downPixel, leftPixel, rightPixel;

    public static BooleanPixel4WayContext GetContext(Vector2Int coordinates, Texture2D texture, Color backgroundColor) =>
        new BooleanPixel4WayContext(
            coordinates.y > 0 && texture.GetPixel(coordinates.x, coordinates.y-1) != backgroundColor,
            coordinates.x > 0 && texture.GetPixel(coordinates.x-1,coordinates.y) !=backgroundColor,
            coordinates.y < texture.height && texture.GetPixel(coordinates.x, coordinates.y+1) != backgroundColor,
            coordinates.x < texture.width && texture.GetPixel(coordinates.x+1,coordinates.y) != backgroundColor);

    public BooleanPixel4WayContext(bool up, bool down, bool left, bool right) {
        upPixel = up;
        downPixel = down;
        leftPixel = left;
        rightPixel = right;
    }

    public bool AnyAdjacentNonBackgroundPixels() => upPixel | downPixel | leftPixel | rightPixel;

    public bool IsSpeckle() => !upPixel && !downPixel && !leftPixel && !rightPixel;
}

class BooleanPixel8WayContext {
    readonly bool upLeftPixel, upPixel, upRightPixel, leftPixel, rightPixel, downLeftPixel, downPixel, downRightPixel;

    public BooleanPixel8WayContext(
        bool upLeft, bool up, bool upRight, bool left, bool right, bool downLeft, bool down, bool downRight) {
        upLeftPixel = upLeft; 
        upPixel = up; 
        upRightPixel = upRight; 
        leftPixel = left; 
        rightPixel = right; 
        downLeftPixel = downLeft; 
        downPixel = down; 
        downRightPixel = downRight; 
    }

    public static BooleanPixel8WayContext GetPixelContext(Texture2D tex, int column, int row, Color backgroundColor) {
        var upLeft = tex.GetPixel(column - 1, row + 1);
        var up = tex.GetPixel(column, row + 1);
        var upRight = tex.GetPixel(column + 1, row + 1);
        var left = tex.GetPixel(column - 1, row);
        var right = tex.GetPixel(column + 1, row);
        var downLeft = tex.GetPixel(column - 1, row - 1);
        var down = tex.GetPixel(column, row - 1);
        var downRight = tex.GetPixel(column + 1, row - 1);
        
        return new BooleanPixel8WayContext(
            upLeft: upLeft != null && upLeft != backgroundColor, 
            up: up != null && up != backgroundColor, 
            upRight: upRight != null && upRight != backgroundColor, 
            left: left != null && left != backgroundColor, 
            right: right != null && right != backgroundColor, 
            downLeft: downLeft != null && downLeft != backgroundColor, 
            down: down != null && down != backgroundColor, 
            downRight: downRight != null && downRight != backgroundColor 
        );
    }

    public bool IsSpeckle() => !upLeftPixel && !upPixel && !upRightPixel &&
                               !leftPixel && !rightPixel &&
                               !downLeftPixel && !downPixel && !downRightPixel;

}
