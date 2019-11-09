using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FrameAnimation : MonoBehaviour {
    [SerializeField] Image image;
    [SerializeField] List<Sprite> frames = new List<Sprite>();
    [SerializeField] float frameTime;

    int currentFrameIndex;
    bool ascending = true;

    public List<Sprite> Frames {
        set {
            frames = value;
            CancelInvoke();
            InvokeRepeating(nameof(Animate), frameTime, frameTime);
        }
    }

    void Animate() {
        if (ascending && currentFrameIndex + 1 < frames.Count)
            currentFrameIndex++;
        else if (ascending && currentFrameIndex + 1 >= frames.Count) {
            ascending = false;
            currentFrameIndex--;
        }
        else if (!ascending && currentFrameIndex - 1 > 0) {
            currentFrameIndex--;
        }
        else if (!ascending && currentFrameIndex - 1 <= 0) {
            ascending = true;
            currentFrameIndex++;
        }

        //if (frames.Count <= currentFrameIndex + 1)
            image.sprite = frames[currentFrameIndex];
    }

    public void Export() {
        var generatedTexture = new Texture2D(frames[0].texture.width * frames.Count, frames[0].texture.height);
        for (var index = 0; index < frames.Count; index++) {
            var frame = frames[index];
            generatedTexture.SetPixels(index * frame.texture.width, 0, frame.texture.width, frame.texture.height, frame.texture.GetPixels());
        }
        var bytes = generatedTexture.EncodeToPNG();
        var directory = $"{Application.dataPath}/Exported Sprites";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes($"{directory}/exported_sprite_{Time.time}.png", bytes);
    }
}
