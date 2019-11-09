using System;
using UnityEngine;

public static class Eagle {
    public static void SetPixelSampled(Texture2D tex, ref Texture2D newTex, int newColumn, int newRow, int scale) {
        switch (scale) {
            case 2:
                var pixelContext = GetPixelContext(tex, newColumn.Downsample(scale), newRow.Downsample(scale));
                newTex.SetPixel(newColumn, newRow, pixelContext.Context(GetOrientation(newColumn, newRow, scale)));
                break;
            case 3: 
                var pixelContext3 = GetPixelContext(tex, newColumn.Downsample(scale), newRow.Downsample(scale));
                newTex.SetPixel(newColumn, newRow, pixelContext3.Context(GetOrientation(newColumn, newRow, scale)));
                break;
        }
    }

    static Orientation GetOrientation(int column, int row, int scale) {
        if (scale == 2) {
            var right = column % 2 == 1;
            var up = row % 2 == 1;
            if (right && up) return Orientation.UpRight;
            if (!right && up) return Orientation.UpLeft;
            if (right) return Orientation.DownRight;
            return Orientation.DownLeft;
        }

        if (scale == 3) {
            var pixelColumn = column % 3;
            var pixelRow = row % 3;
            if (pixelColumn == 0 && pixelRow == 0) return Orientation.DownLeft;
            if (pixelColumn == 0 && pixelRow == 1) return Orientation.Left;
            if (pixelColumn == 0 && pixelRow == 2) return Orientation.UpLeft;
            if (pixelColumn == 1 && pixelRow == 0) return Orientation.Down;
            if (pixelColumn == 1 && pixelRow == 1) return Orientation.Center;
            if (pixelColumn == 1 && pixelRow == 2) return Orientation.Up;
            if (pixelColumn == 2 && pixelRow == 0) return Orientation.DownRight;
            if (pixelColumn == 2 && pixelRow == 1) return Orientation.Right;
            if (pixelColumn == 2 && pixelRow == 2) return Orientation.UpRight;
        }

        return Orientation.Center;
    }

    static ColorPixel9WayContext GetPixelContext(Texture2D tex, int column, int row) =>
        new ColorPixel9WayContext(
            center: tex.GetPixel(column, row),
            upLeft: tex.GetPixel(column - 1, row + 1),
            up: tex.GetPixel(column, row + 1),
            upRight: tex.GetPixel(column + 1, row + 1),
            left: tex.GetPixel(column - 1, row),
            right: tex.GetPixel(column + 1, row),
            downLeft: tex.GetPixel(column - 1, row - 1),
            down: tex.GetPixel(column, row - 1),
            downRight: tex.GetPixel(column + 1, row - 1)
        );
}

public class ColorPixel9WayContext {
    readonly Color center;
    readonly Color? upLeft, up, upRight, left, right, downLeft, down, downRight;

    public ColorPixel9WayContext(
        Color center, 
        Color? upLeft, Color? up, Color? upRight, 
        Color? left, Color? right,
        Color? downLeft, Color? down, Color? downRight) {
        this.center = center;
        this.upLeft = upLeft; 
        this.up = up; 
        this.upRight = upRight; 
        this.left = left; 
        this.right = right; 
        this.downLeft = downLeft; 
        this.down = down; 
        this.downRight = downRight; 
    }

    Color UpLeftColor() => 
        (upLeft != null && left != null && up != null) && 
        (upLeft.Value == left.Value && upLeft.Value == up.Value) ? 
        upLeft.Value : center;
    Color UpRightColor() => 
        (upRight != null && right != null && up != null) && 
        (upRight.Value == right.Value && upRight.Value == up.Value) ? 
        upRight.Value : center;
    Color DownLeftColor() => 
        (downLeft != null && left != null && down != null) && 
        (downLeft.Value == left.Value && downLeft.Value == down.Value) ? 
        downLeft.Value : center;
    Color DownRightColor() => 
        (downRight != null && right != null && down != null) && 
        (downRight.Value == right.Value && downRight.Value == down.Value) ? 
        downRight.Value : center;

    public Color Context(Orientation orientation) {
        switch (orientation) {
            case Orientation.UpLeft:
                return UpLeftColor();
            case Orientation.Up:
                return center;
            case Orientation.UpRight:
                return UpRightColor();
            case Orientation.Left:
            case Orientation.Center:
            case Orientation.Right:
                return center;
            case Orientation.DownLeft:
                return DownLeftColor();
            case Orientation.Down:
                return center;
            case Orientation.DownRight:
                return DownRightColor();
            default:
                throw new Exception("Unhandled orientation.");
        }
    }
}

public enum Orientation {
    UpLeft,
    Up,
    UpRight,
    Left,
    Center,
    Right,
    DownLeft,
    Down,
    DownRight
}
