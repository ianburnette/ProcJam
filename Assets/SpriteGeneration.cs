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
        for (int i = 0; i < configuration.animationFrameCount; i++) {
            sprites.Add(Sprite.Create(GenerateTexture(i, configuration), RectAccordingToScalingMode(configuration), new Vector2(.5f, .5f)));
        }
        return sprites;
    }

    Texture2D GenerateTexture(int frame, Configuration configuration)
    {
        var tex = noiseGeneration.GetNoise(frame, configuration);
        falloff.ApplyFalloff(ref tex, configuration.allowPixelsOnEdgeOfSprite, configuration);
        symmetry.AttemptToApplySymmetry(ref tex, frame, configuration);
        
        backgroundColor = Color.black;
        var outlineColor = Color.black;

        if (configuration.colorEnabled) {
                (backgroundColor, outlineColor) = recoloring.Recolor(ref tex, frame, configuration);
            if (configuration.outlineEnabled && !configuration.applyOutlineAfterScaling)
                outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
            if (configuration.chanceToDeleteLonePixels > Random.value)
                cleanup.Despeckle(ref tex, backgroundColor, configuration.lonePixelEvaluationMode);
        }

        if (configuration.scalingModes != null)
            Scaling.ScaleTexture(ref tex, configuration.scalingModes);
        if (configuration.colorEnabled && configuration.outlineEnabled && configuration.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        tex.filterMode = FilterMode.Point;
        tex.Apply();    
        return tex;
    }

    Rect RectAccordingToScalingMode(Configuration configuration) {
        var scalingFactor = Scaling.ScalingFactorMultiple(configuration.scalingModes);
        return new Rect(0, 0, configuration.spritePixelSize * scalingFactor, configuration.spritePixelSize * scalingFactor);
    }
}
