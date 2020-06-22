
Shader "Clayxels/ClayxelPickingShader" {
	SubShader {
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		Pass {
			Lighting Off

			ZWrite On     
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "clayxelSRPUtils.cginc"

			// StructuredBuffer<int4> chunkPoints;

			// float4x4 objectMatrix;
			// float3 chunkCenter;
			// float chunkSize = 0.0;
			// float splatRadius = 0.01;
			int clayxelId = 0;

			struct VertexData{
				float4 pos: POSITION;
				float2 tex: TEXCOORD0;
				nointerpolation float2 solidId: TEXCOORD1;
			};

			struct FragData{
				fixed4 selection: SV_TARGET;
			};

			VertexData vert(uint id : SV_VertexID){
				// init a blank vertex, we might discard it before to shade it
				VertexData outVertex;
				outVertex.pos = float4(0, 0, 0, 0);
				outVertex.tex = float2(0, 0);
				outVertex.solidId = float2(0, 0);
				
				int4 clayxelPointData = chunkPoints[id / 3];
				
				float3 normal = mul(objectMatrix, unpackNormal(clayxelPointData.z));

				float3 viewVec = float3(unity_CameraToWorld[0][2], unity_CameraToWorld[1][2], unity_CameraToWorld[2][2]);
				float incidence = dot(normal, viewVec);
				if(incidence > 0.5){ // discard vertex if normal is facing away from camera
					return outVertex;
				}

				// in order to shade the point cloud coming from the compute shader, we need to decompress the data.
				int4 compressedData = unpackInt4(clayxelPointData.x);

				float cellSize = chunkSize / 256;
				float3 cellLocalOffset = unpackFloat3(clayxelPointData.y) * cellSize;
				float3 pointPos = expandGridPoint(compressedData.xyz, cellSize, chunkSize) + cellLocalOffset + chunkCenter;
				
				// expand verts to billboard
				uint vertexOffset = id % 3;
				float3 upVec = float3(unity_CameraToWorld[0][1], unity_CameraToWorld[1][1], unity_CameraToWorld[2][1]) * splatRadius;
				float3 sideVec = float3(unity_CameraToWorld[0][0], unity_CameraToWorld[1][0], unity_CameraToWorld[2][0]) * (splatRadius * 2.0);

				float4 p = mul(objectMatrix, float4(pointPos, 1.0));
				// expandSplatVertexClipped(vertexOffset, p, camUpVec, camSideVec, outVertex.pos, outVertex.tex);
				if(vertexOffset == 0){
					outVertex.pos = UnityObjectToClipPos(float4(p + ((-upVec) + sideVec), 1.0));
					outVertex.tex = float2(-0.5, 0.0);
				}
				else if(vertexOffset == 1){
					outVertex.pos = UnityObjectToClipPos(float4(p + ((-upVec) - sideVec), 1.0));
					outVertex.tex = float2(1.5, 0.0);
				}
				else if(vertexOffset == 2){
					outVertex.pos = UnityObjectToClipPos(float4(p + (upVec*1.7), 1.0));
					outVertex.tex = float2(0.5, 1.35);
				}

				outVertex.solidId = float2(compressedData.w, 0);

				return outVertex;
			}

			FragData frag(VertexData inVertex){
				if(length(inVertex.tex-0.5) > 0.5){// if outside circle
					discard;
				}

				FragData outData;
				float clayxelIdA = float(clayxelId + 1) / 255.0;
				outData.selection = float4(unpackRgb(uint(inVertex.solidId.x)), clayxelIdA);
				
				return outData;
			}

			ENDCG
		}
	}
}