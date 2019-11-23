public class SymmetryControls : WindowGuiBase {
    public override string WindowLabel => "Symmetry";

    protected override void Window() {
        ToggleButton("Allow Multiple Symmetries", ref controls.Configuration.symmetryConfig.allowMultipleSymmetryTypes);
        ToggleButton("Enforce Some Type of Symmetry", ref controls.Configuration.symmetryConfig.enforceSomeTypeOfSymmetry);

        Label("The \"Quarter Chance\" property controls the likelihood that the given type of symmetry will only " +
              "be applied to a quarter of the image, resulting in more organic, but partially symmetrical, forms.");

        Label("Horizontal");
        controls.Configuration.symmetryConfig.horizontalSymmetryChance = 
            Slider("Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.horizontalSymmetryChance, 2), 0f, 1f);
        controls.Configuration.symmetryConfig.quarterHorizontalSymmetryChance = 
            Slider("Quarter Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.quarterHorizontalSymmetryChance, 2), 0f, 1f);

        Label("Vertical");
        controls.Configuration.symmetryConfig.verticalSymmetryChance = 
            Slider("Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.verticalSymmetryChance, 2), 0f, 1f);
        controls.Configuration.symmetryConfig.quarterVerticalSymmetryChance = 
            Slider("Quarter Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.quarterVerticalSymmetryChance, 2), 0f, 1f);
       
        Label("Forward Diagonal, like this: /");
        controls.Configuration.symmetryConfig.forwardDiagonalSymmetryChance = 
            Slider("Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.forwardDiagonalSymmetryChance, 2), 0f, 1f);
        controls.Configuration.symmetryConfig.quarterForwardDiagonalSymmetryChance = 
            Slider("Quarter Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.quarterForwardDiagonalSymmetryChance, 2), 0f, 1f);
        
        Label("Backward Diagonal, like this: \\");
        controls.Configuration.symmetryConfig.backwardDiagonalSymmetryChance = 
            Slider("Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.backwardDiagonalSymmetryChance, 2), 0f, 1f);
        controls.Configuration.symmetryConfig.quarterBackwardDiagonalSymmetryChance = 
            Slider("Quarter Chance: ", (float) System.Math.Round(controls.Configuration.symmetryConfig.quarterBackwardDiagonalSymmetryChance, 2), 0f, 1f);
        base.Window();
    }
}
