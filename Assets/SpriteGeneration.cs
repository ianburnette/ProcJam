﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGeneration : MonoBehaviour
{
    [SerializeField] int imageSize = 32;

    [Header("Filtering")]
    [SerializeField] Recoloring recoloring;
    [SerializeField] NoiseGeneration noiseGeneration;
    [SerializeField] Symmetry symmetry;
    [SerializeField] Falloff falloff;
    [SerializeField] Outline outline;
    
    public List<Sprite> Generate(int animationFrameCount) {
        var sprites = new List<Sprite>();
        for (int i = 0; i < animationFrameCount; i++) {
            sprites.Add(Sprite.Create(GenerateTexture(i), new Rect(0, 0, imageSize, imageSize), new Vector2(.5f, .5f)));
        }
        return sprites;
    }

    Texture2D GenerateTexture(int frame)
    {
        var tex = noiseGeneration.GetNoise(imageSize, frame);
        tex.filterMode = FilterMode.Point;
        falloff.ApplyFalloff(ref tex);
        symmetry.AttemptToApplySymmetry(ref tex, frame);
        var (backgroundColor, outlineColor) = recoloring.Recolor(ref tex, frame);
        outline.OutlineTexture(ref tex, backgroundColor, outlineColor);
        tex.Apply();    
        return tex;
    }

    Texture2D EmptyTexture() => new Texture2D(imageSize, imageSize);
}
