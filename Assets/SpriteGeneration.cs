using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

 public class SpriteGeneration : MonoBehaviour
{
    [Header("Filtering")]
    [SerializeField] Recoloring recoloring;
    [SerializeField] NoiseGeneration noiseGeneration;
    [SerializeField] Symmetry symmetry;
    [SerializeField] Falloff falloff;
    [SerializeField] Outline outline;
    [SerializeField] Cleanup cleanup;

    public Color backgroundColor;
    
    public List<Sprite> Generate(ConfigurationAsset configuration) {
        var sprites = new List<Sprite>();
        for (int i = 0; i < configuration.animationConfig.animationFrameCount; i++) {
            sprites.Add(
                Sprite.Create(GenerateTexture(i, configuration), 
                RectAccordingToScalingMode(configuration.scalingConfig.scalingModes, configuration.spriteConfig.pixelSize), 
                new Vector2(.5f, .5f)));
        }
        return sprites;
    }

    Texture2D GenerateTexture(int frame, ConfigurationAsset configuration)
    {
        var tex = noiseGeneration.GetNoise(frame, configuration.noiseConfig, configuration.spriteConfig.pixelSize);
        falloff.ApplyFalloff(ref tex, configuration.falloffConfig);
        symmetry.AttemptToApplySymmetry(ref tex, frame, configuration.symmetryConfig);
        
        backgroundColor = Color.black;
        var outlineColor = Color.black;

        if (configuration.colorConfig.colorEnabled) {
            (backgroundColor, outlineColor) = recoloring.Recolor(ref tex, frame, 
                configuration.colorConfig, configuration.backgroundColorConfig, configuration.outlineConfig);
        }

        if (!configuration.cleanupConfig.allowPixelsOnEdgeOfSprite)
            cleanup.RemovePixelsAtEdgeOfSprite(ref tex, backgroundColor);
        
        if (configuration.cleanupConfig.chanceToDeleteLonePixels > Random.value)
            cleanup.Despeckle(ref tex, backgroundColor, configuration.cleanupConfig.lonePixelEvaluationMode);

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
        tex.filterMode = configuration.scalingConfig.filterMode;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();    
        return tex;
    }

    Rect RectAccordingToScalingMode(ScalingMode[] scalingModes, int spritePixelSize) {
        var scalingFactor = Scaling.ScalingFactorMultiple(scalingModes);
        return new Rect(0, 0, spritePixelSize * scalingFactor, spritePixelSize * scalingFactor);
    }
}