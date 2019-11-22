public class ScalingControls : WindowGuiBase
{    
    public override string WindowLabel => "Scaling";

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
        base.Window();
    }
}
