using UnityEngine;

namespace DefaultNamespace {
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
    }
}