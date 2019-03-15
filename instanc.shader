  Shader "Instanced/InstancedSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows //vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
		//#pragma target 5.0

        sampler2D _MainTex;
		float3 _Color;
		float _WorldSize;
		float4x4 _Matrix;
		float3 _WorldPos;
		float3 _Length;
		float radius;

		#include "ComputeThings.cginc"

        struct Input 
		{
            float2 uv_MainTex;
        };

		
		float NormalizeFloat(float value, float max, float min)
		{
			return (value - min) / (max - min);
		}	

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> pos;
		StructuredBuffer<float4> p;
    #endif

    void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float3 data = pos[unity_InstanceID].xyz + _WorldPos;
			_Color = (p[unity_InstanceID] - pos[unity_InstanceID]) / .25 * .5 + .5;

			//_Color = particleBuffer[unity_InstanceID].vel.w;

			float scale = radius * 2 - .03 * (radius / .1);
			_Length = 1;

            unity_ObjectToWorld._11_21_31_41 = float4(scale, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, scale, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;

        #endif
        }

		void vert (inout appdata_full v) 
		{
			//v.vertex = mul(_Matrix, v.vertex);
			//v.normal = mul(_Matrix, v.normal);
		}

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) 
		{
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = _Color;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
