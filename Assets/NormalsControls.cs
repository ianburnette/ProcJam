using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class NormalsControls : WindowGuiBase {
    public override string WindowLabel => "Normals";

    public Animator rotatingLightAnim;
    public Transform spriteParent;
    public Light2D rotatingLight, cursorLight;

    List<FrameAnimation> animations;
    
    List<FrameAnimation> Animations => spriteParent.GetComponentsInChildren<FrameAnimation>().ToList();
    int filterMode;

    public bool DisableNormalsDisplay {
        get => controls.Configuration.normalsConfig.disableNormalsDisplay;
        set {
            controls.Configuration.normalsConfig.disableNormalsDisplay = value;
            foreach (var frameAnimation in Animations)
                frameAnimation.disableNormalsDisplay = value;
        }
    }

    public bool ViewNormalsOnly {
        get => controls.Configuration.normalsConfig.viewNormalsOnly;
        set {
            controls.Configuration.normalsConfig.viewNormalsOnly = value;
            foreach (var frameAnimation in Animations) 
                frameAnimation.normalsOnly = value;
        }
    }

    public bool RotationEnabled {
        get => controls.Configuration.normalsConfig.rotatingLightEnabled;
        set {
            if (controls != null)
                controls.Configuration.normalsConfig.rotatingLightEnabled = value;
            rotatingLightAnim.enabled = value;
            rotatingLight.lightType = value ? Light2D.LightType.Point : Light2D.LightType.Global;
        }
    }

    public bool CursorLightEnabled {
        get => controls.Configuration.normalsConfig.cursorFollowLightEnabled;
        set {
            if (controls != null)
                controls.Configuration.normalsConfig.cursorFollowLightEnabled = value;
            cursorLight.enabled = value;
        }
    }
    
    public bool GlobalLightEnabled {
        get => controls.Configuration.normalsConfig.globalLightEnabled;
        set {
            if (controls != null)
                controls.Configuration.normalsConfig.globalLightEnabled = value;
            rotatingLight.enabled = value;
        }
    }

    protected override void Window() {
        ToggleButton("Enable Normals", ref controls.Configuration.normalsConfig.enableNormals);
        controls.Configuration.normalsConfig.normalStrength = Slider("Normal Strength", 
            (float) System.Math.Round(controls.Configuration.normalsConfig.normalStrength, 2), 0f, 1f);
        filterMode = MultiValueToggleButton("Filter Mode: ", filterMode, new[] {"Point", "Bilinear", "Trilinear"});
        controls.Configuration.normalsConfig.filterMode = filterMode == 0
            ? FilterMode.Point : filterMode == 1 ? FilterMode.Bilinear : FilterMode.Trilinear;
        
        
        Label("Debug Options");
        var normalsOnly = ViewNormalsOnly;
        ToggleButton("View Normals Only", ref normalsOnly);
        ViewNormalsOnly = normalsOnly;
        var normalsDisplay = DisableNormalsDisplay;
        ToggleButton("Disable Normals Display", ref normalsDisplay);
        DisableNormalsDisplay = normalsDisplay;

        var globalLightEnabled = GlobalLightEnabled;
        ToggleButton("Enable Global Light", ref globalLightEnabled);
        GlobalLightEnabled = globalLightEnabled;
        
        var rotationEnabled = RotationEnabled;
        ToggleButton("Enable Rotating Light", ref rotationEnabled);
        RotationEnabled = rotationEnabled;
        rotatingLightAnim.speed = Slider("Light Rotation Speed", 
            (float) System.Math.Round(rotatingLightAnim.speed, 2), 0f, 2f);

        var cursorLightEnabled = CursorLightEnabled;
        ToggleButton("Enable Cursor Follow Light", ref cursorLightEnabled);
        CursorLightEnabled = cursorLightEnabled;
        
        base.Window();
    }
}
