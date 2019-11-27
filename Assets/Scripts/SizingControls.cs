using Nothke.ProtoGUI;
using UnityEngine;
using UnityEngine.UI;

public class SizingControls : WindowGuiBase {
    
    public override string WindowLabel => "Sizing";

    bool toggleOn;
    float floatInput = 0;
    string stringInput = "";
    float slider1Value;
    int slider2Value;
    float slider3Value;
    bool showFoldout;
    public CanvasScaler canvasScaler;
    Vector2 baseReferenceResolution = new Vector2(2048, 2048);
    float scale = .5f;
    int fontSize = 12;
    public GUISkin protoGuiSkin;
    public ToolbarGUI toolbarGui;

    int FontSize {
        set {
            if (protoGuiSkin.button.fontSize == value && protoGuiSkin.label.fontSize == value &&
                protoGuiSkin.window.fontSize == value)
                return;
            //refresh the window height - probably a better way of doing this
            windowRect.height = 0;
            toolbarGui.sizeDirty = true;
            //set the size
            protoGuiSkin.button.fontSize = protoGuiSkin.label.fontSize = protoGuiSkin.window.fontSize = value;
            fontSize = value;
        }
    }

    protected override void Window()
    {
        controls.Configuration.sizingConfig.spacing = Slider("Spacing", controls.Configuration.sizingConfig.spacing, 0, 64);
        controls.Configuration.sizingConfig.imageGridSize = Slider("Grid Size", controls.Configuration.sizingConfig.imageGridSize, 1, 64);
        controls.Configuration.sizingConfig.pixelSize = Slider("Sprite Pixel Size", controls.Configuration.sizingConfig.pixelSize, 4, 128);

        Label("Window Controls");
        scale = Slider("Canvas Size", (float) System.Math.Round(scale, 2), .5f, 4f);
        canvasScaler.referenceResolution = baseReferenceResolution / scale;
        FontSize = Slider("Font Size", fontSize, 6, 20);

        protoGuiSkin.button.fontSize = protoGuiSkin.label.fontSize = protoGuiSkin.window.fontSize = fontSize;
        
        base.Window();
   //  ToggleButton("Toggle", ref toggleOn);

   //  Foldout("Foldout", ref showFoldout);

   //  if (showFoldout)
   //  {
   //      Label("Some things in the foldout:");

   //      // Inputs
   //      floatInput = FloatField("Input fl", floatInput);
   //      LabelField("Input str", ref stringInput);
   //  }

   //  // Float and int sliders:
   //  slider1Value = Slider("Float", slider1Value, 0.0f, 1.0f, "F2");

   //  // A "notched" float slider that will step by stepSize
   //  slider3Value = Slider("Notched", slider3Value, 0, 2, 0.2f);
    }
}
