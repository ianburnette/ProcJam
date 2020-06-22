
// pretty much a standard unity shader but it reads colored vertices from the mesh
Shader "Clayxels/ClayxelBuiltInMeshShader" {
    Properties {
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [HDR]_Emission ("Emission", Color) = (0, 0, 0, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
 
        struct Input {
            float4 vertexColor : COLOR;
        };
 
        half _Smoothness;
        half _Metallic;
        fixed4 _Emission;
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = IN.vertexColor;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _Emission;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
