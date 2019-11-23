using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FrameAnimation : MonoBehaviour {
    [SerializeField] Image image;
    [SerializeField] List<Sprite> frames = new List<Sprite>();
    [SerializeField] float frameTime;

    int currentFrameIndex;
    bool ascending = true;

    #if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void ImageDownloader(string str, string fn);
    #endif
    
    public List<Sprite> Frames {
        get => frames;
        set {
            frames = value;
            CancelInvoke();
            InvokeRepeating(nameof(Animate), FrameTime, FrameTime);
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

    void Animate() {
        if (ascending && currentFrameIndex + 1 < frames.Count)
            currentFrameIndex++;
        else if (ascending && currentFrameIndex + 1 >= frames.Count) {
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

        if (frames.Count >= currentFrameIndex + 1)
            ImageComponent.sprite = frames[currentFrameIndex];
    }

    public void Export() {
        var generatedTexture = new Texture2D(frames[0].texture.width * frames.Count, frames[0].texture.height);
        for (var index = 0; index < frames.Count; index++) {
            var frame = frames[index];
            generatedTexture.SetPixels(index * frame.texture.width, 0, frame.texture.width, frame.texture.height, frame.texture.GetPixels());
        }
        ExportTexture(generatedTexture, "Exported Sprites");
    }

    public static void ExportTexture(Texture2D generatedTexture, string targetDirectory) {
        var bytes = generatedTexture.EncodeToPNG();
        #if UNITY_WEBGL
            ImageDownloader(System.Convert.ToBase64String(bytes), $"exported_sprite_{DateTime.Now}.png");
        #endif
        #if UNITY_STANDALONE_WIN
            var directory = ExtantDirectory(targetDirectory);
            File.WriteAllBytes($"{directory}/exported_sprite_{Time.time}.png", bytes);
        #endif
        #if UNITY_EDITOR
            var directory = ExtantDirectory(targetDirectory);
            File.WriteAllBytes($"{directory}/exported_sprite_{DateTime.Now}.png", bytes);
        #endif
    }

    public static string ExtantDirectory(string name) {
        var directory = $"{Application.dataPath}/{name}";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        return directory;
    }
}
