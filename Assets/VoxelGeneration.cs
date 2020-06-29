using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[CustomEditor(typeof(VoxelGeneration))]
public class VoxelGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {    
        var myScript = (VoxelGeneration)target;
        GUILayout.Label("Editor Controls", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate")) myScript.Generate();
        if(GUILayout.Button("Clear")) myScript.Clear();
        DrawDefaultInspector();
    }
}
#endif

public class VoxelGeneration : MonoBehaviour {
    [SerializeField] SpriteGeneration spriteGeneration;
    [SerializeField] ConfigurationAsset configuration;

    [SerializeField] GameObject voxelPrefab;
    public void Generate() {
        var voxelModel = spriteGeneration.GenerateVoxelModel(configuration);
        StartCoroutine(SpawnVoxels(voxelModel));
    }

    public void Clear() {
        for (var i = transform.childCount - 1; i >= 0; i--) {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            } else {
                Destroy(transform.GetChild(i).gameObject);
            }
            #else
            Destroy(transform.GetChild(i).gameObject);
            yield return new WaitForSecondsRealtime(0.001f);
            #endif
        }
    }

    IEnumerator SpawnVoxels(GeneratedVoxelModel voxelModel) {
        var tex = voxelModel.modelData;
        for (var i = 0; i < tex.depth; i++) {
            for (var j = 0; j < tex.width; j++) {
                for (var k = 0; k < tex.height; k++) {
                    var voxel = GameObject.Instantiate(voxelPrefab, transform);
                    voxel.transform.localPosition = new Vector3(i, j, k);
                    voxel.GetComponent<Renderer>().material.color = tex.GetPixel(i, j, k);
                    yield return new WaitForSecondsRealtime(0.001f);
                }
            }
        }
    }
}
