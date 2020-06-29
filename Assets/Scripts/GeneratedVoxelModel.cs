using System;
using UnityEngine;

[Serializable]
public class GeneratedVoxelModel : GeneratedData {
    public Texture3D modelData;

    public GeneratedVoxelModel() {}

    public GeneratedVoxelModel(
        Texture3D modelData, Vector2 origin, SymmetryOutcome symmetryOutcome, ColorOutcome colorOutcome) {
        this.modelData = modelData;
        this.origin = origin;
        this.symmetryOutcome = symmetryOutcome;
        this.colorOutcome = colorOutcome;
    }
}
