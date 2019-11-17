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
            gridLayoutGroup.padding = new RectOffset(configuration.layout.spacing, configuration.layout.spacing, configuration.layout.spacing, configuration.layout.spacing);
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
        var scalingFactor = Scaling.ScalingFactorMultiple(configuration.scalingConfig.scalingModes);

        var scaledPixelSize = configuration.spriteConfig.spritePixelSize * scalingFactor;
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

