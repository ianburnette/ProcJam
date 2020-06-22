using UnityEngine;

public class NoiseGeneration : MonoBehaviour {
    [Header("Debug")]
    [SerializeField] Vector2 origin;
    [SerializeField] float noisePerEvolutionStep = .1f;

    float maxFrequencySum = 1.1f;
    
    public Texture2D GetNoise(NoiseConfig noiseConfig, int spritePixelSize, Vector2 desiredOrigin)
    {
        var tex = new Texture2D(spritePixelSize, spritePixelSize);
        tex.SetPixels(CalcNoise(noiseConfig, spritePixelSize, desiredOrigin));
        return tex;
    }

    public Vector2 GetOriginWithOffset(int frame, NoiseConfig config, EvolutionConfig evolutionConfig) {
        var sourceOriginThisFrame = evolutionConfig.evolutionSource[frame].origin;
        origin = new Vector2(sourceOriginThisFrame.x + (evolutionConfig.offsetFromSource.x * noisePerEvolutionStep),
         sourceOriginThisFrame.y + (evolutionConfig.offsetFromSource.y * noisePerEvolutionStep));
        return origin;
    }
    
    public Vector2 GetOrigin(int frame, NoiseConfig config) {
        if (frame == 0) {
            origin = config.randomOrigin ? new Vector2(RandomValue(config.randomOriginBound), 
                RandomValue(config.randomOriginBound)) : config.manualOrigin;
        } else
            origin = new Vector2(origin.x + config.animationFrameNoiseOffset * frame, origin.y + config.animationFrameNoiseOffset * frame);

        return origin;
    }
    
    float RandomValue(float randomBound) => Random.Range(-randomBound, randomBound);

    Color[] CalcNoise(NoiseConfig config, int spritePixelSize, Vector2 origin) {
        var size = spritePixelSize;
        var colors = new Color[size * size];

        var frequencies = new float[config.octaves.Count];
        if (config.randomizeFrequency) {
            var totalFrequency = 0f;
            while (totalFrequency < .9f) {
                var availableFrequency = maxFrequencySum;
                for (int i = 0; i < config.octaves.Count; i++) {
                    var frequency = Random.Range(0, availableFrequency);
                    frequencies[i] = frequency;
                    availableFrequency -= frequency;
                    totalFrequency += frequency;
                }
            }
        } else {
            for (int i = 0; i < config.octaves.Count; i++)
                frequencies[i] = config.octaves[i].frequency;
        }
        
        for (var octaveIndex = 0; octaveIndex < config.octaves.Count; octaveIndex++) {
            for (var row = 0f; row < size; row++) {
                for (var column = 0f; column < size; column++) {
                    var xCoordinate = origin.x + row / size * config.octaves[octaveIndex].scale;
                    var yCoordinate = origin.y + column / size * config.octaves[octaveIndex].scale;
                    var sample = Mathf.PerlinNoise(xCoordinate, yCoordinate);
                    colors[(int) column * size + (int) row] += new Color(sample, sample, sample) * frequencies[octaveIndex];
                }
            }
        }

        return colors;
    }
}

[System.Serializable]
public class Octave
{
    public float scale;
    public float frequency;

    public Octave(float scale, float frequency) {
        this.scale = scale;
        this.frequency = frequency;
    }
}
