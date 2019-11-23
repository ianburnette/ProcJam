public class AnimationControls : WindowGuiBase
{
    public override string WindowLabel => "Animation";

    int pingPong;
    protected override void Window() {
        controls.Configuration.animationConfig.animationFrameCount = Slider("Frame Count", 
            controls.Configuration.animationConfig.animationFrameCount, 1, 8);
        controls.Configuration.animationConfig.timeBetweenFrames = Slider("Time Between Frames", (float) System.Math.Round(controls.Configuration.animationConfig.timeBetweenFrames, 2), .01f, 1f);
        
        //ToggleButton("Animation Mode:", ref pingPong);
        
        pingPong = MultiValueToggleButton("Animation Mode: ",
            pingPong, new[] {"Ping Pong", "Loop"});
        controls.Configuration.animationConfig.animationMode = pingPong == 0 ? AnimationMode.pingPong : AnimationMode.loop;
        Label("Please be aware: every time you add a frame, you're doubling the amount of work the generator performs. If you " +
              "have large images with many frames, generation may take a very long time.");

        base.Window();
    }
}
