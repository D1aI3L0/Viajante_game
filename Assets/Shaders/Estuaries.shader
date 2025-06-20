Shader "Custom/Estuaries"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert
		#pragma target 3.0

        #include "Water.cginc"

        sampler2D _MainTex;

        struct Input {
			float2 uv_MainTex;
			float2 riverUV;
			float3 worldPos;
		};

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.riverUV = v.texcoord1.xy;
		}

        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
			float shore = IN.uv_MainTex.y;
			float foam = Foam(shore, IN.worldPos.xz, _MainTex);
			float waves = Waves(IN.worldPos.xz, _MainTex);
			waves *= 1 - shore;

            float shoreWater = max(foam, waves);
            float river = River(IN.riverUV, _MainTex);
            float water = lerp(shoreWater, river, IN.uv_MainTex.x);
            
            fixed4 c = saturate(_Color + water);
			
            o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
        

        ENDCG
    }
    FallBack "Diffuse"
}