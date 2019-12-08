using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Recoloring : MonoBehaviour {

    [Header("Palettes")]
    public Texture2D[] palettes;

    [Header("Debug")]
    [SerializeField] Color[] cachedGeneratedColors;
    [SerializeField] public List<Color>[] uniqueColorsInTextures;

    Camera cam;

    void OnEnable() {
        uniqueColorsInTextures = new List<Color>[palettes.Length];
        for (var index = 0; index < palettes.Length; index++) {
            var texture2D = palettes[index];
            uniqueColorsInTextures[index] = GetUniqueColorsFromTexture(texture2D);
        }
    }
    
    public ColorOutcome Recolor(
        ref Texture2D tex, int frame, ColorConfig colorConfig, BackgroundColorConfig backgroundColorConfig,
        OutlineConfig outlineConfig, ColorOutcome colorOutcome) {

        if (colorOutcome == ColorOutcome.None) {
            colorOutcome = new ColorOutcome();
            if (frame == 0)
                colorOutcome.generatedColors =
                    cachedGeneratedColors = GenerateColors(colorConfig, backgroundColorConfig);
            else
                colorOutcome.generatedColors = cachedGeneratedColors; 
            colorOutcome.backgroundColor = SetBackgroundColor();
            colorOutcome.outlineColor = OutlineColor(outlineConfig, frame, colorOutcome.generatedColors);
        } 

        var colors = tex.GetPixels();
        var increment = 1f / colorConfig.colorCountPerSprite;
        var newColors = new Color[colors.Length];
        for (var index = 0; index < colors.Length; index++) {
            var gray = colors[index].grayscale;
            for (var i = 0; i < colorConfig.colorCountPerSprite; i++) {
                if (gray >= i * increment && gray <= (i + 1) * increment)
                    newColors[index] = colorOutcome.generatedColors[i];
            }
        }
        tex.SetPixels(newColors);
        return colorOutcome;

        Color SetBackgroundColor() {
            if (!cam) cam = Camera.main;
            var backgroundColor =
                BackgroundColor(backgroundColorConfig, colorConfig, frame, colorOutcome.generatedColors);
            cam.backgroundColor = backgroundColor;
            return backgroundColor;
        }
    }

    Color OutlineColor(OutlineConfig outlineConfig, int frame, IReadOnlyList<Color> generatedColors) {
        if (outlineConfig.overrideOutlineColor)
            return outlineConfig.outlineColorOverride;
        if (outlineConfig.randomPaletteColorForOutline && frame==0)
            return generatedColors[Random.Range(0, generatedColors.Count - 1)];
        return generatedColors[outlineConfig.paletteColorIndexForOutline];
    }

    Color BackgroundColor(BackgroundColorConfig backgroundColorConfig, ColorConfig colorConfig, int frame, Color[] generatedColors) {
        if (!colorConfig.usePaletteColors)
            return Color.black;
        if (backgroundColorConfig.overrideBackgroundColor)
            return backgroundColorConfig.backgroundColorOverride;
        if (backgroundColorConfig.randomPaletteColorForBackground && frame==0)
            return generatedColors[Random.Range(0, generatedColors.Length - 1)];
        return generatedColors[backgroundColorConfig.paletteColorIndexForBackground];
    }

    Color[] GenerateColors(ColorConfig colorConfig, BackgroundColorConfig backgroundColorConfig) {
        var generatedColors = new Color[colorConfig.colorCountPerSprite];
        if (colorConfig.usePaletteColors) {
            if (backgroundColorConfig.overrideBackgroundColor)
                generatedColors[0] = backgroundColorConfig.backgroundColorOverride;
            else if (backgroundColorConfig.randomPaletteColorForBackground) {
                generatedColors[0] = uniqueColorsInTextures[colorConfig.paletteIndex][
                    Random.Range(0, uniqueColorsInTextures[colorConfig.paletteIndex].Count - 1)];
            } else {
                generatedColors[0] =
                    uniqueColorsInTextures[colorConfig.paletteIndex][
                        backgroundColorConfig.paletteColorIndexForBackground];
            }
            
            for (var index = 1; index < generatedColors.Length; index++) {
                generatedColors[index] = uniqueColorsInTextures[colorConfig.paletteIndex][Random.Range(0, uniqueColorsInTextures[colorConfig.paletteIndex].Count)];
                //uniqueColorsInTextures.Remove(generatedColors[index]);
            }
        }
        else {
            generatedColors[0] = Color.black;
            for (var index = 1; index < generatedColors.Length; index++) 
                generatedColors[index] = Random.ColorHSV(0f, 1f, 1f, 1f);
        }

        return generatedColors;
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
