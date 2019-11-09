using System.Collections.Generic;
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

        if (frames.Count <= currentFrameIndex + 1)
            image.sprite = frames[currentFrameIndex];
    }
}
