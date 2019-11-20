public class SizingControls : WindowGuiBase {
    
    public override string WindowLabel => "Sizing";

    bool toggleOn;
    float floatInput = 0;
    string stringInput = "";
    float slider1Value;
    int slider2Value;
    float slider3Value;
    bool showFoldout;

    protected override void Window()
    {
        controls.Configuration.sizingConfig.spacing = Slider("Spacing", controls.Configuration.sizingConfig.spacing, 0, 64);
        controls.Configuration.sizingConfig.imageGridSize = Slider("Grid Size", controls.Configuration.sizingConfig.imageGridSize, 1, 64);
        controls.Configuration.sizingConfig.pixelSize = Slider("Sprite Pixel Size", controls.Configuration.sizingConfig.pixelSize, 4, 128);
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
