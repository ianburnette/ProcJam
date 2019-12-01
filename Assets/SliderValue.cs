using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour {
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text text;
    void Update() {
        var value = slider.value;
        text.text = slider.wholeNumbers ? $"{(value):0}" : $"{value:0.00}";
    }
}
