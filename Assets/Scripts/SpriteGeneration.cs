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
            var diffuseSprite = CreateSprite(generatedTexture.texture);
            var normalSprite = CreateSprite(generatedTexture.normal);
            sprites.Add(diffuseSprite);
            normals.Add(normalSprite);
            generatedTextures.Add(generatedTexture);
        }
        return (sprites, normals, generatedTextures);

        Sprite CreateSprite(Texture2D texture) {
            return Sprite.Create(texture,
                RectAccordingToScalingMode(configuration.scalingConfig.scalingModes,
                    configuration.sizingConfig.pixelSize),
                new Vector2(.5f, .5f));
        }
    }

    GeneratedTexture GenerateTexture(
        int frame, 
        ConfigurationAsset configuration, 
        EvolutionConfig evolutionConfig = null) {
        
        var generatedTexture = new GeneratedTexture();
        
        Vector2 origin;
        if (evolutionConfig == null)
            origin = noiseGeneration.GetOrigin(frame, configuration.noiseConfig);
        else
            origin = noiseGeneration.GetOriginWithOffset(frame, configuration.noiseConfig, evolutionConfig);
        
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

        return generatedTexture;
    }

    Rect RectAccordingToScalingMode(ScalingMode[] scalingModes, int spritePixelSize) {
        var scalingFactor = Scaling.ScalingFactorMultiple(scalingModes);
        return new Rect(0, 0, spritePixelSize * scalingFactor, spritePixelSize * scalingFactor);
    }
}