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

    public void Generate()
    {
        Reset();
        SetUpGridLayoutGroup();

        for (var i = 0; i < configuration.layout.imageGridSize * configuration.layout.imageGridSize; i++)
        {
            var sprite = Instantiate(spritePrefab, spriteParent);
            var frameAnimation = sprite.GetComponent<FrameAnimation>();
            frameAnimation.FrameTime = configuration.animationConfig.timeBetweenFrames;
            frameAnimation.animationMode = configuration.animationConfig.animationMode;
            frameAnimation.Frames = spriteGeneration.Generate(configuration);
            currentFrameAnimations.Add(frameAnimation);
        }

        void SetUpGridLayoutGroup() {
            var cellSize = (Screen.height / ((float) configuration.layout.imageGridSize)) - configuration.layout.spacing;
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.constraintCount = configuration.layout.imageGridSize;
            gridLayoutGroup.spacing = new Vector2(configuration.layout.spacing, configuration.layout.spacing);
            var padding = Mathf.Clamp(configuration.layout.spacing * 4, 32, 64);
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
            configuration.layout,
            configuration.spriteConfig,
            configuration.noiseConfig,
            configuration.falloffConfig,
            configuration.colorConfig,
            configuration.backgroundColorConfig,
            configuration.outlineConfig,
            configuration.symmetryConfig,
            configuration.scalingConfig,
            configuration.animationConfig,
            configuration.shadingConfig,
            configuration.cleanupConfig
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
        var scalingFactor = Scaling.ScalingFactorMultiple(configuration.scalingConfig.scalingModes);

        var scaledPixelSize = configuration.spriteConfig.pixelSize * scalingFactor;
        var scaledSpacing = (configuration.layout.spacing / 10) * scalingFactor;
        var gridSize = configuration.layout.imageGridSize;
        var frameCount = configuration.animationConfig.animationFrameCount;

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

