using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "SpriteConfiguration", order = 1)]
public class ConfigurationAsset : ScriptableObject {
    public Layout layout;
    public SpriteConfig spriteConfig;
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

    public void SetValues(
        Layout layout,
        SpriteConfig spriteConfig,
        NoiseConfig noiseConfig,
        FalloffConfig falloffConfig,
        ColorConfig colorConfig,
        BackgroundColorConfig backgroundColorConfig,
        OutlineConfig outlineConfig,
        SymmetryConfig symmetryConfig,
        ScalingConfig scalingConfig,
        AnimationConfig animationConfig,
        ShadingConfig shadingConfig,
        CleanupConfig cleanupConfig
    ) {
        this.layout = layout;
        this.spriteConfig = spriteConfig;
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
    }
}