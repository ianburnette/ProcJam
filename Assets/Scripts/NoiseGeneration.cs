using UnityEngine;

public class NoiseGeneration : MonoBehaviour {
    [Header("Debug")]
    [SerializeField] Vector2 origin;

    float maxFrequencySum = 1.1f;
    
    public Texture2D GetNoise(int frame, NoiseConfig noiseConfig, int spritePixelSize)
    {
        var tex = new Texture2D(spritePixelSize, spritePixelSize);
        //GenerateNoise();
        tex.SetPixels(CalcNoise(noiseConfig, frame, spritePixelSize));
        return tex;
    }


    Color[] CalcNoise(NoiseConfig config, int frame, int spritePixelSize) {
        var size = spritePixelSize;
        
        var colors = new Color[size * size];
        if (frame == 0) {
            origin = config.randomOrigin ? new Vector2(RandomValue(), RandomValue()) : config.manualOrigin;
        } else
            origin = new Vector2(origin.x + config.animationFrameNoiseOffset * frame, origin.y + config.animationFrameNoiseOffset * frame);

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

        float RandomValue()
        {
            return Random.Range(-config.randomOriginBound, config.randomOriginBound);
        }
        /*
        var newOctaves = new Octave[octaveCount];
        for (var octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) {
            newOctaves[octaveIndex] = new Octave(
                Mathf.Lerp(scaleRange.x, scaleRange.y, GetBiasAndGain()),
                //autoPerlinScale.Evaluate(OctaveCurvePosition(octaveIndex)),
                autoPerlinFrequency.Evaluate(OctaveCurvePosition(octaveIndex))
            );
            
            float GetBiasAndGain() {
                var curvePos = OctaveCurvePosition(octaveIndex);
                return (GetBias(curvePos, scaleBias) + GetGain(curvePos, scaleGain)) / 2f;
            }
        }
        */
    }
    //[SerializeField] int octaveCount;
    //[SerializeField] List<Octave> octaves;
    //[SerializeField] bool randomOrigin;
    //[SerializeField] float randomOriginBound = 255f;
    //[SerializeField] float animationFrameOffset = .2f;

    //[SerializeField] Vector2 scaleRange = new Vector2(5f, 45f);
    //[SerializeField] Vector2 frequencyRange = new Vector2(.8f, .001f);
    
    //[SerializeField] AnimationCurve autoPerlinScale = new AnimationCurve();
    //[SerializeField] AnimationCurve autoPerlinFrequency = new AnimationCurve();

    //[SerializeField, Range(.001f, .999f)] float scaleBias, scaleGain;
    
   // float OctaveCurvePosition(int octaveIndex, int octaveCount) {
   //     return Mathf.Clamp(octaveIndex / (octaveCount - 1f), 0f, 1f);
   // }

    static float ToColorVal(float val) => Mathf.Lerp(0, 255, val);
    
    /*
    void GenerateNoise() {
        var pos = OctaveCurvePosition(0);
        AddKeyToScaleCurve(pos, 0);
        AddKeyToFrequencyCurve(pos, 0);
        pos = OctaveCurvePosition(octaveCount);
        AddKeyToScaleCurve(pos, 1);
        AddKeyToFrequencyCurve(pos, 1);
        
        void AddKeyToScaleCurve(float position, int index) {
            var keyframe = new Keyframe(position, Mathf.Lerp(scaleRange.x, scaleRange.y, position));
            autoPerlinScale.AddKey(keyframe);
            AnimationUtility.SetKeyLeftTangentMode(autoPerlinScale, index, AnimationUtility.TangentMode.ClampedAuto);
        }
        
        void AddKeyToFrequencyCurve(float position, int index) {
            var keyframe = new Keyframe(position, Mathf.Lerp(frequencyRange.x, frequencyRange.y, position));
            autoPerlinFrequency.AddKey(keyframe);
            AnimationUtility.SetKeyLeftTangentMode(autoPerlinFrequency, index, AnimationUtility.TangentMode.ClampedAuto);
        }
    
    }
        */
   //float GetBias(float time, float bias) => time / ((1f/bias - 2f)*(1f - time)+1f);

   //float GetGain(float time, float gain) => 
   //    time < 0.5f ? GetBias(time * 2f, gain) / 2f : GetBias(time * 2f - 1f ,1f - gain) / 2f + 0.5f;
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
