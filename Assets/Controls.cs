using System;
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
        DrawDefaultInspector();
    }
}

public class Controls : MonoBehaviour
{
    [Header("Configuration")] 
    [SerializeField] Configuration configuration;
    [SerializeField] Presets preset;

    [Header("Presets")] 
    [SerializeField] Preset[] presets;
    
    [Header("References")]
    [SerializeField] Transform spriteParent;
    [SerializeField] GameObject spritePrefab;
    [SerializeField] SpriteGeneration spriteGeneration;
    [SerializeField] GridLayoutGroup gridLayoutGroup;

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
    }
}

[Serializable]
public class Preset {
    public string name;
    public Configuration configuration;
}

public enum Presets {none = 0, bitsy = 1, island}

[Serializable]
public class Configuration {
    [Header("Layout")] 
    public int spacing;
    public int imageGridSize;

    [Header("Sprite")] 
    public int spritePixelSize = 16;
    
    [Header("Noise")]
    public List<Octave> octaves;
    public bool randomOrigin;
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

public enum ScalingMode { none, x2, x4, eagle2, eagle3 }
public enum AnimationMode { loop, pingPong }
public enum LonePixelEvaluationMode { CardinalDirectionsOnly, IncludeDiagonals }