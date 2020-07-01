using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteGeneration : MonoBehaviour {
     public static SpriteGeneration instance;
    
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

    void OnEnable() => instance = this;

    public (List<Sprite>, List<Sprite>, List<GeneratedTexture>) Generate(ConfigurationAsset configuration, 
        EvolutionConfig evolutionConfig = null) {
        var sprites = new List<Sprite>();
        var normals = new List<Sprite>();
        var generatedTextures = new List<GeneratedTexture>();
        for (var i = 0; i < configuration.animationConfig.animationFrameCount; i++) {
            var generatedTexture = GenerateTexture(i, configuration, evolutionConfig);
            var diffuseSprite = CreateSprite(generatedTexture.texture, configuration.scalingConfig.scalingModes, configuration.sizingConfig.pixelSize);
            var normalSprite = CreateSprite(generatedTexture.normal, configuration.scalingConfig.scalingModes, configuration.sizingConfig.pixelSize);
            sprites.Add(diffuseSprite);
            normals.Add(normalSprite);
            generatedTextures.Add(generatedTexture);
        }
        return (sprites, normals, generatedTextures);
    }
    
    public static Sprite CreateSprite(Texture2D texture, ScalingMode[] scalingModes, int pixelSize) =>
        Sprite.Create(texture,
            RectAccordingToScalingMode(scalingModes, pixelSize),
            new Vector2(.5f, .5f));

    GeneratedTexture GenerateTexture(
        int frame, 
        ConfigurationAsset configuration, 
        EvolutionConfig evolutionConfig = null) {
        
        var generatedTexture = new GeneratedTexture();
        
        // find the position within the noise to start sampling
        Vector2 origin;
        if (evolutionConfig == null)
            origin = noiseGeneration.GetOrigin(frame, configuration.noiseConfig);
        else if (evolutionConfig.evolutionType == EvolutionType.noiseOffset)
            origin = noiseGeneration.GetOriginWithOffset(frame, configuration.noiseConfig, evolutionConfig);
        else// if (evolutionConfig.evolutionType == EvolutionType.color)
            origin = evolutionConfig.evolutionSource[frame].origin;
        
        
        generatedTexture.origin = origin;
        var tex = noiseGeneration.GetNoise(configuration.noiseConfig, configuration.sizingConfig.pixelSize, origin);
        falloff.ApplyFalloff(ref tex, configuration.falloffConfig);
        if (evolutionConfig != null && evolutionConfig.inheritedSymmetryConfig.inherited) {
            symmetry.AttemptToApplySymmetry(ref tex, frame, configuration.symmetryConfig, true, ref evolutionConfig.inheritedSymmetryConfig.outcome);
            generatedTexture.symmetryOutcome = evolutionConfig.inheritedSymmetryConfig.outcome;
        } else {
            var symmetryOutcome = new SymmetryOutcome();
            symmetry.AttemptToApplySymmetry(ref tex, frame, configuration.symmetryConfig, false, ref symmetryOutcome);
            generatedTexture.symmetryOutcome = symmetryOutcome;
        }
        
        var normalMap = tex;

        ColorOutcome colorOutcome;
        if (evolutionConfig == null) {
            colorOutcome = InGameControls.instance.Configuration.colorConfig.colorLocked ? 
                    InGameControls.instance.Configuration.colorConfig.lockedColorTextures[frame].colorOutcome : 
                    ColorOutcome.None;
        }
        else {
            switch (evolutionConfig.evolutionType) {
                case EvolutionType.noiseOffset:
                    colorOutcome = evolutionConfig.evolutionSource[frame].colorOutcome;
                    break;
                case EvolutionType.color:
                    colorOutcome = ColorOutcome.None;
                    break;
                default:
                    colorOutcome = ColorOutcome.None;
                    break;
            }
        }
        
        if (configuration.colorConfig.colorEnabled) {
            colorOutcome = recoloring.Recolor(ref tex, frame,
                configuration.colorConfig, configuration.backgroundColorConfig, configuration.outlineConfig,
                colorOutcome);
        }

        generatedTexture.colorOutcome = colorOutcome;

        if (configuration.shadingConfig.enableShading)
            Shading.Shade(ref tex, colorOutcome.backgroundColor, configuration.shadingConfig.shadingIntensity, configuration.shadingConfig.shadingByColor);
        if (configuration.shadingConfig.enableHighlights)
            Shading.Highlight(ref tex, colorOutcome.backgroundColor, configuration.shadingConfig.highlightIntensity, configuration.shadingConfig.highlightByColor);
        
        if (configuration.outlineConfig.outlineEnabled && !configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, colorOutcome.backgroundColor, colorOutcome.outlineColor);
        
        if (configuration.scalingConfig.scalingModes != null)
            Scaling.ScaleTexture(ref tex, configuration.scalingConfig.scalingModes);
        if (configuration.colorConfig.colorEnabled && configuration.outlineConfig.outlineEnabled && configuration.outlineConfig.applyOutlineAfterScaling)
            outline.OutlineTexture(ref tex, colorOutcome.backgroundColor, colorOutcome.outlineColor);

        if (!configuration.cleanupConfig.allowPixelsOnEdgeOfSprite)
            cleanup.RemovePixelsAtEdgeOfSprite(ref tex, colorOutcome.backgroundColor, colorOutcome.outlineColor);
        if (configuration.cleanupConfig.chanceToDeleteLonePixels >= Random.value)
            cleanup.Despeckle(ref tex, colorOutcome.backgroundColor, configuration.cleanupConfig.lonePixelEvaluationMode);

        tex.Apply();    
        tex.filterMode = configuration.scalingConfig.filterMode;
        tex.wrapMode = TextureWrapMode.Clamp;
        generatedTexture.texture = tex;

        if (configuration.normalsConfig.enableNormals) {
            NormalGeneration.CreateNormalMap(ref normalMap, configuration.normalsConfig.normalStrength);
        }

        normalGeneration.RotationEnabled = configuration.normalsConfig.rotatingLightEnabled;
        normalGeneration.CursorLightEnabled = configuration.normalsConfig.cursorFollowLightEnabled;
        normalGeneration.GlobalLightEnabled = configuration.normalsConfig.globalLightEnabled;
        
        if (configuration.scalingConfig.scalingModes != null)
            Scaling.ScaleTexture(ref normalMap, configuration.scalingConfig.scalingModes);
        normalMap.wrapMode = TextureWrapMode.Clamp;
        normalMap.filterMode = configuration.normalsConfig.filterMode;
        normalMap.Apply();
        generatedTexture.normal = normalMap;
        generatedTexture.scalingModes = configuration.scalingConfig.scalingModes;
        generatedTexture.filterMode = configuration.scalingConfig.filterMode;

        return generatedTexture;
    }

    static Rect RectAccordingToScalingMode(ScalingMode[] scalingModes, int spritePixelSize) {
        var scalingFactor = Scaling.ScalingFactorMultiple(scalingModes);
        return new Rect(0, 0, spritePixelSize * scalingFactor, spritePixelSize * scalingFactor);
    }

    public GeneratedVoxelModel GenerateVoxelModel(ConfigurationAsset configuration, EvolutionConfig evolutionConfig = null) {
        var generatedVoxel = new GeneratedVoxelModel();

        // fill a 3D texture with noise, offsetting the perlin origin a bit by each layer
        generatedVoxel.modelData = noiseGeneration.GetNoise3D(
            configuration.noiseConfig,
            configuration.sizingConfig.pixelSize, 
            ref generatedVoxel.origin);
        
        // apply falloff
        falloff.ApplyFalloff(ref generatedVoxel.modelData, configuration.falloffConfig);

        // apply symmetry
        var symmetryOutcome = new SymmetryOutcome3D();
        symmetry.AttemptToApplySymmetry(ref generatedVoxel, configuration.symmetryConfig3D, ref symmetryOutcome);
        
        return generatedVoxel;
    }
}