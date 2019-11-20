using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    [SerializeField] ConfigurationAsset configuration;

    [Header("References")]
    [SerializeField] Transform spriteParent;
    [SerializeField] GameObject spritePrefab;
    [SerializeField] SpriteGeneration spriteGeneration;
    [SerializeField] GridLayoutGroup gridLayoutGroup;

    public List<FrameAnimation> currentFrameAnimations;

    public ConfigurationAsset Configuration {
        get => configuration;
        set => configuration = value;
    }

    public void Generate()
    {
        Reset();
        SetUpGridLayoutGroup();

        for (var i = 0; i < Configuration.sizingConfig.imageGridSize * Configuration.sizingConfig.imageGridSize; i++)
        {
            var sprite = Instantiate(spritePrefab, spriteParent);
            var frameAnimation = sprite.GetComponent<FrameAnimation>();
            frameAnimation.FrameTime = Configuration.animationConfig.timeBetweenFrames;
            frameAnimation.animationMode = Configuration.animationConfig.animationMode;
            frameAnimation.Frames = spriteGeneration.Generate(Configuration);
            currentFrameAnimations.Add(frameAnimation);
        }

        void SetUpGridLayoutGroup() {
            var cellSize = (Screen.height / ((float) Configuration.sizingConfig.imageGridSize)) - Configuration.sizingConfig.spacing;
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.constraintCount = Configuration.sizingConfig.imageGridSize;
            gridLayoutGroup.spacing = new Vector2(Configuration.sizingConfig.spacing, Configuration.sizingConfig.spacing);
            var padding = Mathf.Clamp(Configuration.sizingConfig.spacing * 4, 32, 64);
            gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
        }
    }

    public void Reset()
    {
        for (var i = spriteParent.childCount - 1; i > -1; i--) 
            DestroyImmediate(spriteParent.GetChild(i).gameObject);
        currentFrameAnimations.Clear();
    }

    public void SaveAsPreset() {
        var preset = Instantiate(ScriptableObject.CreateInstance<ConfigurationAsset>());
        preset.SetValues(
            Configuration.sizingConfig,
            Configuration.noiseConfig,
            Configuration.falloffConfig,
            Configuration.colorConfig,
            Configuration.backgroundColorConfig,
            Configuration.outlineConfig,
            Configuration.symmetryConfig,
            Configuration.scalingConfig,
            Configuration.animationConfig,
            Configuration.shadingConfig,
            Configuration.cleanupConfig
            );
        
        SaveScriptableObject();

        void SaveScriptableObject() {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            } else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            var assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(path + "/Configuration/New " + typeof(ConfigurationAsset) + ".asset");

            AssetDatabase.CreateAsset(preset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = preset;
        }
    }

    public void SaveSpritesheet() {
        var scalingFactor = Scaling.ScalingFactorMultiple(Configuration.scalingConfig.scalingModes);

        var scaledPixelSize = Configuration.sizingConfig.pixelSize * scalingFactor;
        var scaledSpacing = (Configuration.sizingConfig.spacing / 10) * scalingFactor;
        var gridSize = Configuration.sizingConfig.imageGridSize;
        var frameCount = Configuration.animationConfig.animationFrameCount;

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

