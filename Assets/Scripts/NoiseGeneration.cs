using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class NoiseGeneration : MonoBehaviour {
    [Header("Debug")]
    [SerializeField] Vector2 origin;
    [SerializeField] Vector3 origin3D;
    [SerializeField] float noisePerEvolutionStep = .1f;

    float maxFrequencySum = 1.1f;
    
    public Texture2D GetNoise(NoiseConfig noiseConfig, int spritePixelSize, Vector2 desiredOrigin)
    {
        var tex = new Texture2D(spritePixelSize, spritePixelSize);
        tex.SetPixels(CalcNoise(noiseConfig, spritePixelSize, spritePixelSize, desiredOrigin));
        return tex;
    }

    public Texture3D GetNoise3D(NoiseConfig noiseConfig, Vector3Int voxelSize, ref Vector2 generatedVoxelOrigin) {
        var modelData = new Texture3D(voxelSize.x, voxelSize.y, voxelSize.z, DefaultFormat.LDR,
            TextureCreationFlags.None);
        var colors = new Color[voxelSize.x * voxelSize.y * voxelSize.z];
        // generate a 2D texture at every vertical layer of the 3D texture, stacking them on top of each other
        for (var i = 0; i < modelData.width; i++) {
            var currentLayerOrigin = GetOrigin3D(noiseConfig, i);
            var noise = CalcNoise(noiseConfig, voxelSize.y,voxelSize.z, currentLayerOrigin);
            var colorStartIndex = i * noise.Length;
            noise.CopyTo(colors, colorStartIndex);
            if (i == 0) generatedVoxelOrigin = currentLayerOrigin;
        }
        modelData.SetPixels(colors);
        return modelData;
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
            origin = new Vector2(
                origin.x + config.animationFrameNoiseOffset * frame,
                origin.y + config.animationFrameNoiseOffset * frame);

        return origin;
    }

    public Vector3 GetOrigin3D(NoiseConfig config, int depthLayer) {
        if (depthLayer == 0) {
            origin3D = new Vector3(
                RandomValue(config.randomOriginBound),
                RandomValue(config.randomOriginBound),
                RandomValue(config.randomOriginBound));
        } else {
            origin3D = new Vector3(
                origin3D.x + config.animationFrameNoiseOffset * depthLayer,
                origin3D.y + config.animationFrameNoiseOffset * depthLayer,
                origin3D.z + config.animationFrameNoiseOffset * depthLayer);
        }
        return origin3D;
    }
    
    float RandomValue(float randomBound) => Random.Range(-randomBound, randomBound);

    Color[] CalcNoise(NoiseConfig config, int pixelWidth, int pixelHeight, Vector2 origin) {
        var colors = new Color[pixelHeight * pixelWidth];

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
            for (var row = 0f; row < pixelHeight; row++) {
                for (var column = 0f; column < pixelWidth; column++) {
                    var xCoordinate = origin.x + row / pixelWidth * config.octaves[octaveIndex].scale;
                    var yCoordinate = origin.y + column / pixelHeight * config.octaves[octaveIndex].scale;
                    var sample = Mathf.PerlinNoise(xCoordinate, yCoordinate);
                    colors[(int) column * pixelHeight + (int) row] += new Color(sample, sample, sample) * frequencies[octaveIndex];
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
