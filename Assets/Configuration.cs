using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Layout {
    public int spacing;
    public int imageGridSize;
}

[Serializable]
public class Configuration {
    public Layout layout;
    
    [Header("Sprite")] 
    public int spritePixelSize = 16;

    [Header("Noise")]
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

    [Header("Falloff")]
    public AnimationCurve falloffCurve;

    [Header("Sprite Color")] 
    public bool colorEnabled;
    public int paletteIndex;
    public int colorCountPerSprite;
    public bool overridePaletteColorsWithRandomColors;

    [Header("Background Color")]
    public bool randomPaletteColorForBackground;
    public int paletteColorIndexForBackground;
    public bool overrideBackgroundColor;
    public Color backgroundColorOverride;

    [Header("Outline")]
    public bool outlineEnabled;
    public bool applyOutlineAfterScaling;
    
    public bool randomPaletteColorForOutline;
    public int paletteColorIndexForOutline;
    public bool overrideOutlineColor;
    public Color outlineColorOverride;
    
    [Header("Symmetry")]
    [Range(0f,1f)] public float horizontalSymmetryChance;
    [Range(0f,1f)] public float verticalSymmetryChance;
    [Range(0f,1f)] public float backwardDiagonalSymmetryChance;
    [Range(0f,1f)] public float forwardDiagonalSymmetryChance;
    public bool allowQuarterSymmetry;

    [Header("Scaling")]
    public ScalingMode[] scalingModes;
    public FilterMode filterMode;

    [Header("Animation")] 
    [Range(1, 8)] public int animationFrameCount = 1;
    public float timeBetweenFrames;
    public AnimationMode animationMode;

    [Header("Cleanup")] 
    public bool allowPixelsOnEdgeOfSprite;
    public LonePixelEvaluationMode lonePixelEvaluationMode;
    [Range(0f,1f)] public float chanceToDeleteLonePixels;
}

[Serializable]
public class Preset {
    public string name;
    public Configuration configuration;

    public Preset(string name, Configuration config) {
        this.name = name;
        configuration = config;
    }
}

public enum Presets {
    none = 0, bitsy, island, spaceships, advanced_spaceships, transforming_spaceships,
    pocket_monsters, pocket_monsters_low_rez_symmetrical, pocket_monsters_low_rez_outlined_imperfect_symmetry
}

public enum ScalingMode { none, x2, x4, x10, eagle2, eagle3 }
public enum AnimationMode { loop, pingPong }
public enum LonePixelEvaluationMode { CardinalDirectionsOnly, IncludeDiagonals }
