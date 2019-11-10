using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(Controls))]
public class MapGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var myScript = (Controls)target;
        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate")) myScript.Generate();
        if(GUILayout.Button("Reset")) myScript.Reset();
    }
}

public class Controls : MonoBehaviour
{
    [SerializeField] Transform spriteParent;
    [SerializeField] GameObject spritePrefab;
    [SerializeField] SpriteGeneration spriteGeneration;
    [SerializeField] int imageGridSize;
    [SerializeField] GridLayoutGroup gridLayoutGroup;

    [Header("Configuration")] 
    [SerializeField] Configuration configuration;

    public void Generate()
    {
        Reset();
        SetUpGridLayoutGroup();
        
        for (var i = 0; i < imageGridSize * imageGridSize; i++)
        {
            var sprite = Instantiate(spritePrefab, spriteParent);
            var frameAnimation = sprite.GetComponent<FrameAnimation>();
            frameAnimation.FrameTime = configuration.timeBetweenFrames;     
            frameAnimation.Frames = spriteGeneration.Generate(configuration);
        }

        void SetUpGridLayoutGroup()
        {
            var maxSize = Screen.height / (float) imageGridSize;
            gridLayoutGroup.cellSize = new Vector2(maxSize, maxSize);
            gridLayoutGroup.constraintCount = imageGridSize;
        }
    }

    public void Reset()
    {
        for (var i = spriteParent.childCount - 1; i > -1; i--) 
            DestroyImmediate(spriteParent.GetChild(i).gameObject);
    }
}

[Serializable]
public class Configuration {
    [Header("Scaling")]
    public ScalingMode scalingMode;
    //TODO: second scaling mode
    
    [Header("Animation")]
    [Range(1, 8)] public int animationFrameCount;
    public float timeBetweenFrames;
    public AnimationMode animationMode;

    [Header("Outline")] 
    public bool outlineEnabled;
    public bool applyOutlineAfterScaling;

    [Header("Cleanup")] 
    public bool allowPixelsOnEdgeOfSprite;
    public LonePixelEvaluationMode lonePixelEvaluationMode;
    [Range(0f,1f)] public float chanceToDeleteLonePixels;
}

public enum ScalingMode { none, x2, x4, eagle2, eagle3 }
public enum AnimationMode { loop, pingPong }
public enum LonePixelEvaluationMode { CardinalDirectionsOnly, IncludeDiagonals }