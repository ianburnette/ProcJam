using System;
using Clayxels;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(ClaySpriteConstructor))]
public class ClaySpriteConstructorEditor : Editor
{
    public override void OnInspectorGUI()
    {    
        var myScript = (ClaySpriteConstructor)target;
        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate")) myScript.Generate();
        if(GUILayout.Button("Clear")) myScript.Clear();
        DrawDefaultInspector();
    }
}
#endif


public class ClaySpriteConstructor : MonoBehaviour {
    [SerializeField] Texture2D baseSprite;
    [SerializeField] GameObject clayObjectPrefab;
    [SerializeField] float positioningScale = 0.0625f;
    [SerializeField] Vector3 positioningOffset = new Vector3(-1f, -1f, 0f);
    [SerializeField, Range(.07f,1f)] float objectSize = 0.2f;
    [SerializeField] float blend = 0.2f;
    [SerializeField, Range(0,6)] int primitiveType;
    
    float objectSizeLastFrame;
    float blendLastFrame;
    int primitiveTypeLastFrame;
    
    public void Clear() {
        for (var i = transform.childCount - 1; i >= 0; i--) {
            var child = transform.GetChild(i).gameObject;
            try {
                DestroyImmediate(child);
            }
            catch (InvalidOperationException _) {
                child.SetActive(false);
            }
        }
    }
    
    public void Generate() {
        Clear();
        if (baseSprite == null) return;
        if (!baseSprite.isReadable) {
            Debug.LogError("Texture not set to Read/Write");
        }
        for (var w = 0; w < baseSprite.width; w++) {
            for (var h = 0; h < baseSprite.height; h++) {
                var pixelColor = baseSprite.GetPixel(w, h);
                if(pixelColor==Color.black) continue;
                
                var clayPixel = Instantiate(clayObjectPrefab, transform);
                var clayObject = clayPixel.GetComponent<ClayObject>();
                clayObject.color = pixelColor;
                clayObject.blend = blend;
                clayPixel.transform.position = (new Vector3(w, h, 0) * positioningScale) + positioningOffset;
                clayPixel.transform.localScale = Vector3.one * objectSize;
            }
        }
        //ClayContainer.reloadAll();
    }

    void SetObjectProperties() {
        for (var i = transform.childCount - 1; i >= 0; i--) {
            var clayObject = transform.GetChild(i).GetComponent<ClayObject>();
            clayObject.blend = blend;
            clayObject.transform.localScale = Vector3.one * objectSize;
            clayObject.primitiveType = primitiveType;
        }
        //ClayContainer.reloadAll();
    }

    void OnValidate() {
        if (blendLastFrame != blend || objectSizeLastFrame != objectSize || primitiveTypeLastFrame != primitiveType) SetObjectProperties();
        blendLastFrame = blend;
        objectSizeLastFrame = objectSize;
        primitiveTypeLastFrame = primitiveType;
    }
}
