using Nothke.ProtoGUI;
using UnityEngine;

public class BasicControls : WindowGuiBase {
    public ToolbarGUI toolbarGui;
    public override string WindowLabel => "Operations";

    float myOffset = 30;

    protected override void Start() {
        base.Start();
    }

    protected override void Window() {
        windowRect.position = new Vector2(Screen.width - windowRect.size.x, toolbarGui.toolbarHeight);
        Label("Click the Generate button to generate a new set of random sprites.");
        Label("Click the toolbar bottoms to adjust the settings. " +
              "The changes you make will take affect the next time you click Generate.");
        Label("Click any sprite to save it.");
        Label("Click the Save Spritesheet button below to save the entire" +
              " spritesheet (including all animation frames).");
        Label("If you're not sure where to begin, play around in the prefabs tab a bit.");
        if (Button("Generate")) controls.Generate();
        if (Button("Reset")) controls.Reset();
        if (Button("Save Spritesheet")) controls.SaveSpritesheet();
        //if (Button("Save Config Preset")) controls.SaveAsPreset();
        if (Button("Close")) enabled = false;
        Label("Made by Ian Burnette.");
        Label("UI made with ProtoGUI by nothke.");
        windowRect.position = new Vector2(Screen.width - windowRect.size.x - myOffset, 
            Screen.height - windowRect.height - myOffset);
    }
}
