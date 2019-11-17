using System.Collections.Generic;
using UnityEngine;

class Recoloring : MonoBehaviour {

    [Header("Palettes")]
    [SerializeField] Texture2D[] palettes;

    [Header("Debug")]
    [SerializeField] Color[] generatedColors;

    Camera cam;
    
    public (Color, Color) Recolor(ref Texture2D tex, int frame, Configuration configuration) {
        if (!cam)
            cam = Camera.main;
        cam.backgroundColor = BackgroundColor(configuration);
        if (frame == 0)
            GenerateColors(configuration);
        var colors = tex.GetPixels();
        var increment = 1f / configuration.spriteColorConfig.colorCountPerSprite;
        var newColors = new Color[colors.Length];
        for (var index = 0; index < colors.Length; index++) {
            var gray = colors[index].grayscale;
            for (var i = 0; i < configuration.spriteColorConfig.colorCountPerSprite; i++) {
                if (gray >= i * increment && gray <= (i + 1) * increment)
                    newColors[index] = generatedColors[i];
            }
        }
        tex.SetPixels(newColors);
        return (BackgroundColor(configuration), 
                OutlineColor(configuration));
    }

    Color OutlineColor(Configuration config) {
        if (config.outlineConfig.overrideOutlineColor)
            return config.outlineConfig.outlineColorOverride;
        if (config.outlineConfig.randomPaletteColorForOutline)
            return generatedColors[Random.Range(0, generatedColors.Length - 1)];
        return generatedColors[config.outlineConfig.paletteColorIndexForOutline];
    }

    Color BackgroundColor(Configuration config) {
        if (config.backgroundColorConfig.overrideBackgroundColor)
            return config.backgroundColorConfig.backgroundColorOverride;
        if (config.backgroundColorConfig.randomPaletteColorForBackground)
            return generatedColors[Random.Range(0, generatedColors.Length - 1)];
        return generatedColors[config.backgroundColorConfig.paletteColorIndexForBackground];
    }

    void GenerateColors(Configuration config) {
        generatedColors = new Color[config.spriteColorConfig.colorCountPerSprite];
        if (!config.spriteColorConfig.overridePaletteColorsWithRandomColors) {
            var colors = GetUniqueColorsFromTexture(palettes[config.spriteColorConfig.paletteIndex]);
            generatedColors[0] = 
                config.backgroundColorConfig.overrideBackgroundColor ? 
                config.backgroundColorConfig.backgroundColorOverride : 
                config.backgroundColorConfig.randomPaletteColorForBackground ? 
                    colors[Random.Range(0, colors.Count-1)] : 
                    colors[config.backgroundColorConfig.paletteColorIndexForBackground];
            
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
