﻿using System;
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

    void OnEnable() => Generate();

    [SerializeField] GameObject voxelPrefab;
    public void Generate() {
        Clear();
        var voxelModel = spriteGeneration.GenerateVoxelModel(configuration);
        SpawnVoxels(voxelModel);
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

    void SpawnVoxels(GeneratedVoxelModel voxelModel) {
        var tex = voxelModel.modelData;
        for (var x = 0; x < tex.width; x++) {
            for (var z = 0; z < tex.depth; z++) {
                for (var y = 0; y < tex.height; y++) {
                    var voxel = GameObject.Instantiate(voxelPrefab, transform);
                    voxel.transform.localPosition = new Vector3(x, y, z);
                    voxel.GetComponent<Voxel>().SetColor(tex.GetPixel(x, y ,z));
                }
            }
        }
    }
}
