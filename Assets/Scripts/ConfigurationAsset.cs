using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "SpriteConfiguration", order = 1)]
public class ConfigurationAsset : ScriptableObject {
    public SizingConfig sizingConfig;
    public NoiseConfig noiseConfig;
    public FalloffConfig falloffConfig;
    public ColorConfig colorConfig;
    public BackgroundColorConfig backgroundColorConfig;
    public OutlineConfig outlineConfig;
    public SymmetryConfig symmetryConfig;
    public ScalingConfig scalingConfig;
    public AnimationConfig animationConfig;
    public ShadingConfig shadingConfig;
    public CleanupConfig cleanupConfig;
    public NormalsConfig normalsConfig;

    public void SetValues(
        SizingConfig sizingConfig,
        NoiseConfig noiseConfig,
        FalloffConfig falloffConfig,
        ColorConfig colorConfig,
        BackgroundColorConfig backgroundColorConfig,
        OutlineConfig outlineConfig,
        SymmetryConfig symmetryConfig,
        ScalingConfig scalingConfig,
        AnimationConfig animationConfig,
        ShadingConfig shadingConfig,
        CleanupConfig cleanupConfig,
        NormalsConfig normalsConfig
        ) {
        this.sizingConfig = sizingConfig;
        this.noiseConfig = noiseConfig;
        this.falloffConfig = falloffConfig;
        this.colorConfig = colorConfig;
        this.backgroundColorConfig = backgroundColorConfig;
        this.outlineConfig = outlineConfig;
        this.symmetryConfig = symmetryConfig;
        this.scalingConfig = scalingConfig;
        this.animationConfig = animationConfig;
        this.shadingConfig = shadingConfig;
        this.cleanupConfig = cleanupConfig;
        this.normalsConfig = normalsConfig;
    }
}