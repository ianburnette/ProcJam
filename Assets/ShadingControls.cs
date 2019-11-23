public class ShadingControls : WindowGuiBase
{
    public override string WindowLabel => "Shading";

    protected override void Window() {
        ToggleButton("Enable Shading", ref controls.Configuration.shadingConfig.enableShading);
        ToggleButton("Shading by Color", ref controls.Configuration.shadingConfig.shadingByColor);
        controls.Configuration.shadingConfig.shadingIntensity = Slider("Shading Intensity", 
            (float) System.Math.Round(controls.Configuration.shadingConfig.shadingIntensity, 2), 0f, 1f);
        
        ToggleButton("Enable Highlights", ref controls.Configuration.shadingConfig.enableHighlights);
        ToggleButton("Shading by Color", ref controls.Configuration.shadingConfig.highlightByColor);
        controls.Configuration.shadingConfig.highlightIntensity = Slider("Highlight Intensity", 
            (float) System.Math.Round(controls.Configuration.shadingConfig.highlightIntensity, 2), 0f, 1f);

        Label("The \"... by Color\" parameters control how each pixel decides how it will be shaded/highlighted. If " +
              "left unchecked, the pixel will only be sensitive to the background color. If checked, it's sensitive " +
              "to other colors in the sprite. Unchecked, you'll get rounder, smoother-seeming sprites. Checked, you'll " +
              "get intricate-feeling sprites.");
        
        base.Window();
    }
}
