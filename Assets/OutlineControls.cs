using UnityEngine;

public class OutlineControls : WindowGuiBase
{
    public override string WindowLabel => "Outline";

    int outlineColorOverride;

    protected override void Window() {
        ToggleButton("Outline Enabled", ref controls.Configuration.outlineConfig.outlineEnabled);
        ToggleButton("Outline After Scaling", ref controls.Configuration.outlineConfig.applyOutlineAfterScaling);
        
        outlineColorOverride = MultiValueToggleButton("Outline Color: ",
            outlineColorOverride, new[] {"Black", "White"});
        switch (outlineColorOverride) {
            case 0: controls.Configuration.outlineConfig.outlineColorOverride = Color.black; break;
            case 1: controls.Configuration.outlineConfig.outlineColorOverride = Color.white; break;
        }

        base.Window();
    }
}
