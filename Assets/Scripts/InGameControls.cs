using Nothke.ProtoGUI;
using UnityEngine;

public class InGameControls : WindowGUI {

    [SerializeField] Controls controls;
    
    public override string WindowLabel => "Configuration";

    bool toggleOn;
    float floatInput = 0;
    string stringInput = "";
    float slider1Value;
    int slider2Value;
    float slider3Value;
    bool showFoldout;
    
    private void Start()
    {
        // Optional values you can override
        windowRect.width = 400; // Sets initial window width
        labelWidth = 90; // Sets label width of a Slider()
        sliderNumberWidth = 50; // Sets width of the number in a Slider()
        draggable = true;
    }

    protected override void Window()
    {
        Label("Here are some things:");
        if (Button("Generate")) controls.Generate();

        // Slider2Value = Slider("Spacing", Slider2Value, 0, 100);

        ToggleButton("Toggle", ref toggleOn);

        Foldout("Foldout", ref showFoldout);

        if (showFoldout)
        {
            Label("Some things in the foldout:");

            // Inputs
            floatInput = FloatField("Input fl", floatInput);
            LabelField("Input str", ref stringInput);
        }

        // Float and int sliders:
        slider1Value = Slider("Float", slider1Value, 0.0f, 1.0f, "F2");

        // A "notched" float slider that will step by stepSize
        slider3Value = Slider("Notched", slider3Value, 0, 2, 0.2f);
    }
}
