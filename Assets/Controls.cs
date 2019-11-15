﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(Controls))]
public class MapGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = (Controls)target;
        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate")) myScript.Generate();
        if(GUILayout.Button("Reset")) myScript.Reset();
        if(GUILayout.Button("Save Entire Spritesheet")) myScript.SaveSpritesheet();
        if (GUILayout.Button("Save Config as New Preset")) myScript.SaveAsPreset();
        DrawDefaultInspector();
    }
}

public class Controls : MonoBehaviour
{
    [Header("Configuration")] 
    [SerializeField] Presets preset;
    [SerializeField] Configuration configuration;

    [Header("Presets")] 
    [SerializeField] List<Preset> presets;
    
    [Header("References")]
    [SerializeField] Transform spriteParent;
    [SerializeField] GameObject spritePrefab;
    [SerializeField] SpriteGeneration spriteGeneration;
    [SerializeField] GridLayoutGroup gridLayoutGroup;

    public List<FrameAnimation> currentFrameAnimations; 

    public void Generate()
    {
        Reset();
        SetUpGridLayoutGroup();

        if (preset != 0)
            configuration = presets[(int)preset - 1].configuration;
        
        for (var i = 0; i < configuration.imageGridSize * configuration.imageGridSize; i++)
        {
            var sprite = Instantiate(spritePrefab, spriteParent);
            var frameAnimation = sprite.GetComponent<FrameAnimation>();
            frameAnimation.FrameTime = configuration.timeBetweenFrames;
            frameAnimation.animationMode = configuration.animationMode;
            frameAnimation.Frames = spriteGeneration.Generate(configuration);
            currentFrameAnimations.Add(frameAnimation);
        }

        void SetUpGridLayoutGroup() {
            var cellSize = (Screen.height / ((float) configuration.imageGridSize)) - configuration.spacing;
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.constraintCount = configuration.imageGridSize;
            gridLayoutGroup.spacing = new Vector2(configuration.spacing, configuration.spacing);
            gridLayoutGroup.padding = new RectOffset(configuration.spacing, configuration.spacing, configuration.spacing, configuration.spacing);
        }
    }

    public void Reset()
    {
        for (var i = spriteParent.childCount - 1; i > -1; i--) 
            DestroyImmediate(spriteParent.GetChild(i).gameObject);
        currentFrameAnimations.Clear();
    }

    public void SaveAsPreset() => presets.Add(new Preset("unnamed preset", configuration));

    public void SaveSpritesheet() {
        var scalingFactor = Scaling.ScalingFactor(configuration.scalingMode);

        var scaledPixelSize = configuration.spritePixelSize * scalingFactor;
        var scaledSpacing = (configuration.spacing / 10) * scalingFactor;
        var gridSize = configuration.imageGridSize;
        var frameCount = configuration.animationFrameCount;

        var scaledImageSize = scaledSpacing + scaledPixelSize + scaledSpacing;
        var scaledNewTextureFrameWidth = scaledImageSize * gridSize + scaledSpacing * 2;
        var scaledNewTextureWidth = scaledNewTextureFrameWidth * frameCount;
        var scaledNewTextureFrameHeight = scaledImageSize * gridSize + scaledSpacing * 2;
        
        var generatedTexture = new Texture2D(scaledNewTextureFrameWidth * frameCount, scaledNewTextureFrameHeight);
        
        var backgroundPixels = new Color[scaledNewTextureWidth * scaledNewTextureFrameHeight];
        for (var i = 0; i < scaledNewTextureWidth * scaledNewTextureFrameHeight; i++)
            backgroundPixels[i] = spriteGeneration.backgroundColor;
        generatedTexture.SetPixels(0,0,scaledNewTextureFrameWidth * frameCount, scaledNewTextureFrameHeight, backgroundPixels);
        
        for (var frame = 0; frame < frameCount; frame++) {
            var spriteIndex = 0;
            for (var column = gridSize - 1; column >= 0; column--) {
                for (var row = 0; row < gridSize; row++) {
                    var targetXcoord = scaledSpacing + ((row * scaledImageSize) + (frame * scaledNewTextureFrameWidth)) + scaledSpacing;
                    var targetYcoord = scaledSpacing + (column * scaledImageSize) + scaledSpacing;
                    generatedTexture.SetPixels(
                        targetXcoord, 
                        targetYcoord, 
                        scaledPixelSize, scaledPixelSize, 
                        currentFrameAnimations[spriteIndex].Frames[frame].texture
                        .GetPixels(0, 0, scaledPixelSize, scaledPixelSize));
                    spriteIndex++;
                }
            }
        }

        //    Scaling.ScaleTexture(ref generatedTexture, configuration.scalingMode);
        
        generatedTexture.Apply();
        FrameAnimation.ExportTexture(generatedTexture, "Exported Spritesheets");
    }
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

[Serializable]
public class Configuration {
    [Header("Layout")] 
    public int spacing;
    public int imageGridSize;

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
    public ScalingMode scalingMode;
    //TODO: second scaling mode

    [Header("Animation")] 
    [Range(1, 8)] public int animationFrameCount = 1;
    public float timeBetweenFrames;
    public AnimationMode animationMode;

    [Header("Cleanup")] 
    public bool allowPixelsOnEdgeOfSprite;
    public LonePixelEvaluationMode lonePixelEvaluationMode;
    [Range(0f,1f)] public float chanceToDeleteLonePixels;
}

public enum ScalingMode { none, x2, x4, x10, eagle2, eagle3 }
public enum AnimationMode { loop, pingPong }
public enum LonePixelEvaluationMode { CardinalDirectionsOnly, IncludeDiagonals }
