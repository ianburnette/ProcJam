using System.Collections.Generic;
using UnityEngine;

class Recoloring : MonoBehaviour {

    [Header("Palettes")]
    [SerializeField] Texture2D[] palettes;

    [Header("Debug")]
    [SerializeField] Color[] generatedColors;

    Camera cam;
    
    public (Color, Color) Recolor(ref Texture2D tex, int frame, ColorConfig colorConfig, BackgroundColorConfig backgroundColorConfig, OutlineConfig outlineConfig) {
        if (!cam)
            cam = Camera.main;
        cam.backgroundColor = BackgroundColor(colorConfig, backgroundColorConfig);
        if (frame == 0)
            GenerateColors(colorConfig, backgroundColorConfig);
        var colors = tex.GetPixels();
        var increment = 1f / colorConfig.colorCountPerSprite;
        var newColors = new Color[colors.Length];
        for (var index = 0; index < colors.Length; index++) {
            var gray = colors[index].grayscale;
            for (var i = 0; i < colorConfig.colorCountPerSprite; i++) {
                if (gray >= i * increment && gray <= (i + 1) * increment)
                    newColors[index] = generatedColors[i];
            }
        }
        tex.SetPixels(newColors);
        return (BackgroundColor(colorConfig, backgroundColorConfig), 
                OutlineColor(outlineConfig));
    }

    Color OutlineColor(OutlineConfig outlineConfig) {
        if (outlineConfig.overrideOutlineColor)
            return outlineConfig.outlineColorOverride;
        if (outlineConfig.randomPaletteColorForOutline)
            return generatedColors[Random.Range(0, generatedColors.Length - 1)];
        return generatedColors[outlineConfig.paletteColorIndexForOutline];
    }

    Color BackgroundColor(ColorConfig colorConfig, BackgroundColorConfig backgroundColorConfig) {
        if (backgroundColorConfig.overrideBackgroundColor)
            return backgroundColorConfig.backgroundColorOverride;
        if (backgroundColorConfig.randomPaletteColorForBackground)
            return generatedColors[Random.Range(0, generatedColors.Length - 1)];
        return generatedColors[backgroundColorConfig.paletteColorIndexForBackground];
    }

    void GenerateColors(ColorConfig colorConfig, BackgroundColorConfig backgroundColorConfig) {
        generatedColors = new Color[colorConfig.colorCountPerSprite];
        if (!colorConfig.overridePaletteColorsWithRandomColors) {
            var colors = GetUniqueColorsFromTexture(palettes[colorConfig.paletteIndex]);
            generatedColors[0] = 
                backgroundColorConfig.overrideBackgroundColor ? 
                backgroundColorConfig.backgroundColorOverride : 
                backgroundColorConfig.randomPaletteColorForBackground ? 
                    colors[Random.Range(0, colors.Count-1)] : 
                    colors[backgroundColorConfig.paletteColorIndexForBackground];
            
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
