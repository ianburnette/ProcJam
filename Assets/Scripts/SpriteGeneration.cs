using System.Collections.Generic;
using UnityEngine;

 public class SpriteGeneration : MonoBehaviour
{
    [Header("Filtering")]
    [SerializeField] Recoloring recoloring;
    [SerializeField] NoiseGeneration noiseGeneration;
    [SerializeField] Symmetry symmetry;
    [SerializeField] Falloff falloff;
    [SerializeField] Outline outline;
    [SerializeField] public NormalGeneration normalGeneration;
    [SerializeField] Cleanup cleanup;

    public Color backgroundColor;

    public Recoloring Recoloring {
        get => recoloring;
        set => recoloring = value;
    }

    public (List<Sprite>, List<Sprite>) Generate(ConfigurationAsset configuration) {
        var sprites = new List<Sprite>();
        var normals = new List<Sprite>();
        for (int i = 0; i < configuration.animationConfig.animationFrameCount; i++) {
            var (texture, normal) = GenerateTexture(i, configuration);
            var diffuseSprite = CreateSprite(texture);
            var normalSprite = CreateSprite(normal);
            sprites.Add(diffuseSprite);
            normals.Add(normalSprite);
        }
        return (sprites, normals);

        Sprite CreateSprite(Texture2D texture) {
            return Sprite.Create(texture,
                RectAccordingToScalingMode(configuration.scalingConfig.scalingModes,
                    configuration.sizingConfig.pixelSize),
                new Vector2(.5f, .5f));
        }
    }

    (Texture2D, Texture2D) GenerateTexture(int frame, ConfigurationAsset configuration)
    {
        var tex = noiseGeneration.GetNoise(frame, configuration.noiseConfig, configuration.sizingConfig.pixelSize);
        falloff.ApplyFalloff(ref tex, configuration.falloffConfig);
        symmetry.AttemptToApplySymmetry(ref tex, frame, configuration.symmetryConfig);
        
        backgroundColor = Color.black;
        var outlineColor = Color.black;

        var normalMap = tex;
        
        if (configuration.colorConfig.colorEnabled) {
            (backgroundColor, outlineColor) = recoloring.Recolor(ref tex, frame, 
                configuration.colorConfig, configuration.backgroundColorConfig, configuration.outlineConfig);
        }

        if (configuration.shadingConfig.enableShading)
            Shading.Shade(ref tex, backgroundColor, configuration.shadingConfig.shadingIntensity, configuration.shadingConfig.shadingByColor);
        if (configuration.shadingConfig.enableHighlights)
            Shading.Highlight(ref tex, backgroundColor, configuration.shadingConfig.highlightIntensity, configuration.shadingConfig.highlightByColor);
        
        if (configuration.outlineConfig.outlineEnabled && !configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        
        if (configuration.scalingConfig.scalingModes != null)
            Scaling.ScaleTexture(ref tex, configuration.scalingConfig.scalingModes);
        if (configuration.colorConfig.colorEnabled && configuration.outlineConfig.outlineEnabled && configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);

        if (!configuration.cleanupConfig.allowPixelsOnEdgeOfSprite)
            cleanup.RemovePixelsAtEdgeOfSprite(ref tex, backgroundColor, outlineColor);
        if (configuration.cleanupConfig.chanceToDeleteLonePixels >= Random.value)
            cleanup.Despeckle(ref tex, backgroundColor, configuration.cleanupConfig.lonePixelEvaluationMode);

        tex.Apply();    
        tex.filterMode = configuration.scalingConfig.filterMode;
        tex.wrapMode = TextureWrapMode.Clamp;

        if (configuration.normalsConfig.enableNormals) {
            NormalGeneration.CreateNormalMap(ref normalMap, 1f);
        }

        normalGeneration.RotationEnabled = configuration.normalsConfig.rotatingLightEnabled;
        normalGeneration.CursorLightEnabled = configuration.normalsConfig.cursorFollowLightEnabled;
        normalGeneration.GlobalLightEnabled = configuration.normalsConfig.globalLightEnabled;
        
        if (configuration.scalingConfig.scalingModes != null)
            Scaling.ScaleTexture(ref normalMap, configuration.scalingConfig.scalingModes);
        normalMap.wrapMode = TextureWrapMode.Clamp;
        normalMap.filterMode = configuration.normalsConfig.filterMode;
        normalMap.Apply();

        return (tex, normalMap);
    }

    Rect RectAccordingToScalingMode(ScalingMode[] scalingModes, int spritePixelSize) {
        var scalingFactor = Scaling.ScalingFactorMultiple(scalingModes);
        return new Rect(0, 0, spritePixelSize * scalingFactor, spritePixelSize * scalingFactor);
    }
}