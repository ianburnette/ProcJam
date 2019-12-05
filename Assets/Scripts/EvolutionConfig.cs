using System.Collections.Generic;
using UnityEngine;

public class EvolutionConfig {
    public List<GeneratedTexture> evolutionSource;
    public EvolutionType evolutionType;
    public InheritedSymmetryConfig inheritedSymmetryConfig;
    public Vector2Int offsetFromSource;

    public EvolutionConfig(List<GeneratedTexture> source, EvolutionType evolutionType, InheritedSymmetryConfig inheritedSymmetryConfig) {
        evolutionSource = source;
        this.evolutionType = evolutionType;
        this.inheritedSymmetryConfig = inheritedSymmetryConfig;
    }
}

public enum EvolutionType {
    noiseOffset
}
