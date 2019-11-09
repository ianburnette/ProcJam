using System.Collections.Generic;
using UnityEngine;

class Recoloring : MonoBehaviour {

    [Header("Palettes")]
    [SerializeField] Texture2D[] palettes;
    [SerializeField] int paletteIndex;
    [SerializeField] bool fullRandom;
    [SerializeField] int backgroundColorIndex;
    [SerializeField] int outlineColorIndex;

    [SerializeField] Color backgroundColorOverride;
    [SerializeField] Color outlineColorOverride;

    [SerializeField] int colorCount;
    [SerializeField] Color[] generatedColors;
    
    public (Color, Color) Recolor(ref Texture2D tex, int frame) {
        if (frame==0)
            GenerateColors();
        var colors = tex.GetPixels();
        var increment = 1f / colorCount;
        var newColors = new Color[colors.Length];
        for (var index = 0; index < colors.Length; index++) {
            var gray = colors[index].grayscale;
            for (var i = 0; i < colorCount; i++) {
                if (gray >= i * increment && gray <= (i + 1) * increment)
                    newColors[index] = generatedColors[i];
            }
        }
        tex.SetPixels(newColors);
        return (backgroundColorIndex == -1 ? backgroundColorOverride : generatedColors[backgroundColorIndex], 
                outlineColorIndex == -1 ? outlineColorOverride : generatedColors[outlineColorIndex]);
    }

    void GenerateColors() {
        generatedColors = new Color[colorCount];
        if (!fullRandom) {
            var colors = GetUniqueColorsFromTexture(palettes[paletteIndex]);
            generatedColors[0] = backgroundColorIndex == -1 ? backgroundColorOverride : colors[backgroundColorIndex];
            for (var index = 1; index < generatedColors.Length; index++) {
                generatedColors[index] = colors[Random.Range(0, colors.Count)];
                colors.Remove(generatedColors[index]);
            }
        }
        else {
            generatedColors[0] = Color.white;
            for (var index = 1; index < generatedColors.Length; index++) 
                generatedColors[index] = Random.ColorHSV(0f, 1f, 1f, 1f);
        }
    }

    List<Color> GetUniqueColorsFromTexture(Texture2D texture) {
        var uniqueColors = new List<Color>();
        foreach (var col in texture.GetPixels()) {
            if (!uniqueColors.Contains(col))
                uniqueColors.Add(col);
        }
        return uniqueColors;
    }
}
