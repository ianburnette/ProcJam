using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class SizingConfig {
    public int spacing = 16;
    public int imageGridSize = 16;
    public int pixelSize = 16;
}

[Serializable]
public class NoiseConfig{
    public bool randomizeFrequency;
    public List<Octave> octaves = new List<Octave> {
        new Octave(5f, .8f),
        new Octave(10f, .25f),
        new Octave(20f, .125f),
        new Octave(45, .0625f)
    };
    public bool randomOrigin = true;
    public float randomOriginBound = 255f;
    public Vector2 manualOrigin;
    public float animationFrameNoiseOffset = .2f;
}

[Serializable]
public class FalloffConfig{
    public AnimationCurve falloffCurve;
}

[Serializable]
public class ColorConfig{
    public bool colorEnabled;
    public int paletteIndex;
    public int colorCountPerSprite;
    public bool usePaletteColors;
    public bool colorLocked;
    public List<GeneratedTexture> lockedColorTextures;
}

[Serializable]
public class BackgroundColorConfig{
    public bool randomPaletteColorForBackground;
    public int paletteColorIndexForBackground;
    public bool overrideBackgroundColor;
    public Color backgroundColorOverride;
}

[Serializable]
public class OutlineConfig{
    public bool outlineEnabled;
    public bool applyOutlineAfterScaling;

    public bool randomPaletteColorForOutline;
    public int paletteColorIndexForOutline;
    public bool overrideOutlineColor;
    public Color outlineColorOverride;
}

[Serializable]
public class SymmetryConfig {
    public bool allowMultipleSymmetryTypes = true;
    public bool enforceSomeTypeOfSymmetry;
    
    [Header("Horizontal")]
    [Range(0f,1f)] public float horizontalSymmetryChance;
    [Range(0f,1f)] public float quarterHorizontalSymmetryChance;
    
    [Header("Vertical")]
    [Range(0f,1f)] public float verticalSymmetryChance;
    [Range(0f,1f)] public float quarterVerticalSymmetryChance;
    
    [Header("Forward Diagonal")]
    [Range(0f,1f)] public float forwardDiagonalSymmetryChance;    
    [Range(0f,1f)] public float quarterForwardDiagonalSymmetryChance;
    
    [Header("Backward Diagonal")]
    [Range(0f,1f)] public float backwardDiagonalSymmetryChance;
    [Range(0f,1f)] public float quarterBackwardDiagonalSymmetryChance;
}

[Serializable]
public class SymmetryConfig3D {
    public bool allowMultipleSymmetryTypes = true;
    public bool enforceSomeTypeOfSymmetry;
    // https://images.slideplayer.com/26/8501691/slides/slide_24.jpg
    [Range(0f,1f)] public float eastTopToWestBottomChance;
    [Range(0f,1f)] public float southBottomToNorthTopChance;
    [Range(0f,1f)] public float westBottomToEastTopChance;
    [Range(0f,1f)] public float southTopToNorthBottomChance;
    [Range(0f,1f)] public float southEastCenterToNorthWestCenterChance;    
    [Range(0f,1f)] public float southWestCenterToNorthEastCenterChance;
    [Range(0f,1f)] public float southCenterToNorthCenterVerticalChance;
    [Range(0f,1f)] public float southCenterToNorthCenterHorizontalChance;
    [Range(0f,1f)] public float centerUpToCenterDownChance;

    public float ChanceOf(SymmetryDirection3D direction) {
        switch (direction) {
            case SymmetryDirection3D.EastTopToWestBottom: return eastTopToWestBottomChance;
            case SymmetryDirection3D.SouthBottomToNorthTop: return southBottomToNorthTopChance;
            case SymmetryDirection3D.WestBottomToEastTop: return westBottomToEastTopChance;
            case SymmetryDirection3D.SouthTopToNorthBottom: return southTopToNorthBottomChance;
            case SymmetryDirection3D.SouthEastCenterToNorthWestCenter: return southEastCenterToNorthWestCenterChance;
            case SymmetryDirection3D.SouthWestCenterToNorthEastCenter: return southWestCenterToNorthEastCenterChance;
            case SymmetryDirection3D.SouthCenterToNorthCenterVertical: return southCenterToNorthCenterVerticalChance;
            case SymmetryDirection3D.SouthCenterToNorthCenterHorizontal: return southCenterToNorthCenterHorizontalChance;
            case SymmetryDirection3D.CenterUpToCenterDown: return centerUpToCenterDownChance;
            default: return 0;
        }
    }
}

[Serializable]
public class ScalingConfig{
    public ScalingMode[] scalingModes;
    public FilterMode filterMode;

    public void ResizeScalingMode(int newSize) {
        if (newSize == scalingModes.Length) return;
        var newScalingModes = new ScalingMode[newSize];
        for (int i = 0; i < scalingModes.Length; i++) {
            if(i>=newSize) continue;
            newScalingModes[i] = scalingModes[i];
        }
        scalingModes = newScalingModes;
    }
}

[Serializable]
public class AnimationConfig{
[Range(1, 8)] public int animationFrameCount = 1;
public float timeBetweenFrames;
public AnimationMode animationMode;
}

[Serializable]
public class ShadingConfig {
    public bool enableShading;
    public bool shadingByColor;
    [Range(0, 1)] public float shadingIntensity;
    public bool enableHighlights;
    public bool highlightByColor;
    [Range(0, 1)] public float highlightIntensity;
}

[Serializable]
public class CleanupConfig{
    public bool allowPixelsOnEdgeOfSprite;
    public LonePixelEvaluationMode lonePixelEvaluationMode;
    [Range(0f,1f)] public float chanceToDeleteLonePixels;
}

[Serializable]
public class NormalsConfig {
    public bool enableNormals;
    public float normalStrength = .5f;
    public FilterMode filterMode;
    
    public bool viewNormalsOnly;
    public bool disableNormalsDisplay;
    public bool rotatingLightEnabled;
    public bool cursorFollowLightEnabled;
    public bool globalLightEnabled = true;
}

//TODO: the pure multipliers should be extracted to a export class, as they don't affect in-game appearance
public enum ScalingMode { none = 0, x2, x4, x10, eagle2, eagle3 }
public enum AnimationMode { loop, pingPong }
public enum LonePixelEvaluationMode { CardinalDirectionsOnly, IncludeDiagonals }
