using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[ExecuteInEditMode]
public class FrameAnimation : MonoBehaviour {
    [SerializeField] Image image;
    [SerializeField] List<Sprite> diffuseFrames = new List<Sprite>();
    [SerializeField] List<Sprite> normalFrames = new List<Sprite>();
    [SerializeField] List<GeneratedTexture> generatedTextures = new List<GeneratedTexture>();
    [SerializeField] float frameTime;

    int currentFrameIndex;
    bool ascending = true;

    #if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void ImageDownloader(string str, string fn);
    #endif
    
    public List<Sprite> DiffuseFrames {
        get => diffuseFrames;
        set {
            diffuseFrames = value;
            CancelInvoke();
            InvokeRepeating(nameof(Animate), FrameTime, FrameTime);
        }
    }    
    
    public List<Sprite> NormalFrames {
        get => normalFrames;
        set {
            normalFrames = value;
        }
    }    
    
    public List<GeneratedTexture> GeneratedTextures {
        get => generatedTextures;
        set {
            generatedTextures = value;
        }
    }

    public float FrameTime {
        get => frameTime;
        set => frameTime = value;
    }

    public Image ImageComponent {
        get => image;
        set => image = value;
    }

    public AnimationMode animationMode;
    Material myMaterial;
    public bool enableNormals;
    public bool disableNormalsDisplay;
    public bool normalsOnly;

    void OnEnable() {
        ImageComponent.material = Instantiate(ImageComponent.material);
        myMaterial = ImageComponent.material;
    }

    void Animate() {
        if (ascending && currentFrameIndex + 1 < diffuseFrames.Count)
            currentFrameIndex++;
        else if (ascending && currentFrameIndex + 1 >= diffuseFrames.Count) {
            if (animationMode == AnimationMode.pingPong) {
                ascending = false;
                currentFrameIndex--;
            }
            else if (animationMode == AnimationMode.loop) {
                currentFrameIndex = 0;
            }
        }
        else if (!ascending && currentFrameIndex - 1 >= 0) {
            currentFrameIndex--;
        }
        else if (!ascending && currentFrameIndex - 1 < 0) {
            ascending = true;
            currentFrameIndex++;
        }
        
        if (currentFrameIndex < 0)
            currentFrameIndex = 0;

        if (diffuseFrames.Count >= currentFrameIndex + 1) {
            if (!normalsOnly) 
                ImageComponent.sprite = diffuseFrames[currentFrameIndex];
            else if (normalFrames.Count >= currentFrameIndex + 1)
                ImageComponent.sprite = normalFrames[currentFrameIndex];
            if(enableNormals && !disableNormalsDisplay)
                myMaterial.SetTexture("_NormalMap", normalFrames[currentFrameIndex].texture);
            if (disableNormalsDisplay)
                myMaterial.SetTexture("_NormalMap", null);
        }
    }

    public void Clicked() {
        InGameControls.instance.OpenSpritePanel(this);
    }

    public void EvolveShape() {
        Controls.instance.Evolve(generatedTextures, EvolutionType.noiseOffset);
    }
    
    public void Export() {
        var generatedTexture = new Texture2D(diffuseFrames[0].texture.width * diffuseFrames.Count, diffuseFrames[0].texture.height);
        for (var index = 0; index < diffuseFrames.Count; index++) {
            var frame = diffuseFrames[index];
            generatedTexture.SetPixels(index * frame.texture.width, 0, frame.texture.width, frame.texture.height, frame.texture.GetPixels());
        }

        var time = DateTime.Now.Ticks;
        ExportTexture(generatedTexture, "Exported Sprites", "exported_sprite", time);

        if (enableNormals) {
            for (var index = 0; index < normalFrames.Count; index++) {
                var frame = normalFrames[index];
                generatedTexture.SetPixels(index * frame.texture.width, 0, 
                    frame.texture.width, frame.texture.height, frame.texture.GetPixels());
            }
            ExportTexture(generatedTexture, "Exported Sprites", "exported_sprite_n", time);
        }
    }

    public static void ExportTexture(Texture2D generatedTexture, string targetDirectory, string name, long time) {
        var bytes = generatedTexture.EncodeToPNG();
        #if UNITY_EDITOR
            var directory = ExtantDirectory(targetDirectory);
            File.WriteAllBytes($"{directory}/{name}_{time}.png", bytes);
            return;
        #endif
        #if UNITY_WEBGL
            ImageDownloader(System.Convert.ToBase64String(bytes), $"{name}_{time}.png");
        #endif
        #if UNITY_STANDALONE_WIN
            var directory = ExtantDirectory(targetDirectory);
            File.WriteAllBytes($"{directory}/{name}_{time}.png", bytes);
        #endif
    }

    public static string ExtantDirectory(string name) {
        var directory = $"{Application.dataPath}/{name}";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        return directory;
    }

    public void AddInterpolationFrames() {
        var newFrames = FrameInterpolation.AddInterpolationFrames(generatedTextures, 1);
        var scalingMode = generatedTextures[0].scalingModes;
        var size = generatedTextures[0].texture.width;
        normalFrames.Clear();
        diffuseFrames.Clear();
        generatedTextures.Clear();
        for (var index = 0; index < newFrames.Count; index++) {
            DiffuseFrames.Add(SpriteGeneration.CreateSprite(newFrames[index].texture, scalingMode, size));
            NormalFrames.Add(SpriteGeneration.CreateSprite(newFrames[index].normal, scalingMode, size));
            GeneratedTextures.Add(newFrames[index]);    
        }
    }

    public void EvolveColor() {
        Controls.instance.Evolve(generatedTextures, EvolutionType.color);
    }
}
