// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Clayxels/ClayxelBuiltInShader"
{
	Properties {
		_Smoothness ("Smoothness", Range(0.0,1.0)) = 0.5
		_Metallic ("Metallic", Range(0.0,1.0)) = 0.0
		[HDR]_Emission ("Emission", Color) = (0, 0, 0, 0)
		_ClayxelSize ("Clayxel Size", Range(0.0, 100.0)) = 1.0
		_NormalOrient ("Normal Orient", Range(0.0, 1.0)) = 1.0
		[NoScaleOffset]_MainTex ("Texture", 2D) = "defaulttexture" {}
		_Cutoff ("Cutoff", Range(0.0, 1.0)) = 0.95
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		ZWrite On
		Cull Back
		
		CGPROGRAM

		// #pragma multi_compile __ DUMMY

		#pragma surface surf Standard vertex:vert addshadow fullforwardshadows alphatest:_Cutoff
		#pragma target 3.0

		#include "UnityCG.cginc"
		
		#include "../clayxelSRPUtils.cginc"

		float _Smoothness;
		float _Metallic;
		float4 _Emission;
		float _ClayxelSize;
		float _NormalOrient;
		sampler2D _MainTex;
		// float _Cutoff = 0.9;

		struct VertexData{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float4 color : COLOR;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			uint vid : SV_VertexID;
		};

		struct Input
		{
			float2 tex : TEXCOORD0;
			float4 color : COLOR;
		};

		void vert(inout VertexData outVertex, out Input outData){
			UNITY_INITIALIZE_OUTPUT(Input, outData);

			float4 tex;
			
			clayxelVertNormalBlend(outVertex.vid, _ClayxelSize, _NormalOrient, tex, outVertex.color.xyz, outVertex.vertex.xyz, outVertex.normal);

			outVertex.color.w = 1.0;
			outVertex.vertex.w = 1.0;
			outData.tex = tex.xy;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float alphaTexture = tex2D(_MainTex, IN.tex).a;
			o.Alpha = alphaTexture;
			// if(alphaTexture < 1.0){
			// 	discard;
			// }

			// if(length(IN.tex - 0.5) > 0.5){// if outside circle
			// 	discard;
			// }

			o.Albedo = IN.color;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Emission = _Emission;
		}

		ENDCG
	}
}