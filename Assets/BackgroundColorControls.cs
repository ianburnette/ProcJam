using UnityEngine;

public class BackgroundColorControls : WindowGuiBase
{
    public override string WindowLabel => "Background";

    int backgroundColorOverride;
    public Color grey;
    
    protected override void Window() {
        ToggleButton("Random Palette Color", ref controls.Configuration.backgroundColorConfig.randomPaletteColorForBackground);
        controls.Configuration.backgroundColorConfig.paletteColorIndexForBackground = Slider(
            "Palette Color", 
            controls.Configuration.backgroundColorConfig.paletteColorIndexForBackground, 
            1, 
            controls.Generation.Recoloring.uniqueColorsInTextures[controls.Configuration.colorConfig.paletteIndex].Count - 1);
        ToggleButton("Override Background Color", ref controls.Configuration.backgroundColorConfig.overrideBackgroundColor);
        backgroundColorOverride = MultiValueToggleButton("Background Color Override: ",
            backgroundColorOverride, new[] {"Black", "White", "Transparent", "Magenta"/*, "Grey"*/});
        switch (backgroundColorOverride) {
            case 0: controls.Configuration.backgroundColorConfig.backgroundColorOverride = Color.black; break;
            case 1: controls.Configuration.backgroundColorConfig.backgroundColorOverride = Color.white; break;
            case 2: controls.Configuration.backgroundColorConfig.backgroundColorOverride = Color.clear; break;
            case 3: controls.Configuration.backgroundColorConfig.backgroundColorOverride = Color.magenta; break;
            //case 4: controls.Configuration.backgroundColorConfig.backgroundColorOverride = grey; break;
        }
        base.Window();
    }
}
