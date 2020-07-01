using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.XR;

#if UNITY_EDITOR
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
#endif

public class Controls : MonoBehaviour {
    public static Controls instance;
    
    [Header("Configuration")] 
    [SerializeField] ConfigurationAsset configuration;

    [Header("References")]
    [SerializeField] Transform spriteParent;
    [SerializeField] GameObject spritePrefab;
    [SerializeField] public SpriteGeneration spriteGeneration;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] public List<ConfigurationAsset> configurationAssets;

    public List<FrameAnimation> currentFrameAnimations;

    public ConfigurationAsset Configuration {
        get => configuration;
        set => configuration = value;
    }

    public SpriteGeneration Generation {
        get => spriteGeneration;
        set => spriteGeneration = value;
    }

    void OnEnable() => instance = this;

    public void Evolve(List<GeneratedTexture> sourceGeneratedTextures, EvolutionType type) => 
        GenerateEvolution(new EvolutionConfig(sourceGeneratedTextures, type, 
        new InheritedSymmetryConfig(true, sourceGeneratedTextures[0].symmetryOutcome)));

    public void GenerateEvolution(EvolutionConfig evolutionConfig) {
        Reset();
        var gridSize = Configuration.sizingConfig.imageGridSize;
        var rowCenter = (int) Math.Floor(gridSize / 2f);
        var centerPoint = new Vector2Int(rowCenter, rowCenter);
        for (var column = 0; column < gridSize; column++) {
            for (var row = 0; row < gridSize; row++) {
                evolutionConfig.offsetFromSource = new Vector2Int(column - centerPoint.x, row - centerPoint.y);
                var frameAnimation = SetUpSprite();
                var (diffuse, normal, generatedTextures) = Generation.Generate(configuration, evolutionConfig);
                AssignSprite(frameAnimation, diffuse, normal, generatedTextures);
                currentFrameAnimations.Add(frameAnimation);
            }
        }
    }
    
    public void Generate()
    {
        Reset();
        SetUpGridLayoutGroup();

        for (var i = 0; i < Configuration.sizingConfig.imageGridSize * Configuration.sizingConfig.imageGridSize; i++)
        {
            var frameAnimation = SetUpSprite();
            var (diffuse, normal, generatedTextures) = Generation.Generate(configuration, null);
            AssignSprite(frameAnimation, diffuse, normal, generatedTextures);
            currentFrameAnimations.Add(frameAnimation);
        }

        void SetUpGridLayoutGroup() {
            var cellSize = (Screen.height / ((float) Configuration.sizingConfig.imageGridSize)) - Configuration.sizingConfig.spacing;
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.constraintCount = Configuration.sizingConfig.imageGridSize;
            gridLayoutGroup.spacing = new Vector2(Configuration.sizingConfig.spacing, Configuration.sizingConfig.spacing);
            var padding = Mathf.Clamp(Configuration.sizingConfig.spacing * 4, 32, 32);
            gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
        }
    }

    void AssignSprite(FrameAnimation frameAnimation, List<Sprite> diffuse, List<Sprite> normal, List<GeneratedTexture> generatedTextures) {
        frameAnimation.DiffuseFrames = diffuse;
        if (configuration.normalsConfig.enableNormals) frameAnimation.NormalFrames = normal;
        frameAnimation.GeneratedTextures = generatedTextures;
        frameAnimation.enableNormals = configuration.normalsConfig.enableNormals;
    }

    FrameAnimation SetUpSprite() {
        var sprite = Instantiate(spritePrefab, spriteParent);
        var frameAnimation = sprite.GetComponent<FrameAnimation>();
        frameAnimation.FrameTime = Configuration.animationConfig.timeBetweenFrames;
        frameAnimation.animationMode = Configuration.animationConfig.animationMode;
        return frameAnimation;
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
            Configuration.symmetryConfig3D,
            Configuration.scalingConfig,
            Configuration.animationConfig,
            Configuration.shadingConfig,
            Configuration.cleanupConfig,
            configuration.normalsConfig
        );
        #if UNITY_EDITOR        
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
        #endif
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

        var generatedTexture = GenerateTexture(diffuse: true);

        Texture2D normalsTexture = null;
        if (configuration.normalsConfig.enableNormals) normalsTexture = GenerateTexture(diffuse: false);

        //    Scaling.ScaleTexture(ref generatedTexture, configuration.scalingMode);

        Texture2D GenerateTexture(bool diffuse) {
            var texture = new Texture2D(scaledNewTextureFrameWidth * frameCount, scaledNewTextureFrameHeight);

            var backgroundPixels = new Color[scaledNewTextureWidth * scaledNewTextureFrameHeight];
            for (var i = 0; i < scaledNewTextureWidth * scaledNewTextureFrameHeight; i++)
                backgroundPixels[i] = Generation.backgroundColor;
            texture.SetPixels(0, 0, scaledNewTextureFrameWidth * frameCount, scaledNewTextureFrameHeight,
                backgroundPixels);

            for (var frame = 0; frame < frameCount; frame++) {
                var spriteIndex = 0;
                for (var column = gridSize - 1; column >= 0; column--) {
                    for (var row = 0; row < gridSize; row++) {
                        var targetXcoord = scaledSpacing + ((row * scaledImageSize) + (frame * scaledNewTextureFrameWidth)) +
                                           scaledSpacing;
                        var targetYcoord = scaledSpacing + (column * scaledImageSize) + scaledSpacing;
                        texture.SetPixels(
                            targetXcoord,
                            targetYcoord,
                            scaledPixelSize, scaledPixelSize,
                            diffuse ? 
                            currentFrameAnimations[spriteIndex].DiffuseFrames[frame].texture.GetPixels(0, 0, scaledPixelSize, scaledPixelSize) : 
                            currentFrameAnimations[spriteIndex].NormalFrames[frame].texture.GetPixels(0, 0, scaledPixelSize, scaledPixelSize)
                            ); 
                        spriteIndex++;
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        var time = DateTime.Now.Ticks;
        FrameAnimation.ExportTexture(generatedTexture, "Exported Spritesheets", "exported_spritesheet", time);
        if (configuration.normalsConfig.enableNormals)
            FrameAnimation.ExportTexture(normalsTexture, "Exported Spritesheets", "exported_spritesheet_n", time);
            
    }

}

