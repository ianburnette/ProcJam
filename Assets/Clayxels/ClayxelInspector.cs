
#if UNITY_EDITOR // exclude from build

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

using Clayxels;

namespace Clayxels{
	[CustomEditor(typeof(ClayContainer))]
	public class ClayxelInspector : Editor{
		// Color newColorValue;
		// Vector4 newVectorValue;
		// float newFloatValue;
		// Texture2D newTextureValue;
		// string changedMaterialProperty = "";
		// ShaderPropertyType changedMaterialPropertyType;

		public override void OnInspectorGUI(){
			ClayContainer clayxel = (ClayContainer)this.target;

			EditorGUILayout.LabelField("Clayxels, V0.71 beta");
			EditorGUILayout.LabelField("clayObjects: " + clayxel.getNumClayObjects());

			#if !CLAYXELS_ONEUP
				EditorGUILayout.LabelField("itch.io limit: 64 clayObjects");
			#else
				EditorGUILayout.LabelField("limit is " + clayxel.getMaxClayObjects());
			#endif

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			int chunkSize = EditorGUILayout.IntField(new GUIContent("chunk size", "Small number means more detail in a smaller grid, big number results in less detail with a bigger grid. Enable Gizmos in your viewport to see the boundaries."), clayxel.chunkSize);
			Vector3Int gridSize = EditorGUILayout.Vector3IntField(new GUIContent("chunks", "Make the grid bigger by employing multiple chunks. The more chunks you have, the slower the computation."), new Vector3Int(clayxel.chunksX, clayxel.chunksY, clayxel.chunksZ));
			
			if(EditorGUI.EndChangeCheck()){
				ClayContainer.inspectorUpdate();

				clayxel.chunkSize = chunkSize;
				clayxel.chunksX = gridSize.x;
				clayxel.chunksY = gridSize.y;
				clayxel.chunksZ = gridSize.z;

				clayxel.init();
				clayxel.needsUpdate = true;
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				return;
			}

			EditorGUILayout.Space();

			clayxel.forceUpdate = EditorGUILayout.Toggle(new GUIContent("update always", "Force this container to update on every frame, use it if you're moving the container as well as the clayObjects inside it."), clayxel.forceUpdate);

			if(GUILayout.Button((new GUIContent("reload all", "This is necessary after you make changes to the shader or to the claySDF file.")))){
				ClayContainer.reloadAll();
			}

			if(GUILayout.Button(new GUIContent("pick clay (p)", "Press p on your keyboard to mouse pick ClayObjects from the viewport. Pressing Shift will add to a previous selection."))){
				ClayContainer.startPicking();
			}

			if(GUILayout.Button(new GUIContent("add clay", "lets get this party started"))){
				ClayObject clayObj = ((ClayContainer)this.target).addClayObject();

				Undo.RegisterCreatedObjectUndo(clayObj.gameObject, "added clayxel solid");
				UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};

				clayxel.needsUpdate = true;
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				return;
			}

			EditorGUILayout.Space();

			#if CLAYXELS_ONEUP
				clayxel.shouldRetopoMesh = EditorGUILayout.Toggle(new GUIContent("retopology", "(Experimental) use this to save better meshes on disk"), clayxel.shouldRetopoMesh);
				if(clayxel.shouldRetopoMesh){
					clayxel.retopoMaxVerts = EditorGUILayout.IntField(new GUIContent("max verts", "-1 will let the tool decide on the best number of vertices."), clayxel.retopoMaxVerts);
				}
			#endif

			clayxel.meshAssetPath = EditorGUILayout.TextField(new GUIContent("save asset", "Specify an asset name to store the computed mesh on disk. Files are saved relative to this project's Assets folder."), clayxel.meshAssetPath);
			string[] paths = clayxel.meshAssetPath.Split('.');
			if(paths.Length > 0){
				clayxel.meshAssetPath = paths[0];
			}

			if(!clayxel.hasCachedMesh()){
				if(GUILayout.Button(new GUIContent("freeze to mesh", "Switch between live clayxels and a frozen mesh that will not be updated."))){
					clayxel.generateMesh();

					if(clayxel.shouldRetopoMesh){
						clayxel.retopoMesh();
					}
					
					clayxel.transferMaterialPropertiesToMesh();

					if(clayxel.meshAssetPath != ""){
						clayxel.storeMesh();
					}
				}
			}
			else{
				if(GUILayout.Button(new GUIContent("defrost clayxels", "Back to live clayxels."))){
					clayxel.disableMesh();
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();
				}
			}


			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			Material customMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("customMaterial", "Custom materials will need to use shaders that talk to clayxels. Use the provided shaders as reference, the Asset Store version also comes with editable Amplify shaders. "), clayxel.customMaterial, typeof(Material), false);
			
			if(EditorGUI.EndChangeCheck()){
				ClayContainer.inspectorUpdate();
				
				Undo.RecordObject(this.target, "changed clayxel container");

				if(customMaterial != clayxel.customMaterial){
					clayxel.customMaterial = customMaterial;
					clayxel.init();
				}

				clayxel.needsUpdate = true;
				clayxel.forceUpdateAllChunks();
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();
			}

			if(!clayxel.hasCachedMesh()){
				this.inspectMaterial(clayxel);
			}
		}

		MaterialEditor materialEditor = null;
		void inspectMaterial(ClayContainer clayContainer){
			EditorGUI.BeginChangeCheck();

			if(this.materialEditor != null){
				DestroyImmediate(this.materialEditor);
				this.materialEditor = null;
			}

			this.materialEditor = (MaterialEditor)CreateEditor(clayContainer.getMaterial());

			if(this.materialEditor != null){
				this.materialEditor.DrawHeader();
				this.materialEditor.OnInspectorGUI();
			}
			
			if(EditorGUI.EndChangeCheck()){
				clayContainer.updatedMaterialProperties();
				
				if(this.materialEditor != null){
					DestroyImmediate(this.materialEditor);
					this.materialEditor = null;
				}
			}
		}
	}

	[CustomEditor(typeof(ClayObject)), CanEditMultipleObjects]
	public class ClayObjectInspector : Editor{
		
		public override void OnInspectorGUI(){
			ClayObject clayObj = (ClayObject)this.targets[0];
			ClayContainer clayxel = clayObj.getClayContainer();
			
			EditorGUI.BeginChangeCheck();

			ClayObject.ClayObjectMode mode = (ClayObject.ClayObjectMode)EditorGUILayout.EnumPopup("mode", clayObj.mode);
			
			if(EditorGUI.EndChangeCheck()){
				clayObj.setMode(mode);

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}

			if(clayObj.mode == ClayObject.ClayObjectMode.offset){
				this.drawOffsetMode(clayObj);
			}
			else if(clayObj.mode == ClayObject.ClayObjectMode.spline){
				this.drawSplineMode(clayObj);
			}

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			float blend = EditorGUILayout.FloatField("blend", clayObj.blend);
			if(blend > 1.0f){
				blend = 1.0f;
			}
			else if(blend < -1.0f){
				blend = -1.0f;
			}

			Color color = EditorGUILayout.ColorField("color", clayObj.color);
			
			string[] solidsLabels = clayxel.getSolidsCatalogueLabels();
	 		int primitiveType = EditorGUILayout.Popup("solidType", clayObj.primitiveType, solidsLabels);

	 		Dictionary<string, float> paramValues = new Dictionary<string, float>();
	 		paramValues["x"] = clayObj.attrs.x;
	 		paramValues["y"] = clayObj.attrs.y;
	 		paramValues["z"] = clayObj.attrs.z;
	 		paramValues["w"] = clayObj.attrs.w;

	 		List<string[]> parameters = clayxel.getSolidsCatalogueParameters(primitiveType);
	 		List<string> wMaskLabels = new List<string>();
	 		for(int paramIt = 0; paramIt < parameters.Count; ++paramIt){
	 			string[] parameterValues = parameters[paramIt];
	 			string attr = parameterValues[0];
	 			string label = parameterValues[1];
	 			string defaultValue = parameterValues[2];

	 			if(primitiveType != clayObj.primitiveType){
	 				// reset to default params when changing primitive type
	 				paramValues[attr] = float.Parse(defaultValue, CultureInfo.InvariantCulture);
	 			}
	 			
	 			if(attr.StartsWith("w")){
	 				wMaskLabels.Add(label);
	 			}
	 			else{
	 				paramValues[attr] = EditorGUILayout.FloatField(label, paramValues[attr]);
	 			}
	 		}

	 		if(wMaskLabels.Count > 0){
	 			paramValues["w"] = (float)EditorGUILayout.MaskField("options", (int)clayObj.attrs.w, wMaskLabels.ToArray());
	 		}

	 		if(EditorGUI.EndChangeCheck()){
	 			ClayContainer.inspectorUpdate();

	 			Undo.RecordObjects(this.targets, "changed clayobject");

	 			for(int i = 1; i < this.targets.Length; ++i){
	 				bool somethingChanged = false;
	 				ClayObject currentClayObj = (ClayObject)this.targets[i];

	 				bool shouldAutoRename = false;

	 				if(clayObj.blend != blend){
	 					currentClayObj.blend = blend;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.color != color){
	 					currentClayObj.color = color;
	 					somethingChanged = true;
	 				}
					
	 				if(clayObj.primitiveType != primitiveType){

	 					currentClayObj.primitiveType = primitiveType;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.attrs.x != paramValues["x"]){
	 					currentClayObj.attrs.x = paramValues["x"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.y != paramValues["y"]){
	 					currentClayObj.attrs.y = paramValues["y"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.z != paramValues["z"]){
	 					currentClayObj.attrs.z = paramValues["z"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.w != paramValues["w"]){
	 					currentClayObj.attrs.w = paramValues["w"];
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(somethingChanged){
	 					currentClayObj.getClayContainer().clayObjectUpdated(currentClayObj);

	 					if(shouldAutoRename){
		 					if(currentClayObj.gameObject.name.StartsWith("clay_")){
		 						this.autoRename(currentClayObj, solidsLabels);
		 					}
		 				}
	 				}
				}

	 			clayObj.blend = blend;
	 			clayObj.color = color;
	 			clayObj.primitiveType = primitiveType;
	 			clayObj.attrs.x = paramValues["x"];
	 			clayObj.attrs.y = paramValues["y"];
	 			clayObj.attrs.z = paramValues["z"];
	 			clayObj.attrs.w = paramValues["w"];

	 			if(clayObj.gameObject.name.StartsWith("clay_")){
					this.autoRename(clayObj, solidsLabels);
				}

				clayObj.forceUpdate();
	 			
	 			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
	 			ClayContainer.getSceneView().Repaint();
			}
		}

		void drawSplineMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();

			int subdivs = EditorGUILayout.IntField("subdivs", clayObj.splineSubdiv);

			var list = this.serializedObject.FindProperty("splinePoints");
			EditorGUILayout.PropertyField(list, new GUIContent("spline points"), true);

			if(EditorGUI.EndChangeCheck()){
				this.serializedObject.ApplyModifiedProperties();

				clayObj.splineSubdiv = subdivs;
				
				clayObj.updateSplineSetup();

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}

		void drawOffsetMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();
				
			int numSolids = EditorGUILayout.IntField("solids", clayObj.getNumSolids());
			bool allowSceneObjects = true;
			clayObj.offsetter = (GameObject)EditorGUILayout.ObjectField("offsetter", clayObj.offsetter, typeof(GameObject), allowSceneObjects);
			
			if(EditorGUI.EndChangeCheck()){
				if(numSolids < 1){
					numSolids = 1;
				}
				else if(numSolids > 100){
					numSolids = 100;
				}

				clayObj.setOffsetNum(numSolids);
				
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}

		void autoRename(ClayObject clayObj, string[] solidsLabels){
			string blendSign = "+";
			if(clayObj.blend < 0.0f){
				blendSign = "-";
			}

			string isColoring = "";
			if(clayObj.attrs.w == 1.0f){
				blendSign = "";
				isColoring = "[paint]";
			}

			clayObj.gameObject.name = "clay_" + solidsLabels[clayObj.primitiveType] + blendSign + isColoring;
		}
	}
}

#endif // end if UNITY_EDITOR
