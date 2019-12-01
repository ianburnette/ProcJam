using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[CustomEditor(typeof(InGameControls))]
public class InGameControlsEditor : Editor
{
    public override void OnInspectorGUI()
    {    
        var myScript = (InGameControls)target;
        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);
        if(GUILayout.Button("Refresh")) myScript.RefreshUi();
        DrawDefaultInspector();
    }
}
#endif
    

public class InGameControls : MonoBehaviour {
    [SerializeField] Controls controls;
    Recoloring recoloring;

    [Header("Sizing")]
    [SerializeField] Slider spacing;
    [SerializeField] Slider imageGridSize;
    [SerializeField] Slider pixelSize;
    [Header("Noise")]
    [SerializeField] Slider scale0;
    [SerializeField] Slider scale1;
    [SerializeField] Slider scale2;
    [SerializeField] Slider scale3;
    [SerializeField] Slider frequency0;
    [SerializeField] Slider frequency1;
    [SerializeField] Slider frequency2;
    [SerializeField] Slider frequency3;
    [SerializeField] Toggle toggleFrequencyRandomization;
    [SerializeField] Toggle toggleRandomOrigin;
    [SerializeField] TMP_InputField randomOriginBound;
    [SerializeField] TMP_InputField manualOriginX;
    [SerializeField] TMP_InputField manualOriginY;
    [Header("Color")] 
    [SerializeField] Toggle colorEnabled;
    [SerializeField] Toggle usePalettes;
    [SerializeField] Slider colorsPerSprite;
    [SerializeField] TMP_Dropdown palettes;
    [Header("Background")]
    [SerializeField] Toggle randomPaletteColor;
    [SerializeField] TMP_Dropdown paletteColor;
    [Header("Outline")]
    [SerializeField] Toggle outlineEnabled;
    [SerializeField] Toggle outlineAfterScaling;
    [SerializeField] TMP_Dropdown outlineColor;
    [Header("Symmetry")]
    [SerializeField] Toggle allowMultipleSymmetries;
    [SerializeField] Toggle enforceSomeTypeOfSymmetry;
    [SerializeField] Slider horizontalChance;
    [SerializeField] Slider horizontalQuarterChance;
    [SerializeField] Slider verticalChance;
    [SerializeField] Slider verticalQuarterChance;
    [SerializeField] Slider forwardDiagonalChance;
    [SerializeField] Slider forwardDiagonalQuarterChance;
    [SerializeField] Slider backwardDiagonalChance;
    [SerializeField] Slider backwardDiagonalQuarterChance;
    [Header("Animation")]
    [SerializeField] Slider frameCount;
    [SerializeField] Slider timeBetweenFrames;
    [SerializeField] TMP_Dropdown animationMode;
    [SerializeField] Slider noiseFrameOffset;
    [Header("Shading")]
    [SerializeField] Toggle enableShading;
    [SerializeField] Toggle shadingByColor;
    [SerializeField] Slider shadingIntensity;
    [SerializeField] Toggle enableHighlights;
    [SerializeField] Toggle highlightByColor;
    [SerializeField] Slider highlightIntensity;
    [Header("Normals")]
    [SerializeField] Toggle enableNormals;
    [SerializeField] Slider normalStrength;
    [SerializeField] Toggle viewNormalsOnly;
    [SerializeField] Toggle disableNormalsRendering;
    [SerializeField] Toggle enableGlobalLight;
    [SerializeField] Toggle enableRotatingLight;
    [SerializeField] Slider lightRotationSpeed;
    [SerializeField] Toggle enableCursorFollowLight;
    [Header("Scaling")]
    [SerializeField] Slider scalingPassCount;
    [SerializeField] List<TMP_Dropdown> scalingPasses;
    [SerializeField] TMP_Dropdown filterMode;
    [Header("Cleanup")]
    [SerializeField] Toggle allowPixelsOnEdge;
    [SerializeField] TMP_Dropdown lonePixelEvaluation;
    [SerializeField] Slider chanceToDeleteLonePixel;
    [Header("Presets")]
    [SerializeField] TMP_Dropdown presets;
    
    public ConfigurationAsset Configuration {
        get => controls.Configuration;
        set => controls.Configuration = value;
    }

    //SIZING
    public void Spacing(Single spacing) => Configuration.sizingConfig.spacing = (int)spacing;
    public void ImageGridSize(Single size) => Configuration.sizingConfig.imageGridSize = (int)size;
    public void PixelSize(Single pixelSize) => Configuration.sizingConfig.pixelSize = (int)pixelSize;
    //NOISE
    public void Scale0(Single scale0) => Configuration.noiseConfig.octaves[0].scale = (float)Math.Round(scale0, 2);
    public void Scale1(Single scale1) => Configuration.noiseConfig.octaves[1].scale = (float)Math.Round(scale1, 2);
    public void Scale2(Single scale2) => Configuration.noiseConfig.octaves[2].scale = (float)Math.Round(scale2, 2);
    public void Scale3(Single scale3) => Configuration.noiseConfig.octaves[3].scale = (float)Math.Round(scale3, 2);
    public void Frequency0(Single frequency0) => Configuration.noiseConfig.octaves[0].frequency = (float)Math.Round(frequency0, 2);
    public void Frequency1(Single frequency1) => Configuration.noiseConfig.octaves[1].frequency = (float)Math.Round(frequency1, 2);
    public void Frequency2(Single frequency2) => Configuration.noiseConfig.octaves[2].frequency = (float)Math.Round(frequency2, 2);
    public void Frequency3(Single frequency3) => Configuration.noiseConfig.octaves[3].frequency = (float)Math.Round(frequency3, 2);
    public void ToggleFrequencyRandomization(bool value) => Configuration.noiseConfig.randomizeFrequency = value;
    public void ToggleRandomOrigin(bool value) => Configuration.noiseConfig.randomOrigin = value;
    public void RandomOriginBound(string bound) => Configuration.noiseConfig.randomOriginBound = float.Parse(bound);
    public void ManualOriginX(string origin) => Configuration.noiseConfig.manualOrigin.x = float.Parse(origin);
    public void ManualOriginY(string origin) => Configuration.noiseConfig.manualOrigin.y = float.Parse(origin);
    //COLOR
    public void ColorEnabled(bool value) => Configuration.colorConfig.colorEnabled = value;
    public void UsePalettes(bool value) => Configuration.colorConfig.usePaletteColors = value;
    public void ColorsPerSprite(Single count) => Configuration.colorConfig.colorCountPerSprite = (int)count;
    public void Palettes(int value) {
        Configuration.colorConfig.paletteIndex = value;
        colorsPerSprite.maxValue = controls.spriteGeneration.Recoloring.uniqueColorsInTextures[value].Count;
    }

    //BACKGROUND
    public void RandomPaletteColor(bool value) => Configuration.backgroundColorConfig.randomPaletteColorForBackground = value;
    //public void PaletteColor(int value) => Configuration.backgroundColorConfig.paletteColorIndexForBackground = value;
    public void PaletteColor(int value) {
        if (Configuration.backgroundColorConfig.randomPaletteColorForBackground) {
            Configuration.backgroundColorConfig.overrideBackgroundColor = false;
        } else {
            Configuration.backgroundColorConfig.overrideBackgroundColor = true;
            Configuration.backgroundColorConfig.backgroundColorOverride =
                value == 0 ? Color.black :
                value == 1 ? Color.white :
                value == 2 ? Color.clear :
                value == 3 ? Color.magenta :
                controls.spriteGeneration.Recoloring.uniqueColorsInTextures[Configuration.colorConfig.paletteIndex][value];
        }
    }

    //OUTLINE
    public void OutlineEnabled(bool value) => Configuration.outlineConfig.outlineEnabled = value;
    public void OutlineAfterScaling(bool value) => Configuration.outlineConfig.applyOutlineAfterScaling = value;
    public void OutlineColor(int value) =>
        Configuration.outlineConfig.outlineColorOverride = 
        value == 0 ? Color.black :
        value == 1 ? Color.white :
        value == 2 ? Color.clear :
        value == 3 ? Color.magenta :
        controls.spriteGeneration.Recoloring.uniqueColorsInTextures[Configuration.colorConfig.paletteIndex][value];
    //SYMMETRY
    public void AllowMultipleSymmetries(bool value) => Configuration.symmetryConfig.allowMultipleSymmetryTypes = value;
    public void EnforceSomeTypeOfSymmetry(bool value) => Configuration.symmetryConfig.enforceSomeTypeOfSymmetry = value;
    public void HorizontalChance(float value) => Configuration.symmetryConfig.horizontalSymmetryChance = (float)Math.Round(value, 2);
    public void HorizontalQuarterChance(float value) => Configuration.symmetryConfig.quarterHorizontalSymmetryChance = (float)Math.Round(value, 2);
    public void VerticalChance(float value) => Configuration.symmetryConfig.verticalSymmetryChance = (float)Math.Round(value, 2);
    public void VerticalQuarterChance(float value) => Configuration.symmetryConfig.quarterVerticalSymmetryChance = (float)Math.Round(value, 2);
    public void ForwardDiagonalChance(float value) => Configuration.symmetryConfig.forwardDiagonalSymmetryChance = (float)Math.Round(value, 2);
    public void ForwardDiagonalQuarterChance(float value) => Configuration.symmetryConfig.quarterForwardDiagonalSymmetryChance = (float)Math.Round(value, 2);
    public void BackwardDiagonalChance(float value) => Configuration.symmetryConfig.backwardDiagonalSymmetryChance = (float)Math.Round(value, 2);
    public void BackwardDiagonalQuarterChance(float value) => Configuration.symmetryConfig.quarterBackwardDiagonalSymmetryChance = (float)Math.Round(value, 2);
    //ANIMATION
    public void FrameCount(float value) => Configuration.animationConfig.animationFrameCount = (int)value;
    public void TimeBetweenFrames(float value) => Configuration.animationConfig.timeBetweenFrames = (float)Math.Round(value, 2);
    public void SetAnimationMode(int value) => Configuration.animationConfig.animationMode =
        value == 0 ? AnimationMode.pingPong : AnimationMode.loop;
    public void NoiseFrameOffset(float value) => Configuration.noiseConfig.animationFrameNoiseOffset = (float)Math.Round(value, 2);
    //SHADING
    public void EnableShading(bool value) => Configuration.shadingConfig.enableShading = value;
    public void ShadingByColor(bool value) => Configuration.shadingConfig.shadingByColor = value;
    public void ShadingIntensity(float value) => Configuration.shadingConfig.shadingIntensity = value;
    public void EnableHighlights(bool value) => Configuration.shadingConfig.enableHighlights = value;
    public void HighlightByColor(bool value) => Configuration.shadingConfig.highlightByColor = value;
    public void HighlightIntensity(float value) => Configuration.shadingConfig.highlightIntensity = value;
    //NORMALS
    public void EnableNormals(bool value) => Configuration.normalsConfig.enableNormals = value;
    public void NormalStrength(float value) => Configuration.normalsConfig.normalStrength = (float)Math.Round(value, 2);
    public void ViewNormalsOnly(bool value) => Configuration.normalsConfig.viewNormalsOnly = value;
    public void DisableNormalsRendering(bool value) => Configuration.normalsConfig.disableNormalsDisplay = value;
    public void EnableGlobalLight(bool value) => Configuration.normalsConfig.globalLightEnabled = value;
    public void EnableRotatingLight(bool value) => Configuration.normalsConfig.rotatingLightEnabled = value;
    public void LightRotationSpeed(float value) => controls.spriteGeneration.normalGeneration.RotationSpeed = (float)Math.Round(value, 2);
    public void EnableCursorFollowLight(bool value) => Configuration.normalsConfig.cursorFollowLightEnabled = value;
    //SCALING
    public void ScalingPassCount(float value) {
        Configuration.scalingConfig.ResizeScalingMode((int)value);
        RefreshScalingPasses();
        ResizeScalingModeList((int)value);
    }
    void RefreshScalingPasses(int _ = 0) {
        Configuration.scalingConfig.ResizeScalingMode(scalingPasses.Count);
        for (var i = 0; i < scalingPasses.Count; i++)
            Configuration.scalingConfig.scalingModes[i] = ScalingModeByIndex(scalingPasses[i].value);
    }
    public void SetFilterMode(int value) => Configuration.scalingConfig.filterMode = value == 0 ? FilterMode.Point : value == 1 ? FilterMode.Bilinear : FilterMode.Trilinear;
    //CLEANUP
    public void AllowPixelsOnEdge(bool value) => Configuration.cleanupConfig.allowPixelsOnEdgeOfSprite = value;
    public void LonePixelEvaluation(int value) => Configuration.cleanupConfig.lonePixelEvaluationMode =
        value == 0 ? LonePixelEvaluationMode.CardinalDirectionsOnly : LonePixelEvaluationMode.IncludeDiagonals;
    public void ChanceToDeleteLonePixel(float value) => Configuration.cleanupConfig.chanceToDeleteLonePixels = value;
    //PRESETS
    public void SetPreset(Int32 preset) {
        Configuration = controls.configurationAssets[preset];
        //RefreshUi();
        controls.Generate();
    }

    void OnEnable() {
        recoloring = controls.spriteGeneration.Recoloring;
        BindUi();
        RefreshUi();
        controls.Generate();
    }
    
    public void BindUi() {
        //SIZING
        spacing.onValueChanged.AddListener(Spacing);
        imageGridSize.onValueChanged.AddListener(ImageGridSize);
        pixelSize.onValueChanged.AddListener(PixelSize);
        //NOISE
        scale0.onValueChanged.AddListener(Scale0);
        scale1.onValueChanged.AddListener(Scale1);
        scale2.onValueChanged.AddListener(Scale2);
        scale3.onValueChanged.AddListener(Scale3);
        frequency0.onValueChanged.AddListener(Frequency0);
        frequency1.onValueChanged.AddListener(Frequency1);
        frequency2.onValueChanged.AddListener(Frequency2);
        frequency3.onValueChanged.AddListener(Frequency3);
        toggleFrequencyRandomization.onValueChanged.AddListener(ToggleFrequencyRandomization);
        toggleRandomOrigin.onValueChanged.AddListener(ToggleRandomOrigin);
        randomOriginBound.onValueChanged.AddListener(RandomOriginBound);
        manualOriginX.onValueChanged.AddListener(ManualOriginX);
        manualOriginY.onValueChanged.AddListener(ManualOriginY);
        //COLOR
        colorEnabled.onValueChanged.AddListener(ColorEnabled); 
        usePalettes.onValueChanged.AddListener(UsePalettes); 
        colorsPerSprite.onValueChanged.AddListener(ColorsPerSprite);
        palettes.onValueChanged.AddListener(Palettes);
        //BACKGROUND
        randomPaletteColor.onValueChanged.AddListener(RandomPaletteColor);
        paletteColor.onValueChanged.AddListener(PaletteColor);
        
        //OUTLINE
        outlineEnabled.onValueChanged.AddListener(OutlineEnabled);
        outlineAfterScaling.onValueChanged.AddListener(OutlineAfterScaling);
        outlineColor.onValueChanged.AddListener(OutlineColor);
        //SYMMETRY
        allowMultipleSymmetries.onValueChanged.AddListener(AllowMultipleSymmetries);
        enforceSomeTypeOfSymmetry.onValueChanged.AddListener(EnforceSomeTypeOfSymmetry);
        horizontalChance.onValueChanged.AddListener(HorizontalChance);
        horizontalQuarterChance.onValueChanged.AddListener(HorizontalQuarterChance);
        verticalChance.onValueChanged.AddListener(VerticalChance);
        verticalQuarterChance.onValueChanged.AddListener(VerticalQuarterChance);
        forwardDiagonalChance.onValueChanged.AddListener(ForwardDiagonalChance);
        forwardDiagonalQuarterChance.onValueChanged.AddListener(ForwardDiagonalQuarterChance);
        backwardDiagonalChance.onValueChanged.AddListener(BackwardDiagonalChance);
        backwardDiagonalQuarterChance.onValueChanged.AddListener(BackwardDiagonalQuarterChance);
        //ANIMATION
        frameCount.onValueChanged.AddListener(FrameCount);
        timeBetweenFrames.onValueChanged.AddListener(TimeBetweenFrames);
        animationMode.onValueChanged.AddListener(SetAnimationMode);
        noiseFrameOffset.onValueChanged.AddListener(NoiseFrameOffset);
        //SHADING
        enableShading.onValueChanged.AddListener(EnableShading);
        shadingByColor.onValueChanged.AddListener(ShadingByColor);
        shadingIntensity.onValueChanged.AddListener(ShadingIntensity);
        enableHighlights.onValueChanged.AddListener(EnableHighlights);
        highlightByColor.onValueChanged.AddListener(HighlightByColor);
        highlightIntensity.onValueChanged.AddListener(HighlightIntensity);
        //NORMALS
        enableNormals.onValueChanged.AddListener(EnableNormals);
        normalStrength.onValueChanged.AddListener(NormalStrength);
        viewNormalsOnly.onValueChanged.AddListener(ViewNormalsOnly);
        disableNormalsRendering.onValueChanged.AddListener(DisableNormalsRendering);
        enableGlobalLight.onValueChanged.AddListener(EnableGlobalLight);
        enableRotatingLight.onValueChanged.AddListener(EnableRotatingLight);
        lightRotationSpeed.onValueChanged.AddListener(LightRotationSpeed);
        enableCursorFollowLight.onValueChanged.AddListener(EnableCursorFollowLight);
        //SCALING
        scalingPassCount.onValueChanged.AddListener(ScalingPassCount);
        foreach (var scalingPass in scalingPasses) {
            scalingPass.onValueChanged.AddListener(RefreshScalingPasses);
        }
        filterMode.onValueChanged.AddListener(SetFilterMode);
        //CLEANUP 
        allowPixelsOnEdge.onValueChanged.AddListener(AllowPixelsOnEdge);
        lonePixelEvaluation.onValueChanged.AddListener(LonePixelEvaluation);
        chanceToDeleteLonePixel.onValueChanged.AddListener(ChanceToDeleteLonePixel);
        //PRESETS
        presets.onValueChanged.AddListener(SetPreset);
    }
    
    public void RefreshUi() {
        //SIZING
        spacing.value = Configuration.sizingConfig.spacing;
        imageGridSize.value = Configuration.sizingConfig.imageGridSize;
        pixelSize.value = Configuration.sizingConfig.pixelSize;
        //NOISE
        scale0.value = Configuration.noiseConfig.octaves[0].scale; 
        scale1.value = Configuration.noiseConfig.octaves[1].scale; 
        scale2.value = Configuration.noiseConfig.octaves[2].scale; 
        scale3.value = Configuration.noiseConfig.octaves[3].scale; 
        frequency0.value = Configuration.noiseConfig.octaves[0].frequency; 
        frequency1.value = Configuration.noiseConfig.octaves[1].frequency; 
        frequency2.value = Configuration.noiseConfig.octaves[2].frequency; 
        frequency3.value = Configuration.noiseConfig.octaves[3].frequency; 
        toggleFrequencyRandomization.isOn = Configuration.noiseConfig.randomizeFrequency;
        toggleRandomOrigin.isOn = Configuration.noiseConfig.randomOrigin;
        randomOriginBound.text = Configuration.noiseConfig.randomOriginBound.ToString();
        manualOriginX.text = Configuration.noiseConfig.manualOrigin.x.ToString();
        manualOriginY.text = Configuration.noiseConfig.manualOrigin.y.ToString();
        //COLOR
        colorEnabled.isOn = Configuration.colorConfig.colorEnabled; 
        usePalettes.isOn = Configuration.colorConfig.usePaletteColors; 
        colorsPerSprite.value = Configuration.colorConfig.colorCountPerSprite;
        palettes.ClearOptions();
        var paletteOptions = new List<TMP_Dropdown.OptionData>();
        foreach (var t in controls.spriteGeneration.Recoloring.palettes) {
            var option = new TMP_Dropdown.OptionData(t.name);
            paletteOptions.Add(option);
        }
        palettes.AddOptions(paletteOptions);
        palettes.value = Configuration.colorConfig.paletteIndex;
        //BACKGROUND
        randomPaletteColor.isOn = Configuration.backgroundColorConfig.randomPaletteColorForBackground;
        paletteColor.options = GetColorDropdownOptions();
        paletteColor.value = Configuration.backgroundColorConfig.paletteColorIndexForBackground;
        //OUTLINE
        outlineEnabled.isOn = Configuration.outlineConfig.outlineEnabled;
        outlineAfterScaling.isOn = Configuration.outlineConfig.applyOutlineAfterScaling;
        outlineColor.options = GetColorDropdownOptions();
        outlineColor.value = 1;
        //SYMMETRY
        allowMultipleSymmetries.isOn = Configuration.symmetryConfig.allowMultipleSymmetryTypes;
        enforceSomeTypeOfSymmetry.isOn = Configuration.symmetryConfig.enforceSomeTypeOfSymmetry;
        horizontalChance.value = (float)Math.Round(Configuration.symmetryConfig.horizontalSymmetryChance, 1);
        horizontalQuarterChance.value = (float)Math.Round(Configuration.symmetryConfig.quarterHorizontalSymmetryChance, 1);
        verticalChance.value = (float)Math.Round(Configuration.symmetryConfig.verticalSymmetryChance, 1);
        verticalQuarterChance.value = (float)Math.Round(Configuration.symmetryConfig.quarterVerticalSymmetryChance, 1);
        forwardDiagonalChance.value = (float)Math.Round(Configuration.symmetryConfig.forwardDiagonalSymmetryChance, 1);
        forwardDiagonalQuarterChance.value = (float)Math.Round(Configuration.symmetryConfig.quarterForwardDiagonalSymmetryChance, 1);
        backwardDiagonalChance.value = (float)Math.Round(Configuration.symmetryConfig.backwardDiagonalSymmetryChance, 1);
        backwardDiagonalQuarterChance.value = (float)Math.Round(Configuration.symmetryConfig.quarterBackwardDiagonalSymmetryChance, 1);
        //ANIMATION
        frameCount.value = Configuration.animationConfig.animationFrameCount;
        timeBetweenFrames.value = (float)Math.Round(Configuration.animationConfig.timeBetweenFrames, 2);
        animationMode.value = Configuration.animationConfig.animationMode == AnimationMode.pingPong ? 0 : 1;
        noiseFrameOffset.value = (float)Math.Round(Configuration.noiseConfig.animationFrameNoiseOffset, 2);
        //SHADING
        enableShading.isOn = Configuration.shadingConfig.enableShading;
        shadingByColor.isOn = Configuration.shadingConfig.shadingByColor;
        shadingIntensity.value = Configuration.shadingConfig.shadingIntensity;
        enableHighlights.isOn = Configuration.shadingConfig.enableHighlights;
        highlightByColor.isOn = Configuration.shadingConfig.highlightByColor;
        highlightIntensity.value = Configuration.shadingConfig.highlightIntensity;
        //NORMALS
        enableNormals.isOn = Configuration.normalsConfig.enableNormals;
        normalStrength.value = Configuration.normalsConfig.normalStrength;
        viewNormalsOnly.isOn = Configuration.normalsConfig.viewNormalsOnly;
        disableNormalsRendering.isOn = Configuration.normalsConfig.disableNormalsDisplay;
        enableGlobalLight.isOn = Configuration.normalsConfig.globalLightEnabled;
        enableRotatingLight.isOn = Configuration.normalsConfig.rotatingLightEnabled;
        lightRotationSpeed.value = controls.spriteGeneration.normalGeneration.RotationSpeed;
        enableCursorFollowLight.isOn =Configuration.normalsConfig.cursorFollowLightEnabled; 
        //SCALING
        ResizeScalingModeList((int)scalingPassCount.value);
        filterMode.value = Configuration.scalingConfig.filterMode == FilterMode.Point ? 0 : 
            Configuration.scalingConfig.filterMode == FilterMode.Bilinear ? 1 : 2;
        //CLEANUP
        allowPixelsOnEdge.isOn = Configuration.cleanupConfig.allowPixelsOnEdgeOfSprite;
        lonePixelEvaluation.value = Configuration.cleanupConfig.lonePixelEvaluationMode ==
                                    LonePixelEvaluationMode.CardinalDirectionsOnly ? 0 : 1;
        chanceToDeleteLonePixel.value = Configuration.cleanupConfig.chanceToDeleteLonePixels;
        //PRESETS
        presets.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var c in controls.configurationAssets)
            options.Add(new TMP_Dropdown.OptionData(c.name));
        presets.AddOptions(options);
        presets.value = 0;
        for (var i = 0; i < controls.configurationAssets.Count; i++)
            if (controls.Configuration == controls.configurationAssets[i])
                presets.value = i;
    }

    List<Color> ColorOptions() {
        var colors = new List<Color>();
        colors.Add(Color.black);
        colors.Add(Color.white);
        colors.Add(Color.clear);
        colors.Add(Color.magenta);
        var paletteColors = controls.spriteGeneration.Recoloring.uniqueColorsInTextures[Configuration.colorConfig.paletteIndex];
        foreach (var paletteColor in paletteColors) colors.Add(paletteColor);
        return colors;
    }
    
    void ResizeScalingModeList(int count) {
        for (int i = 0; i < scalingPasses.Count; i++) {
            if (i < count) {
                scalingPasses[i].gameObject.SetActive(true);
                scalingPasses[i].value = 0;
            } else
                scalingPasses[i].gameObject.SetActive(false);
        }
    }

    ScalingMode ScalingModeByIndex(int index) {
        switch (index) {
            case 0: return ScalingMode.none;
            case 1: return ScalingMode.x2;
            case 2: return ScalingMode.x4;
            case 3: return ScalingMode.x10;
            case 4: return ScalingMode.eagle2;
            case 5: return ScalingMode.eagle3;
        }
        return ScalingMode.none;
    }
    
    int IndexByScalingMode(ScalingMode scalingMode) {
        switch (scalingMode) {
            case ScalingMode.none: return 0;
            case ScalingMode.x2: return 1;
            case ScalingMode.x4: return 2;
            case ScalingMode.x10: return 3;
            case ScalingMode.eagle2: return 4;
            case ScalingMode.eagle3: return 5;
        }
        return 0;
    }

    List<TMP_Dropdown.OptionData> GetColorDropdownOptions() {
        var list = new List<TMP_Dropdown.OptionData>();
        var colors = ColorOptions();
        for (var index = 0; index < colors.Count; index++) {
            if (index == 0) list.Add(new TMP_Dropdown.OptionData("Black"));
            else if (index == 1) list.Add(new TMP_Dropdown.OptionData("White"));
            else if (index == 2) list.Add(new TMP_Dropdown.OptionData("Clear"));
            else if (index == 3) list.Add(new TMP_Dropdown.OptionData("Magenta"));
            else {
                // var color = recoloring.uniqueColorsInTextures[Configuration.colorConfig.paletteIndex][index];
               // var texture2D = new Texture2D(1,1);
               // texture2D.SetPixel(0, 0, color);
               // var sprite = Sprite.Create(texture2D, new Rect(0,0,1,1), Vector2.zero);
                //sprite.name = $"{color.ToString()}";
                list.Add(new TMP_Dropdown.OptionData($"Color {index-3}"));
            }
        }
        return list;
    }
}
