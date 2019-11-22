public class ColorControls : WindowGuiBase {
    public override string WindowLabel => "Color";

    protected override void Window() {
        ToggleButton("Color Enabled", ref controls.Configuration.colorConfig.colorEnabled);
        ToggleButton("Use Palettes", ref controls.Configuration.colorConfig.usePaletteColors);
        if (controls.Configuration.colorConfig.usePaletteColors) {
            controls.Configuration.colorConfig.paletteIndex = 
                Slider("Palette Index", controls.Configuration.colorConfig.paletteIndex, 
                    0, controls.Generation.Recoloring.palettes.Length - 1);
            Label("Current Palette: " +
                  $"{controls.Generation.Recoloring.palettes[controls.Configuration.colorConfig.paletteIndex].name}");
            controls.Configuration.colorConfig.colorCountPerSprite = Slider(
                "Colors per sprite",
                controls.Configuration.colorConfig.colorCountPerSprite, 
                1, controls.Generation.Recoloring.uniqueColorsInTextures[controls.Configuration.colorConfig.paletteIndex].Count);
        } else {
            Label("Palette Index: N/A");
            Label("Current Palette: Full Random RGB");
            controls.Configuration.colorConfig.colorCountPerSprite =
                IntField("Colors per sprite", controls.Configuration.colorConfig.colorCountPerSprite);

        }
        base.Window();
    }
}
