using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class NormalGeneration : MonoBehaviour
{
    public Animator rotatingLightAnim;
    public Transform spriteParent;
    public Light2D rotatingLight, cursorLight;
    public Controls controls;
    List<FrameAnimation> animations;
    
    List<FrameAnimation> Animations => spriteParent.GetComponentsInChildren<FrameAnimation>().ToList();
    int filterMode;
    
    public static void CreateNormalMap(ref Texture2D source, float strength) {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);
        var normalTexture = new Texture2D (source.width, source.height, TextureFormat.ARGB32, true);

        for (var y=0; y<normalTexture.height; y++) 
        {
            for (var x=0; x<normalTexture.width; x++) 
            {
                var xLeft = source.GetPixel(x-1,y).grayscale*strength;
                var xRight = source.GetPixel(x+1,y).grayscale*strength;
                var yUp = source.GetPixel(x,y-1).grayscale*strength;
                var yDown = source.GetPixel(x,y+1).grayscale*strength;
                var xDelta = ((xLeft-xRight)+1)*0.5f;
                var yDelta = ((yUp-yDown)+1)*0.5f;
                normalTexture.SetPixel(x,y,new Color(xDelta,yDelta,1.0f,yDelta));
            }
        }
        source = normalTexture;
    }

    public float RotationSpeed {
        get => rotatingLightAnim.speed;
        set => rotatingLightAnim.speed = value;
    }

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
        //   if (controls != null)
        //       controls.Configuration.normalsConfig.globalLightEnabled = value;
        //   rotatingLight.enabled = value;
        }
    }

}
