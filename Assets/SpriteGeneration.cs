﻿using System.Collections.Generic;
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
        
        Scaling.ScaleTexture(ref tex, configuration.scalingMode);
        if (configuration.colorEnabled && configuration.outlineEnabled && configuration.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        tex.filterMode = FilterMode.Point;
        tex.Apply();    
        return tex;
    }

    Rect RectAccordingToScalingMode(Configuration configuration) {
        switch (configuration.scalingMode) {
            case ScalingMode.none:
                return new Rect(0, 0, configuration.spritePixelSize, configuration.spritePixelSize);
            case ScalingMode.x2:
                return new Rect(0, 0, configuration.spritePixelSize * 2, configuration.spritePixelSize * 2);
            case ScalingMode.x4:
                return new Rect(0, 0, configuration.spritePixelSize * 4, configuration.spritePixelSize * 4);
            case ScalingMode.eagle2:
                return new Rect(0, 0, configuration.spritePixelSize * 2, configuration.spritePixelSize * 2);
            case ScalingMode.eagle3:
                return new Rect(0, 0, configuration.spritePixelSize * 3, configuration.spritePixelSize * 3);
        }
        return new Rect(0, 0, configuration.spritePixelSize, configuration.spritePixelSize);
    }
}
