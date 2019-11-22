using UnityEngine;

public class BasicControls : WindowGuiBase
{
    public override string WindowLabel => "Operations";

    protected override void Window() {
        Label("Click any sprite to save it.");
        if (Button("Generate")) controls.Generate();
        if (Button("Reset")) controls.Reset();
        if (Button("Save Spritesheet")) controls.SaveSpritesheet();
        if (Button("Save Config Preset")) controls.SaveAsPreset();
        if (Button("Close")) enabled = false;
        windowRect.position = new Vector2(Screen.width - windowRect.size.x, 0);
    }
}
