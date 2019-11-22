using UnityEngine;

public class NoiseControls : WindowGuiBase
{
    public override string WindowLabel => "Noise";

    protected override void Window() {
        Label("Noise Octaves: Scale");
        for (var index = 0; index < controls.Configuration.noiseConfig.octaves.Count; index++) {
            var octave = controls.Configuration.noiseConfig.octaves[index];
            octave.scale = Slider($"Scale {index + 1}", (float) System.Math.Round(octave.scale, 2), 1f, 50f);
        }
        
        Label("Noise Octaves: Frequency");
        for (var index = 0; index < controls.Configuration.noiseConfig.octaves.Count; index++) {
            var octave = controls.Configuration.noiseConfig.octaves[index];
            if (!controls.Configuration.noiseConfig.randomizeFrequency)
                octave.frequency = Slider("Frequency", (float) System.Math.Round(octave.frequency, 2), 0f, 1f);
            else
                Label("Frequency: [Randomized per sprite]");
        }

        Label("Animation-Related");
        controls.Configuration.noiseConfig.animationFrameNoiseOffset = Slider("Animation Frame Noise Offset",
            (float)System.Math.Round(controls.Configuration.noiseConfig.animationFrameNoiseOffset, 2), 0f, 1f);
        
        Label("Randomization");
        ToggleButton("Randomize Frequency", ref controls.Configuration.noiseConfig.randomizeFrequency);
        ToggleButton("Random Origin", ref controls.Configuration.noiseConfig.randomOrigin);
        
        Label("Perlin Mapping");
        controls.Configuration.noiseConfig.randomOriginBound = FloatField("Random Origin Bound", 
            controls.Configuration.noiseConfig.randomOriginBound);
        controls.Configuration.noiseConfig.manualOrigin.x = FloatField("Manual Origin X", controls.Configuration.noiseConfig.manualOrigin.x);
        controls.Configuration.noiseConfig.manualOrigin.y = FloatField("Manual Origin Y", controls.Configuration.noiseConfig.manualOrigin.y);
        
        base.Window();
    }
}
