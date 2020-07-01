using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedData {
    public Vector2 origin;
    public SymmetryOutcome symmetryOutcome;
    public ColorOutcome colorOutcome;
    public ScalingMode[] scalingModes;
    public FilterMode filterMode;
}

[Serializable]
public class GeneratedTexture : GeneratedData {
    public Texture2D texture;
    public Texture2D normal;

    public GeneratedTexture() {}
    
    public GeneratedTexture(Texture2D texture, Texture2D normal, Vector2 origin, SymmetryOutcome symmetryOutcome, ColorOutcome colorOutcome) {
        this.texture = texture;
        this.normal = normal;
        this.origin = origin;
        this.symmetryOutcome = symmetryOutcome;
        this.colorOutcome = colorOutcome;
    }
}

[Serializable]
public class SymmetryOutcome {
    public bool horizontalSymmetryResult;
    public bool verticalSymmetryResult;
    public bool forwardDiagonalSymmetryResult;
    public bool backwardDiagonalSymmetryResult;
    public bool quarterHorizontalSymmetryResult;
    public bool quarterVerticalSymmetryResult;
    public bool quarterForwardDiagonalSymmetryResult;
    public bool quarterBackwardDiagonalSymmetryResult;
    public bool lowerIsDominant;
    public SymmetryOutcome(bool horizontalSymmetryResult, bool verticalSymmetryResult, bool forwardDiagonalSymmetryResult,
        bool backwardDiagonalSymmetryResult, bool quarterHorizontalSymmetryResult, bool quarterVerticalSymmetryResult,
        bool quarterForwardDiagonalSymmetryResult, bool quarterBackwardDiagonalSymmetryResult, bool lowerIsDominant) {
        this.horizontalSymmetryResult = horizontalSymmetryResult; 
        this.verticalSymmetryResult = verticalSymmetryResult; 
        this.forwardDiagonalSymmetryResult = forwardDiagonalSymmetryResult; 
        this.backwardDiagonalSymmetryResult = backwardDiagonalSymmetryResult; 
        this.quarterHorizontalSymmetryResult = quarterHorizontalSymmetryResult; 
        this.quarterVerticalSymmetryResult = quarterVerticalSymmetryResult; 
        this.quarterForwardDiagonalSymmetryResult = quarterForwardDiagonalSymmetryResult; 
        this.quarterBackwardDiagonalSymmetryResult = quarterBackwardDiagonalSymmetryResult;
        this.lowerIsDominant = lowerIsDominant;
    }
    public SymmetryOutcome(){}
}

[Serializable]
public class SymmetryOutcome3D {
    public List<SymmetryDirection3D> symmetryDirections = new List<SymmetryDirection3D>();
    public bool lowerIsDominant;
    public SymmetryOutcome3D(List<SymmetryDirection3D> symmetryDirections, bool lowerIsDominant) {
        this.symmetryDirections = symmetryDirections;
        this.lowerIsDominant = lowerIsDominant;
    }

    public SymmetryOutcome3D() {}
}

public class ColorOutcome {
    public Color backgroundColor, outlineColor;
    public Color[] generatedColors;

    public ColorOutcome(Color backgroundColor, Color outlineColor, Color[] generatedColors) {
        this.backgroundColor = backgroundColor;
        this.outlineColor = outlineColor;
        this.generatedColors = generatedColors;
    }
    
    public static ColorOutcome None = new ColorOutcome(Color.black, Color.black, new Color[0]);

    public ColorOutcome() {}
}

public class InheritedSymmetryConfig {
    public bool inherited;
    public SymmetryOutcome outcome;

    public InheritedSymmetryConfig(bool inherited, SymmetryOutcome outcome = null) {
        this.inherited = inherited;
        this.outcome = outcome;
    }
}