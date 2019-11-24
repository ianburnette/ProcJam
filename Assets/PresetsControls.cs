using System.Collections.Generic;
using System.Linq;

public class PresetsControls : WindowGuiBase {
    public override string WindowLabel => "Presets";

    ConfigurationAsset currentPreset;
    public List<ConfigurationAsset> allConfigurations;
    
    protected override void Window() {
        foreach (var configuration in allConfigurations.Where(configuration => Button(configuration.name)))
            currentPreset = configuration;
        controls.Configuration = currentPreset;
        if(currentPreset) Label($"Current: {currentPreset.name}");
        base.Window();
    }
}
