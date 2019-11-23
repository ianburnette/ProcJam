public class CleanupControls : WindowGuiBase
{
    int evaluation;

    public override string WindowLabel => "Cleanup";

    protected override void Window() {
        ToggleButton("Allow Pixels on Edge", ref controls.Configuration.cleanupConfig.allowPixelsOnEdgeOfSprite);
        evaluation = MultiValueToggleButton("Lone Pixel Evaluation: ",
            evaluation, new[] {"Include Diagonals", "Cardinal Directions Only"});
        controls.Configuration.cleanupConfig.lonePixelEvaluationMode = evaluation == 0 ? LonePixelEvaluationMode.IncludeDiagonals : LonePixelEvaluationMode.CardinalDirectionsOnly;
        controls.Configuration.cleanupConfig.chanceToDeleteLonePixels = 
            Slider("Chance to Delete Lone Pixel", (float) System.Math.Round(controls.Configuration.cleanupConfig.chanceToDeleteLonePixels, 2), .01f, 1f);
        base.Window();
    }
}
