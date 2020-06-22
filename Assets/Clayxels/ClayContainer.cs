
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
#endif

namespace Clayxels{
	public class Solid{
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
		public Vector3 scale = Vector3.one * 0.5f;
		public float blend = 0.0f;
		public Vector3 color = Vector3.one;
		public Vector4 attrs = Vector4.zero;
		public int primitiveType = 0;
		public int id = -1;
		public int clayObjectId = -1;
	}

	[ExecuteInEditMode]
	public class ClayContainer : MonoBehaviour{
		class ClayxelChunk{
			public ComputeBuffer pointCloudDataBuffer;
			public ComputeBuffer indirectDrawArgsBuffer;
			public Vector3 center = new Vector3();
			public Material clayxelMaterial;
			public Material clayxelPickingMaterial;

			#if DRAW_DEBUG
				public ComputeBuffer debugGridOutPointsBuffer;
			#endif
		}

		public int chunkSize = 8;
		public int chunksX = 1;
		public int chunksY = 1;
		public int chunksZ = 1;
		public float normalOrientedSplat = 1.0f;
		public Material customMaterial = null;
		public string meshAssetPath = "";

		static public bool globalDataNeedsInit = true;

		static List<string> solidsCatalogueLabels = new List<string>();
		static List<List<string[]>> solidsCatalogueParameters = new List<List<string[]>>();
		static ComputeShader claycoreCompute;
		static ComputeBuffer gridDataBuffer;
		static ComputeBuffer triangleConnectionTable;
		static ComputeBuffer prefilteredSolidIdsBuffer;
		static ComputeBuffer numSolidsPerChunkBuffer;
		
		static List<ComputeBuffer> globalCompBuffers = new List<ComputeBuffer>();
		static int lastUpdatedContainerId = -1;
		static int maxThreads = 8;
		static int maxSolids = 512;
		static int[] solidsInSingleChunkArray;
		
		public bool forceUpdate = false;
		public bool needsUpdate = true;
		
		static ComputeBuffer solidsPosBuffer;
		static ComputeBuffer solidsRotBuffer;
		static ComputeBuffer solidsScaleBuffer;
		static ComputeBuffer solidsBlendBuffer;
		static ComputeBuffer solidsTypeBuffer;
		static ComputeBuffer solidsColorBuffer;
		static ComputeBuffer solidsAttrsBuffer;
		static ComputeBuffer solidsClayObjectIdBuffer;
		static ComputeBuffer solidsUpdatedBuffer;
		static ComputeBuffer solidsPerChunkBuffer;
		
		static List<Vector3> solidsPos;
		static List<Quaternion> solidsRot;
		static List<Vector3> solidsScale;
		static List<float> solidsBlend;
		static List<int> solidsType;
		static List<Vector3> solidsColor;
		static List<Vector4> solidsAttrs;
		static List<int> solidsClayObjectId;

		static int inspectorUpdated;
		static int chunkMaxOutPoints = (256*256*256) / 8;
		static int[] tmpChunkData;
		
		bool memoryOptimized = false;
		[SerializeField]Material material = null;
		float globalSmoothing = 0.0f;
		Dictionary<int, int> solidsUpdatedDict = new Dictionary<int, int>();
		List<ClayxelChunk> chunks = new List<ClayxelChunk>();
		List<ComputeBuffer> compBuffers = new List<ComputeBuffer>();
		bool needsInit = true;
		bool invalidated = false;
		int[] countBufferArray = new int[1]{0};
		ComputeBuffer countBuffer;
		ComputeBuffer indirectChunkArgs1Buffer;
		ComputeBuffer indirectChunkArgs2Buffer;
		ComputeBuffer updateChunksBuffer;
		Vector3 boundsScale = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 boundsCenter = new Vector3(0.0f, 0.0f, 0.0f);
		Bounds renderBounds = new Bounds();
		Vector3[] vertices = new Vector3[1];
		int[] meshTopology = new int[1];
		bool solidsHierarchyNeedsScan = false;
		List<WeakReference> clayObjects = new List<WeakReference>();
		List<Solid> solids = new List<Solid>();
		int numChunks = 0;
		float deltaTime = 0.0f;
		bool meshCached = false;
		Mesh mesh = null;
		int numThreadsComputeStartRes;
		int numThreadsComputeFullRes;
		float splatRadius = 0.0f;
		int clayxelId = -1;
		
		static string renderPipe = "";
		static RenderTexture pickingRenderTexture = null;
		static RenderTargetIdentifier pickingRenderTextureId;
		static CommandBuffer pickingCommandBuffer;
		static Texture2D pickingTextureResult;
		static Rect pickingRect;
		static int pickingMousePosX = -1;
		static int pickingMousePosY = -1;
		static int pickedClayObjectId = -1;
		static int pickedClayxelId = -1;
		static GameObject pickedObj = null;
		static bool pickingMode = false;
		static bool pickingShiftPressed = false;

		enum Kernels{
			computeGrid,
			generatePointCloud,
			debugDisplayGridPoints,
			genMesh,
			filterSolidsPerChunk
		}

		public int getMaxClayObjects(){
			return ClayContainer.maxSolids;
		}

		public void scanClayObjectsHierarchy(){
			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();
			this.solids.Clear();

			this.scanRecursive(this.transform);

			this.solidsHierarchyNeedsScan = false;

			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);
			}
		}

		public void updatedSolidCount(){
			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);
			}
			else{
				for(int i = 0; i < this.solids.Count; ++i){
					Solid solid = this.solids[i];
					solid.id = i;
					
					this.solidsUpdatedDict[solid.id] = 1;
				}
			}
		}

		public int getNumClayObjects(){
			return  this.clayObjects.Count;
		}

		static public void initGlobalData(){
			if(!ClayContainer.globalDataNeedsInit){
				return;
			}

			string renderPipeAsset = "";
			if(GraphicsSettings.renderPipelineAsset != null){
				renderPipeAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;
			}
			
			if(renderPipeAsset == "HDRenderPipelineAsset"){
				ClayContainer.renderPipe = "hdrp";
			}
			else if(renderPipeAsset == "UniversalRenderPipelineAsset"){
				ClayContainer.renderPipe = "urp";
			}
			else{
				ClayContainer.renderPipe = "builtin";
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer.setupScenePicking();
					ClayContainer.pickingMode = false;
					ClayContainer.pickedObj = null;
				}

				ClayContainer.reloadSolidsCatalogue();
			#endif

			ClayContainer.globalDataNeedsInit = false;

			ClayContainer.lastUpdatedContainerId = -1;

			ClayContainer.releaseGlobalBuffers();

			UnityEngine.Object clayCore = Resources.Load("clayCoreLock");
			if(clayCore == null){
				clayCore = Resources.Load("clayCore");
			}

			ClayContainer.claycoreCompute = (ComputeShader)Instantiate(clayCore);

			ClayContainer.gridDataBuffer = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.gridDataBuffer);

			ClayContainer.prefilteredSolidIdsBuffer = new ComputeBuffer(64 * 64 * 64, sizeof(int) * 128);
			ClayContainer.globalCompBuffers.Add(ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			// ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearGrid, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			
			ClayContainer.triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.triangleConnectionTable);

			ClayContainer.triangleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

			int numKernels = Enum.GetNames(typeof(Kernels)).Length;
			for(int i = 0; i < numKernels; ++i){
				ClayContainer.claycoreCompute.SetBuffer(i, "gridData", ClayContainer.gridDataBuffer);
			}
			
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "triangleConnectionTable", ClayContainer.triangleConnectionTable);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.genMesh, "triangleConnectionTable", ClayContainer.triangleConnectionTable);

			ClayContainer.solidsPosBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPosBuffer);
			ClayContainer.solidsRotBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsRotBuffer);
			ClayContainer.solidsScaleBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsScaleBuffer);
			ClayContainer.solidsBlendBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsBlendBuffer);
			ClayContainer.solidsTypeBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsTypeBuffer);
			ClayContainer.solidsColorBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsColorBuffer);
			ClayContainer.solidsAttrsBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsAttrsBuffer);
			ClayContainer.solidsClayObjectIdBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsClayObjectIdBuffer);

			ClayContainer.solidsPos = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsRot = new List<Quaternion>(new Quaternion[ClayContainer.maxSolids]);
			ClayContainer.solidsScale = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsBlend = new List<float>(new float[ClayContainer.maxSolids]);
			ClayContainer.solidsType = new List<int>(new int[ClayContainer.maxSolids]);
			ClayContainer.solidsColor = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			ClayContainer.solidsAttrs = new List<Vector4>(new Vector4[ClayContainer.maxSolids]);
			ClayContainer.solidsClayObjectId = new List<int>(new int[ClayContainer.maxSolids]);

			ClayContainer.numSolidsPerChunkBuffer = new ComputeBuffer(64, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);

			ClayContainer.solidsUpdatedBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsUpdatedBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsUpdated", ClayContainer.solidsUpdatedBuffer);

			int maxChunks = 64;
			ClayContainer.solidsPerChunkBuffer = new ComputeBuffer(maxChunks, sizeof(int) * ClayContainer.maxSolids);
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);

			ClayContainer.solidsInSingleChunkArray = new int[ClayContainer.maxSolids];
			for(int i = 0; i < ClayContainer.maxSolids; ++i){
				ClayContainer.solidsInSingleChunkArray[i] = i;
			}

			ClayContainer.tmpChunkData = new int[ClayContainer.chunkMaxOutPoints * 4];

			#if DRAW_DEBUG
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.debugDisplayGridPoints, "gridData", ClayContainer.gridDataBuffer);
			#endif
		}

		public void init(){
			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.reinstallEditorEvents();
				}
			#endif

			if(ClayContainer.globalDataNeedsInit){
				ClayContainer.initGlobalData();
			}

			this.needsInit = false;

			this.memoryOptimized = false;

			if(this.gameObject.GetComponent<MeshFilter>() != null){
				this.meshCached = true;
				this.releaseBuffers();
				return;
			}

			this.limitChunkValues();

			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();

			this.releaseBuffers();

			this.numThreadsComputeStartRes = 64 / ClayContainer.maxThreads;
			this.numThreadsComputeFullRes = 256 / ClayContainer.maxThreads;

			this.splatRadius = (((float)this.chunkSize / 256) * 0.5f) * 1.8f;

			this.initChunks();

			this.countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
			this.compBuffers.Add(this.countBuffer);

			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
			ClayContainer.lastUpdatedContainerId = -1;

			this.initMaterialProperties();

			this.scanClayObjectsHierarchy();
			this.computeClay();

			if(this.clayObjects.Count > 0){
				this.optimizeMemory();
			}
		}

		public ClayObject addClayObject(){
			GameObject clayObj = new GameObject("clay_cube+");
			clayObj.transform.parent = this.transform;
			clayObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			ClayObject clayObjComp = clayObj.AddComponent<ClayObject>();
			clayObjComp.clayxelContainerRef = new WeakReference(this);
			clayObjComp.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			this.solidsHierarchyNeedsScan = true;

			return clayObjComp;
		}

		public ClayObject getClayObject(int id){
			return (ClayObject)this.clayObjects[id].Target;
		}

		public void scheduleClayObjectsScan(){
			this.solidsHierarchyNeedsScan = true;
		}

		public void forceUpdateAllChunks(){
			if(this.numChunks == 1){
				return;
			}

			for(int i = 0; i < this.solids.Count; ++i){
				this.solidsUpdatedDict[this.solids[i].id] = 1;
			}
		}

		public void solidUpdated(int id){
			if(this.numChunks > 1){
				this.solidsUpdatedDict[id] = 1;
			}

			this.needsUpdate = true;
		}

		public List<Solid> getSolids(){
			return this.solids;
		}

		public void clayObjectUpdated(ClayObject clayObj){
			if(!this.transform.hasChanged){
				if(this.numChunks > 1){
					for(int i = 0; i < clayObj.getNumSolids(); ++i){
						this.solidsUpdatedDict[clayObj.getSolid(i).id] = 1;
					}
				}

				this.needsUpdate = true;
			}
		}
		
		public Material getMaterial(){
			return this.material;
		}

		public void updatedMaterialProperties(){
			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
					ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
					string name = this.material.shader.GetPropertyName(propertyId);

					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						chunk.clayxelMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						chunk.clayxelMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						chunk.clayxelMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}
		}

		// if this container should not receive any more editing of its ClayObjects, 
		// this method will resize the memory used by this container to make it weight just as a frozen mesh
		public void optimizeMemory(){
			if(this.memoryOptimized){
				return;
			}

			this.memoryOptimized = true;

			int[] indirectDrawBufferData = new int[4]{0, 0, 0, 0};

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.indirectDrawArgsBuffer.GetData(indirectDrawBufferData);
				
				int pointCount = indirectDrawBufferData[0];

				int bufferId = this.compBuffers.IndexOf(chunk.pointCloudDataBuffer);

				chunk.pointCloudDataBuffer.GetData(ClayContainer.tmpChunkData, 0, 0, pointCount *4);
				chunk.pointCloudDataBuffer.Release();
				chunk.pointCloudDataBuffer = null;
				
				if(pointCount == 0){
					pointCount = 1;
				}

				chunk.pointCloudDataBuffer = new ComputeBuffer(pointCount, sizeof(int) * 4);
				this.compBuffers[bufferId] = chunk.pointCloudDataBuffer;

				chunk.pointCloudDataBuffer.SetData(ClayContainer.tmpChunkData, 0, 0, pointCount * 4);

				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
			}
		}

		void expandMemory(){
			this.memoryOptimized = false;

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				
				int bufferId = this.compBuffers.IndexOf(chunk.pointCloudDataBuffer);

				chunk.pointCloudDataBuffer.Release();	
				
				chunk.pointCloudDataBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
				this.compBuffers[bufferId] = chunk.pointCloudDataBuffer;

				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
			}
		}

		public void computeClay(){
			this.needsUpdate = false;

			if(this.memoryOptimized){
				this.expandMemory();
			}
			
			if(this.solidsHierarchyNeedsScan){
				this.scanClayObjectsHierarchy();
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			this.updateSolids();
			
			if(this.numChunks == 1){
				this.computeChunk(0);
			}
			else{
				for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
					this.computeChunk(chunkIt);
				}
			}
		}

		public bool shouldRetopoMesh = false;
		public int retopoMaxVerts = -1;

		public void generateMesh(){
			if(this.needsInit){
				this.init();
			}

			this.meshCached = true;

			if(this.gameObject.GetComponent<MeshFilter>() == null){
				this.gameObject.AddComponent<MeshFilter>();
			}
			
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				render = this.gameObject.AddComponent<MeshRenderer>();

				if(ClayContainer.renderPipe == "hdrp"){
					render.material = new Material(Shader.Find("Clayxels/ClayxelHDRPMeshShader"));
				}
				else if(ClayContainer.renderPipe == "urp"){
					render.material = new Material(Shader.Find("Clayxels/ClayxelURPMeshShader"));
				}
				else{
					render.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInMeshShader"));
				}
			}

			ComputeBuffer meshIndicesBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints * 6, sizeof(float) * 3, ComputeBufferType.Counter);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.genMesh, "meshOutIndices", meshIndicesBuffer);

			ComputeBuffer meshVertsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 3, ComputeBufferType.Counter);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.genMesh, "meshOutPoints", meshVertsBuffer);

			ComputeBuffer meshColorsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 4);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.genMesh, "meshOutColors", meshColorsBuffer);

			List<Vector3> totalVertices = new List<Vector3>();
			List<int> totalIndices = new List<int>();
			List<Color> totalColors = new List<Color>();

			int totalNumVerts = 0;

			this.mesh = new Mesh();
			this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			this.forceUpdateAllChunks();
			this.switchComputeData();

			this.updateSolids();

			if(this.shouldRetopoMesh){
				ClayContainer.claycoreCompute.SetInt("retopo", 1);
			}
			else{
				ClayContainer.claycoreCompute.SetInt("retopo", 0);
			}

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayxelChunk chunk = this.chunks[chunkIt];

				meshIndicesBuffer.SetCounterValue(0);
				meshVertsBuffer.SetCounterValue(0);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);

				// ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearGrid, "indirectDrawArgs", chunk.indirectDrawArgsBuffer);
				// ClayContainer.claycoreCompute.Dispatch((int)Kernels.clearGrid, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes);
				
				ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeGrid, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes, this.numThreadsComputeStartRes);

				ClayContainer.claycoreCompute.SetInt("outMeshIndexOffset", totalNumVerts);
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.genMesh, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes);//64, 64, 64);

				int numVerts = this.getBufferCount(meshVertsBuffer);
				int numQuads = this.getBufferCount(meshIndicesBuffer) * 3;
				
				totalNumVerts += numVerts;
				
				Vector3[] vertices = new Vector3[numVerts];
				meshVertsBuffer.GetData(vertices);

				int[] indices = new int[numQuads];
				meshIndicesBuffer.GetData(indices);

				Color[] colors = new Color[numVerts];
				meshColorsBuffer.GetData(colors);

				totalVertices.AddRange(vertices);
				totalIndices.AddRange(indices);
				totalColors.AddRange(colors);
			}

			mesh.vertices = totalVertices.ToArray();
			mesh.triangles = totalIndices.ToArray();
			mesh.colors = totalColors.ToArray();
			
			this.mesh.Optimize();

			this.mesh.RecalculateNormals();
			
			this.gameObject.GetComponent<MeshFilter>().mesh = this.mesh;

			meshIndicesBuffer.Release();
			meshVertsBuffer.Release();
			meshColorsBuffer.Release();

			this.releaseBuffers();
		}

		public void transferMaterialPropertiesToMesh(){
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				return;
			}
			
			for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
				ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
				string name = this.material.shader.GetPropertyName(propertyId);
				
				if(render.sharedMaterial.shader.FindPropertyIndex(name) != -1){
					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						render.sharedMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						render.sharedMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						render.sharedMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}

		}

		public bool hasCachedMesh(){
			return this.meshCached;
		}

		public void disableMesh(){
			this.meshCached = false;
			this.needsInit = true;

			if(this.gameObject.GetComponent<MeshFilter>() != null){
				DestroyImmediate(this.gameObject.GetComponent<MeshFilter>());
			}
		}

		static void parseSolidsAttrs(string content, ref int lastParsed){
			string[] lines = content.Split(new[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for(int i = 0; i < lines.Length; ++i){
				string line = lines[i];
				if(line.Contains("label: ")){
					if(line.Split('/').Length == 3){// if too many comment slashes, it's a commented out solid,
						lastParsed += 1;

						string[] parameters = line.Split(new[]{"label:"}, StringSplitOptions.None)[1].Split(',');
						string label = parameters[0].Trim();
						
						ClayContainer.solidsCatalogueLabels.Add(label);

						List<string[]> paramList = new List<string[]>();

						for(int paramIt = 1; paramIt < parameters.Length; ++paramIt){
							string param = parameters[paramIt];
							string[] attrs = param.Split(':');
							string paramId = attrs[0];
							string[] paramLabelValue = attrs[1].Split(' ');
							string paramLabel = paramLabelValue[1];
							string paramValue = paramLabelValue[2];

							paramList.Add(new string[]{paramId.Trim(), paramLabel.Trim(), paramValue.Trim()});
						}

						ClayContainer.solidsCatalogueParameters.Add(paramList);
					}
				}
			}
		}

		static public void reloadSolidsCatalogue(){
			ClayContainer.solidsCatalogueLabels.Clear();
			ClayContainer.solidsCatalogueParameters.Clear();

			int lastParsed = -1;
			try{
				string claySDF = ((TextAsset)Resources.Load("claySDF", typeof(TextAsset))).text;
				ClayContainer.parseSolidsAttrs(claySDF, ref lastParsed);

				string numThreadsDef = "MAXTHREADS";
				ClayContainer.maxThreads = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(numThreadsDef) + numThreadsDef.Length + 1]);
			}
			catch{
				Debug.Log("error trying to parse parameters in claySDF.compute, solid #" + lastParsed);
			}
		}

		public string[] getSolidsCatalogueLabels(){
			return ClayContainer.solidsCatalogueLabels.ToArray();
		}

		public List<string[]> getSolidsCatalogueParameters(int solidId){
			return ClayContainer.solidsCatalogueParameters[solidId];
		}

		void OnDestroy(){
			this.invalidated = true;

			this.releaseBuffers();

			if(UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length == 0){
				ClayContainer.releaseGlobalBuffers();
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.removeEditorEvents();
				}
			#endif
		}

		void releaseBuffers(){
			for(int i = 0; i < this.compBuffers.Count; ++i){
				this.compBuffers[i].Release();
			}

			this.compBuffers.Clear();
		}

		static void releaseGlobalBuffers(){
			for(int i = 0; i < ClayContainer.globalCompBuffers.Count; ++i){
				ClayContainer.globalCompBuffers[i].Release();
			}

			ClayContainer.globalCompBuffers.Clear();
		}

		void limitChunkValues(){
			if(this.chunksX > 4){
				this.chunksX = 4;
			}
			if(this.chunksY > 4){
				this.chunksY = 4;
			}
			if(this.chunksZ > 4){
				this.chunksZ = 4;
			}
			if(this.chunksX < 1){
				this.chunksX = 1;
			}
			if(this.chunksY < 1){
				this.chunksY = 1;
			}
			if(this.chunksZ < 1){
				this.chunksZ = 1;
			}

			if(this.chunkSize < 4){
				this.chunkSize = 4;
			}
		}

		void initChunks(){
			this.numChunks = 0;
			this.chunks.Clear();

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			for(int z = 0; z < this.chunksZ; ++z){
				for(int y = 0; y < this.chunksY; ++y){
					for(int x = 0; x < this.chunksX; ++x){
						this.initNewChunk(x, y, z);
						this.numChunks += 1;
					}
				}
			}

			this.updateChunksBuffer = new ComputeBuffer(this.numChunks, sizeof(int));
			this.compBuffers.Add(this.updateChunksBuffer);

			this.indirectChunkArgs1Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs1Buffer);

			this.indirectChunkArgs2Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs2Buffer);

			int[] indirectChunk1 = new int[this.numChunks * 3];
			int[] indirectChunk2 = new int[this.numChunks * 3];

			int indirectChunkSize1 = 64 / ClayContainer.maxThreads;
			int indirectChunkSize2 = 256 / ClayContainer.maxThreads;
			if(this.numChunks > 1){
				indirectChunkSize1 = 0;
				indirectChunkSize2 = 0;
			}

			int[] updateChunks = new int[this.numChunks];

			for(int i = 0; i < this.numChunks; ++i){
				int indirectChunkId = i * 3;
				indirectChunk1[indirectChunkId] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 1] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 2] = indirectChunkSize1;

				indirectChunk2[indirectChunkId] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 1] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 2] = indirectChunkSize2;

				updateChunks[i] = 1;
			}

			this.updateChunksBuffer.SetData(updateChunks);
			this.indirectChunkArgs1Buffer.SetData(indirectChunk1);
			this.indirectChunkArgs2Buffer.SetData(indirectChunk2);
		}

		void initNewChunk(int x, int y, int z){
			ClayxelChunk chunk = new ClayxelChunk();
			this.chunks.Add(chunk);

			float seamOffset = this.chunkSize / 256.0f; // removes the seam between chunks
			float chunkOffset = this.chunkSize - seamOffset;
			float gridCenterOffset = (this.chunkSize * 0.5f);
			chunk.center = new Vector3(
				(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * x),
				(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * y),
				(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * z));

			chunk.pointCloudDataBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 4);
			this.compBuffers.Add(chunk.pointCloudDataBuffer);

			chunk.indirectDrawArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(chunk.indirectDrawArgsBuffer);

			chunk.indirectDrawArgsBuffer.SetData(new int[]{0, 1, 0, 0});

			chunk.clayxelPickingMaterial = new Material(Shader.Find("Clayxels/ClayxelPickingShader"));
			chunk.clayxelPickingMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);

			// #if DRAW_DEBUG
			// 	chunk.clayxelMaterial = new Material(Shader.Find("ClayContainer/ClayxelDebugShader"));

			// 	chunk.debugGridOutPointsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(float) * 3, ComputeBufferType.Counter);
			// 	this.compBuffers.Add(chunk.debugGridOutPointsBuffer);

			// 	chunk.clayxelMaterial.SetBuffer("debugChunkPoints", chunk.debugGridOutPointsBuffer);
			// #endif
		}

		void initMaterialProperties(){
			if(this.customMaterial != null){
				this.material = this.customMaterial;
				
				if(ClayContainer.renderPipe == "hdrp"){
					this.material.hideFlags = HideFlags.HideAndDontSave;// required in hdrp
				}
			}
			else if(this.material == null){
				if(ClayContainer.renderPipe == "hdrp"){
					this.material = new Material(Shader.Find("Clayxels/ClayxelHDRPShader"));
					this.material.hideFlags = HideFlags.HideAndDontSave;// required in hdrp
				}
				else if(ClayContainer.renderPipe == "urp"){
					this.material = new Material(Shader.Find("Clayxels/ClayxelURPShader"));
				}
				else{
					this.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInShader"));
				}
			}

			if(this.customMaterial == null){
				// set the default clayxel texture to a dot on the standard material
				Texture texture = this.material.GetTexture("_MainTex");
				if(texture == null){
					this.material.SetTexture("_MainTex", (Texture)Resources.Load("clayxelDot"));
				}
			}

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.clayxelMaterial = new Material(this.material);
				
				chunk.clayxelMaterial.SetInt("solidHighlightId", -1);
				chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
				chunk.clayxelMaterial.SetFloat("chunkSize", (float)this.chunkSize);
				chunk.clayxelMaterial.SetVector("chunkCenter",  chunk.center);
			}
		}

		void scanRecursive(Transform trn){
			ClayObject clayObj = trn.gameObject.GetComponent<ClayObject>();
			if(clayObj != null){
				if(clayObj.isValid() && trn.gameObject.activeSelf){
					this.collectClayObject(clayObj);
				}
			}

			for(int i = 0; i < trn.childCount; ++i){
				this.scanRecursive(trn.GetChild(i));
			}
		}

		void collectClayObject(ClayObject clayObj){
			if(clayObj.getNumSolids() == 0){
				clayObj.init();
			}

			clayObj.clayObjectId = this.clayObjects.Count;
			this.clayObjects.Add(new WeakReference(clayObj));

			for(int i = 0; i < clayObj.getNumSolids(); ++i){
				Solid solid = clayObj.getSolid(i);
				solid.id = this.solids.Count;
				solid.clayObjectId = clayObj.clayObjectId;
				this.solids.Add(solid);

				this.solidsUpdatedDict[solid.id] = 1;
			}

			clayObj.transform.hasChanged = true;
			clayObj.setClayxelContainer(this);
		}

		int getBufferCount(ComputeBuffer buffer){
			ComputeBuffer.CopyCount(buffer, this.countBuffer, 0);
			this.countBuffer.GetData(this.countBufferArray);
			int count = this.countBufferArray[0];

			return count;
		}

		#if DRAW_DEBUG
		void debugGridPoints(ClayxelChunk chunk){
			chunk.debugGridOutPointsBuffer.SetCounterValue(0);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.debugDisplayGridPoints, "debugGridOutPoints", chunk.debugGridOutPointsBuffer);
			ClayContainer.claycoreCompute.Dispatch((int)Kernels.debugDisplayGridPoints, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes, this.numThreadsComputeFullRes);
		}
		#endif

		void computeChunk(int chunkId){
			ClayxelChunk chunk = this.chunks[chunkId];

			uint indirectChunkId = sizeof(int) * ((uint)chunkId * 3);

			ClayContainer.claycoreCompute.SetInt("chunkId", chunkId);

			ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "indirectDrawArgs", chunk.indirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGrid, this.indirectChunkArgs1Buffer, indirectChunkId);
			
			#if DRAW_DEBUG
			this.debugGridPoints(chunk);
			return;
			#endif
			
			// generate point cloud
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "indirectDrawArgs", chunk.indirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "pointCloudData", chunk.pointCloudDataBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.generatePointCloud, this.indirectChunkArgs2Buffer, indirectChunkId);
		}

		void updateSolids(){
			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];

				int clayObjId = solid.clayObjectId;
				if(solid.clayObjectId > -1){
					ClayObject clayObj = (ClayObject)this.clayObjects[solid.clayObjectId].Target;
					clayObj.pullUpdate();
				}
				else{
					clayObjId = 0;
				}

				float blend = solid.blend;
				if(blend < 0.0f){
					blend = blend - this.globalSmoothing;
				}
				
				ClayContainer.solidsPos[i] = solid.position;
				ClayContainer.solidsRot[i] = solid.rotation;
				ClayContainer.solidsScale[i] = solid.scale;
				ClayContainer.solidsBlend[i] = blend;
				ClayContainer.solidsType[i] = solid.primitiveType;
				ClayContainer.solidsColor[i] = solid.color;
				ClayContainer.solidsAttrs[i] = solid.attrs;
				ClayContainer.solidsClayObjectId[i] = clayObjId;
			}

			if(this.solids.Count > 0){
				ClayContainer.solidsPosBuffer.SetData(ClayContainer.solidsPos);
				ClayContainer.solidsRotBuffer.SetData(ClayContainer.solidsRot);
				ClayContainer.solidsScaleBuffer.SetData(ClayContainer.solidsScale);
				ClayContainer.solidsBlendBuffer.SetData(ClayContainer.solidsBlend);
				ClayContainer.solidsTypeBuffer.SetData(ClayContainer.solidsType);
				ClayContainer.solidsColorBuffer.SetData(ClayContainer.solidsColor);
				ClayContainer.solidsAttrsBuffer.SetData(ClayContainer.solidsAttrs);
				ClayContainer.solidsClayObjectIdBuffer.SetData(ClayContainer.solidsClayObjectId);
			}

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			if(this.numChunks > 1){
				ClayContainer.claycoreCompute.SetInt("numSolidsUpdated", this.solidsUpdatedDict.Count);
				ClayContainer.solidsUpdatedBuffer.SetData(this.solidsUpdatedDict.Keys.ToArray());
				
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.filterSolidsPerChunk, this.chunksX, this.chunksY, this.chunksZ);
				
				this.solidsUpdatedDict.Clear();
			}
		}

		void logFPS(){
			this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;
			float fps = 1.0f / this.deltaTime;
			Debug.Log(fps);
		}

		void switchComputeData(){
			ClayContainer.lastUpdatedContainerId = this.GetInstanceID();

			int numKernels = Enum.GetNames(typeof(Kernels)).Length;
			for(int i = 0; i < numKernels; ++i){
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsPos", ClayContainer.solidsPosBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsRot", ClayContainer.solidsRotBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsScale", ClayContainer.solidsScaleBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsBlend", ClayContainer.solidsBlendBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsType", ClayContainer.solidsTypeBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsColor", ClayContainer.solidsColorBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsAttrs", ClayContainer.solidsAttrsBuffer);
				ClayContainer.claycoreCompute.SetBuffer(i, "solidsClayObjectId", ClayContainer.solidsClayObjectIdBuffer);
			}

			if(this.chunkSize < 10){
				this.globalSmoothing = this.splatRadius * 2.0f;
			}
			else{
				this.globalSmoothing = this.splatRadius;
			}

			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);

			if(this.numChunks == 1){
				this.countBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.countBufferArray);

				ClayContainer.solidsPerChunkBuffer.SetData(ClayContainer.solidsInSingleChunkArray);
			}
			else{
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "updateChunks", this.updateChunksBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs1", this.indirectChunkArgs1Buffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs2", this.indirectChunkArgs2Buffer);
			}
		}

		void Update(){
			if(this.meshCached || this.invalidated){
				return;
			}

			if(this.needsInit){
				this.init();
			}
			else{
				// inhibit updates if this transform is the trigger
				if(this.transform.hasChanged){
					this.needsUpdate = false;
					this.transform.hasChanged = false;

					// if this transform moved and also one of the solids moved, then we still need to update
					if(this.forceUpdate){
						this.needsUpdate = true;
					}
				}
			}

			if(!this.needsUpdate){
				this.drawClayxels();
				return;
			}
			
			this.computeClay();

			this.drawClayxels();

			#if CLAYXELS_GPU_FIX1
				// this dummy getData fixes a driver error on some lower end GPUS
				this.countBuffer.GetData(this.countBufferArray);
			#endif
		}

		void drawChunk(int chunkId, float splatSize){
			ClayxelChunk chunk = this.chunks[chunkId];

			// #if DRAW_DEBUG 
			// 	int pnts = this.getBufferCount(chunk.debugGridOutPointsBuffer);
			// 	Graphics.DrawProcedural(chunk.clayxelMaterial, 
			// 		this.renderBounds,
			// 		MeshTopology.Points, pnts, 1);
			// 	return;

			chunk.clayxelMaterial.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
			chunk.clayxelMaterial.SetFloat("splatRadius", splatSize);

			#if UNITY_EDITOR
				// update some properties of the material only while in editor to avoid disappearing clayxels on certain editor events
				if(!Application.isPlaying){
					this.updateMaterialInEditor(chunk, splatSize);
				}
			#endif

			Graphics.DrawProceduralIndirect(chunk.clayxelMaterial, 
				this.renderBounds,
				MeshTopology.Triangles, chunk.indirectDrawArgsBuffer, 0,
				null, null,
				ShadowCastingMode.TwoSided, true, this.gameObject.layer);
		}

		void drawClayxels(){
			if(this.needsInit){
				return;
			}

			this.renderBounds.center = this.transform.position;
			this.renderBounds.size = this.boundsScale;

			float splatSize = this.splatRadius * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			if(this.numChunks == 1){
				this.drawChunk(0, splatSize);
			}
			else{
				for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
					this.drawChunk(chunkIt, splatSize);
				}
			}
		}

		#if UNITY_EDITOR
		void Awake(){
			if(!Application.isPlaying){
				// this is needed to trigger a re-init after playing in editor
				ClayContainer.globalDataNeedsInit = true;
				this.needsInit = true;
			}
		}

		void updateMaterialInEditor(ClayxelChunk chunk, float splatSize){
			if(ClayContainer.pickedClayxelId == this.clayxelId){
				chunk.clayxelMaterial.SetInt("solidHighlightId", ClayContainer.pickedClayObjectId);
			}
			else{
				chunk.clayxelMaterial.SetInt("solidHighlightId", -1);
			}

			chunk.clayxelMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
			chunk.clayxelMaterial.SetFloat("chunkSize", (float)this.chunkSize);
			chunk.clayxelMaterial.SetVector("chunkCenter",  chunk.center);
			
			chunk.clayxelPickingMaterial.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
			chunk.clayxelPickingMaterial.SetFloat("splatRadius",  splatSize);
			chunk.clayxelPickingMaterial.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
			chunk.clayxelPickingMaterial.SetFloat("chunkSize", (float)this.chunkSize);
			chunk.clayxelPickingMaterial.SetVector("chunkCenter",  chunk.center);
		}

		[MenuItem("GameObject/3D Object/Clayxel Container" )]
		public static ClayContainer createNewContainer(){
			 GameObject newObj = new GameObject("ClayxelContainer");
			 ClayContainer newClayContainer = newObj.AddComponent<ClayContainer>();

			 UnityEditor.Selection.objects = new GameObject[]{newObj};

			 return newClayContainer;
		}

		bool editingThisContainer = false;

		void OnValidate(){
			// called when editor value on this object is changed
			this.numChunks = 0;
		}

		void removeEditorEvents(){
			AssemblyReloadEvents.beforeAssemblyReload -= this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged -= this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged -= this.onSelectionChanged;

			Undo.undoRedoPerformed -= this.onUndoPerformed;
		}

		void reinstallEditorEvents(){
			this.removeEditorEvents();

			AssemblyReloadEvents.beforeAssemblyReload += this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged += this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged += this.onSelectionChanged;

			Undo.undoRedoPerformed += this.onUndoPerformed;

		}

		void onBeforeAssemblyReload(){
			// called when this script recompiles

			if(Application.isPlaying){
				return;
			}

			this.releaseBuffers();
			ClayContainer.releaseGlobalBuffers();

			ClayContainer.globalDataNeedsInit = true;
			this.needsInit = true;
		}

		void onUndoPerformed(){
			if(Undo.GetCurrentGroupName() == "changed clayobject" ||
				Undo.GetCurrentGroupName() == "changed clayxel container"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "changed clayxel grid"){
				this.init();
			}
			else if(Undo.GetCurrentGroupName() == "added clayxel solid"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "Selection Change"){
				if(!UnityEditor.Selection.Contains(this.gameObject)){
					if(UnityEditor.Selection.gameObjects.Length > 0){
						ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
						if(clayObj != null){
							if(clayObj.getClayContainer() == this){
								this.needsUpdate = true;
							}
						}
					}
				}
			}
		}

		void onHierarchyChanged(){
			if(this.meshCached){
				return;
			}

			if(this.invalidated){
				// scene is being cleared
				return;
			}
			
			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
			this.onSelectionChanged();
			
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			ClayContainer.getSceneView().Repaint();
			#if DEBUG_CLAYXEL_REPAINT_WARN
			Debug.Log("onHierarchyChanged!");
			#endif
		}

		public static void inspectorUpdate(){
			ClayContainer.inspectorUpdated = UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length;
		}

		void onSelectionChanged(){
			// for some reason this callback is also triggered by the inspector
			// so we first have to check if this is really a selection change or an inspector update. wtf. 

			if(ClayContainer.inspectorUpdated > 0){
				ClayContainer.inspectorUpdated -= 1;
				return;
			}

			if(this.invalidated){
				return;
			}

			if(this.meshCached){
				return;
			}

			bool wasEditingThis = this.editingThisContainer;
			this.editingThisContainer = false;
			if(UnityEditor.Selection.Contains(this.gameObject)){
				// check if this container got selected
				this.editingThisContainer = true;
			}

			if(!this.editingThisContainer){
				// check if one of the clayObjs in container has been selected
				for(int i = 0; i < this.clayObjects.Count; ++i){
					ClayObject clayObj = (ClayObject)this.clayObjects[i].Target;

					if(clayObj != null){
						if(UnityEditor.Selection.Contains(clayObj.gameObject)){
							this.editingThisContainer = true;
							return;
						}
					}
				}

				if(wasEditingThis){// if we're changing selection, optimize the buffers of this container
					this.optimizeMemory();
				}
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}
		}

		static void setupScenePicking(){
			SceneView sceneView = (SceneView)SceneView.sceneViews[0];
			SceneView.duringSceneGui -= ClayContainer.onSceneGUI;
			SceneView.duringSceneGui += ClayContainer.onSceneGUI;

			ClayContainer.pickingCommandBuffer = new CommandBuffer();
			
			ClayContainer.pickingTextureResult = new Texture2D(1, 1, TextureFormat.ARGB32, false);

			ClayContainer.pickingRect = new Rect(0, 0, 1, 1);

			if(ClayContainer.pickingRenderTexture != null){
				ClayContainer.pickingRenderTexture.Release();
				ClayContainer.pickingRenderTexture = null;
			}

			ClayContainer.pickingRenderTexture = new RenderTexture(1024, 768, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			ClayContainer.pickingRenderTexture.Create();
			ClayContainer.pickingRenderTextureId = new RenderTargetIdentifier(ClayContainer.pickingRenderTexture);
		}

		public static void startPicking(){
			ClayContainer.pickingMode = true;
			ClayContainer.pickedObj = null;

			ClayContainer.getSceneView().Repaint();
		}

		static void clearPicking(){
			ClayContainer.pickingMode = false;
			ClayContainer.pickedObj = null;
			ClayContainer.pickedClayxelId = -1;
			ClayContainer.pickedClayObjectId = -1;

			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
		}

		static void onSceneGUI(SceneView sceneView){
			if(Application.isPlaying){
				return;
			}

			if(!UnityEditorInternal.InternalEditorUtility.isApplicationActive){
				// this callback keeps running even in the background
				return;
			}

			Event ev = Event.current;

			if(ev.isKey){
				if(ev.keyCode == KeyCode.P){
					ClayContainer.startPicking();
				}

				return;
			}
			
			if(!ClayContainer.pickingMode){
				return;
			}
			
			if(ClayContainer.pickedObj != null){
				if(ClayContainer.pickingShiftPressed){
					List<UnityEngine.Object> sel = new List<UnityEngine.Object>();
	   			for(int i = 0; i < UnityEditor.Selection.objects.Length; ++i){
	   				sel.Add(UnityEditor.Selection.objects[i]);
	   			}
	   			sel.Add(ClayContainer.pickedObj);
	   			UnityEditor.Selection.objects = sel.ToArray();
	   		}
	   		else{
					UnityEditor.Selection.objects = new GameObject[]{ClayContainer.pickedObj};
				}
			}
			
			if(ev.type == EventType.MouseMove){
				ClayContainer.pickingMousePosX = (int)ev.mousePosition.x;
				ClayContainer.pickingMousePosY = (int)ev.mousePosition.y;
				
				if(ClayContainer.pickedObj != null){
					ClayContainer.clearPicking();
				}
			}
			else if(ev.type == EventType.MouseDown && !ev.alt){
				if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
					ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
					ClayContainer.clearPicking();
					return;
				}

				ev.Use();

				if(ClayContainer.pickedClayxelId > -1 && ClayContainer.pickedClayObjectId > -1){
					ClayContainer[] clayxels = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
					GameObject newSel = clayxels[ClayContainer.pickedClayxelId].getClayObject(ClayContainer.pickedClayObjectId).gameObject;
					UnityEditor.Selection.objects = new GameObject[]{newSel};

					ClayContainer.pickedObj = newSel;
					ClayContainer.pickingShiftPressed = ev.shift;
				}
				else{
					ClayContainer.clearPicking();
				}
			}
			else if((int)ev.type == 7){ // on repaint
				if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
					ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
					return;
				}

				ClayContainer.pickedClayObjectId = -1;
		  		ClayContainer.pickedClayxelId = -1;

				ClayContainer.pickingCommandBuffer.Clear();
				ClayContainer.pickingCommandBuffer.SetRenderTarget(ClayContainer.pickingRenderTextureId);
				ClayContainer.pickingCommandBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

				ClayContainer[] clayxels = UnityEngine.Object.FindObjectsOfType<ClayContainer>();

				for(int i = 0; i < clayxels.Length; ++i){
					ClayContainer clayxel = clayxels[i];
					if(!clayxel.meshCached){
						clayxels[i].drawClayxelPicking(i, ClayContainer.pickingCommandBuffer);
					}
				}

				Graphics.ExecuteCommandBuffer(ClayContainer.pickingCommandBuffer);
				
				ClayContainer.pickingRect.Set(
					(int)(1024.0f * ((float)ClayContainer.pickingMousePosX / (float)sceneView.camera.pixelWidth)), 
					(int)(768.0f * ((float)ClayContainer.pickingMousePosY / (float)sceneView.camera.pixelHeight)), 
					1, 1);

				RenderTexture oldRT = RenderTexture.active;
				RenderTexture.active = ClayContainer.pickingRenderTexture;
				ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
				ClayContainer.pickingTextureResult.Apply();
				RenderTexture.active = oldRT;
				
				Color pickCol = ClayContainer.pickingTextureResult.GetPixel(0, 0);
				
				int pickId = (int)((pickCol.r + pickCol.g * 255.0f + pickCol.b * 255.0f) * 255.0f);
		  		ClayContainer.pickedClayObjectId = pickId - 1;
		  		ClayContainer.pickedClayxelId = (int)(pickCol.a * 255.0f) - 1;
			}

			ClayContainer.getSceneView().Repaint();
		}

		void drawClayxelPicking(int clayxelId, CommandBuffer pickingCommandBuffer){
			if(this.needsInit){
				return;
			}

			this.clayxelId = clayxelId;

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayxelChunk chunk = this.chunks[chunkIt];

				chunk.clayxelPickingMaterial.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
				chunk.clayxelPickingMaterial.SetInt("clayxelId", clayxelId);

				pickingCommandBuffer.DrawProceduralIndirect(Matrix4x4.identity, chunk.clayxelPickingMaterial, -1, 
					MeshTopology.Triangles, chunk.indirectDrawArgsBuffer);
			}
		}

		void OnDrawGizmos(){
			if(Application.isPlaying){
				return;
			}

			if(!this.editingThisContainer){
				return;
			}

			Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 0.1f);
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(this.boundsCenter, this.boundsScale);

			// debug chunks
			// Vector3 boundsScale2 = new Vector3(this.chunkSize, this.chunkSize, this.chunkSize);
			// for(int i = 0; i < this.numChunks; ++i){
			// 	Gizmos.DrawWireCube(this.chunks[i].center, boundsScale2);
			// }
		}

		static public void reloadAll(){
			ClayContainer.globalDataNeedsInit = true;

			ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < clayxelObjs.Length; ++i){
				clayxelObjs[i].init();
			}
			
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			((SceneView)SceneView.sceneViews[0]).Repaint();
		}

		public static SceneView getSceneView(){
			return (SceneView)SceneView.sceneViews[0];
		}

		#if CLAYXELS_ONEUP
			public void retopoMesh(){
				RetopoUtils.retopoMesh(this.mesh, this.retopoMaxVerts, -1);
			}
		#else
			public void retopoMesh(){
				Debug.Log("");
			}
		#endif
		
		public void storeMesh(){
			AssetDatabase.CreateAsset(this.mesh, "Assets/" + this.meshAssetPath + ".mesh");
			AssetDatabase.SaveAssets();
			
			UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/" + this.meshAssetPath + ".mesh");
			for(int i = 0; i < data.Length; ++i){
				if(data[i].GetType() == typeof(Mesh)){
					this.mesh = (Mesh)data[i];
					this.gameObject.GetComponent<MeshFilter>().mesh = this.mesh;

					break;
				}
			}
		}

		#endif// end if UNITY_EDITOR
	}
}
