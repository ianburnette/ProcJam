using UnityEngine;

public class ScalingControls : WindowGuiBase
{    
    public override string WindowLabel => "Scaling";

    int filterMode;
    protected override void Window() {
        var newScalingModeLength = controls.Configuration.scalingConfig.scalingModes.Length;
        newScalingModeLength = Slider("Scaling Passes", newScalingModeLength, 1, 6);
        controls.Configuration.scalingConfig.ResizeScalingMode(newScalingModeLength);
        for (int i = 0; i < newScalingModeLength; i++) {
            controls.Configuration.scalingConfig.scalingModes[i] = (ScalingMode)MultiValueToggleButton(
                $"Scaling Mode {i + 1}: ", 
                (int)controls.Configuration.scalingConfig.scalingModes[i], 
                new[]{"None", "x2", "x4", "x10", "Eagle 2", "Eagle 3"});
        }
        Label("Please note: the x2, x4, and x10 scaling modes are WIP and only take effect when the spritesheet is exported.");
        filterMode = MultiValueToggleButton("Filter Mode: ", filterMode, new[] {"Point", "Bilinear", "Trilinear"});
        controls.Configuration.scalingConfig.filterMode = filterMode == 0
            ? FilterMode.Point : filterMode == 1 ? FilterMode.Bilinear : FilterMode.Trilinear;
        base.Window();
    }
}
