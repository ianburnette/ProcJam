using System.Collections.Generic;
using UnityEngine;

public class NoiseGeneration : MonoBehaviour {
    [SerializeField] int octaveCount;
    [SerializeField] List<Octave> octaves;
    [SerializeField] Vector2 origin;
    [SerializeField] bool randomOrigin;
    [SerializeField] float randomOriginBound = 255f;
    [SerializeField] float animationFrameOffset = .2f;

    [SerializeField] Vector2 scaleRange = new Vector2(5f, 45f);
    [SerializeField] float frequencyStart = .8f;

    [SerializeField] AnimationCurve perlinBias;
    [SerializeField] AnimationCurve perlinGain;
    
    public Texture2D GetNoise(int size, int frame)
    {
        var tex = new Texture2D(size, size);
        GenerateNoise();
        tex.SetPixels(CalcNoise(size, frame));
        return tex;
    }

    void GenerateNoise() {
        List<Keyframe> biases;
        //var octaveDistance = scaleRange.x 
        for (var i = 0; i < octaveCount; i++) {
          //  biases.Add(new Keyframe());
        }
        
        
        //perlinBias = new AnimationCurve([]);
        
        float GetBias(float time, float bias) => time / ((1f/bias - 2f)*(1f - time)+1f);

        float GetGain(float time, float gain) => 
            time < 0.5f ? GetBias(time * 2f, gain) / 2f : GetBias(time * 2f - 1f ,1f - gain) / 2f + 0.5f;
    }

    Color[] CalcNoise(int size, int frame)
    {
        var colors = new Color[size * size];
        if (frame == 0) {
            if (randomOrigin) 
                origin = new Vector2(RandomValue(), RandomValue());
        } else
            origin = new Vector2(origin.x + animationFrameOffset * frame, origin.y + animationFrameOffset * frame);

        foreach (var octave in octaves)
        {
            for (var row = 0f; row < size; row++)
            {
                for (var column = 0f; column < size; column++)
                {
                    var xCoordinate = origin.x + row / size * octave.scale;
                    var yCoordinate = origin.y + column / size * octave.scale;
                    var sample = Mathf.PerlinNoise(xCoordinate, yCoordinate);
                    colors[(int)column * size + (int)row] += new Color(sample, sample, sample) * octave.frequency;
                }
            }
        }

        return colors;

        float RandomValue()
        {
            return Random.Range(-randomOriginBound, randomOriginBound);
        }
    }
    
    static float ToColorVal(float val) => Mathf.Lerp(0, 255, val);
}

[System.Serializable]
public class Octave
{
    public float scale;
    public float frequency;
}