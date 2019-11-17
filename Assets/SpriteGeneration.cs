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
    [SerializeField] Cleanup cleanup;

    public Color backgroundColor;
    
    public List<Sprite> Generate(Configuration configuration) {
        var sprites = new List<Sprite>();
        for (int i = 0; i < configuration.animationConfig.animationFrameCount; i++) {
            sprites.Add(Sprite.Create(GenerateTexture(i, configuration), RectAccordingToScalingMode(configuration), new Vector2(.5f, .5f)));
        }
        return sprites;
    }

    Texture2D GenerateTexture(int frame, Configuration configuration)
    {
        var tex = noiseGeneration.GetNoise(frame, configuration);
        falloff.ApplyFalloff(ref tex, configuration);
        symmetry.AttemptToApplySymmetry(ref tex, frame, configuration);
        
        backgroundColor = Color.black;
        var outlineColor = Color.black;

        if (configuration.spriteColorConfig.colorEnabled) {
            (backgroundColor, outlineColor) = recoloring.Recolor(ref tex, frame, configuration);
        }

        if (!configuration.cleanupConfig.allowPixelsOnEdgeOfSprite)
            cleanup.RemovePixelsAtEdgeOfSprite(ref tex, backgroundColor);
        
        if (configuration.cleanupConfig.chanceToDeleteLonePixels > Random.value)
            cleanup.Despeckle(ref tex, backgroundColor, configuration.cleanupConfig.lonePixelEvaluationMode);
        
        if (configuration.outlineConfig.outlineEnabled && !configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        
        if (configuration.scalingConfig.scalingModes != null)
            Scaling.ScaleTexture(ref tex, configuration.scalingConfig.scalingModes);
        if (configuration.spriteColorConfig.colorEnabled && configuration.outlineConfig.outlineEnabled && configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        tex.filterMode = configuration.scalingConfig.filterMode;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();    
        return tex;
    }

    Rect RectAccordingToScalingMode(Configuration configuration) {
        var scalingFactor = Scaling.ScalingFactorMultiple(configuration.scalingConfig.scalingModes);
        return new Rect(0, 0, configuration.spriteConfig.spritePixelSize * scalingFactor, configuration.spriteConfig.spritePixelSize * scalingFactor);
    }
}
