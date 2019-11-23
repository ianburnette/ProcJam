using Nothke.ProtoGUI;
using UnityEngine;

public class InGameControlsBase : MonoBehaviour {
    public Controls controls;
    public int windowRectWidth = 400;
    public int labelWidth = 90;
    public int sliderNumberWidth = 50;
    public bool draggable = true;
    public ToolbarGUI toolbarGui;
}

public abstract class WindowGuiBase : WindowGUI {
    InGameControlsBase inGameControlsBase;
    [HideInInspector] public Controls controls;
    
    protected virtual void Start() {
        inGameControlsBase = GetComponent<InGameControlsBase>();
        controls = inGameControlsBase.controls;
        labelWidth = inGameControlsBase.labelWidth;
        sliderNumberWidth = inGameControlsBase.sliderNumberWidth;
        draggable = inGameControlsBase.draggable;
    }

    protected override void Window() {
        if (Button("Generate")) controls.Generate();
        if (Button("Close")) enabled = false;
        windowRect.width = inGameControlsBase.windowRectWidth;
        windowRect.position = new Vector2(Screen.width - windowRect.size.x, inGameControlsBase.toolbarGui.toolbarHeight);
    }
}