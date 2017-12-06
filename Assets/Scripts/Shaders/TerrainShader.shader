Shader "Custom/TerrainShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2DArray) = "white" {}
		_Glossiness ("Smoothness", Range(0, 0.3)) = 0.2
		_Tiling ("Tiling", Vector) = (1, 1, 1, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows vertex:vert

		#pragma target 3.5

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 worldNormal;
			float2 index;
		};

		half _Glossiness;
		float3 _Tiling;

		void vert(inout appdata_full v, out Input IN) {
			UNITY_INITIALIZE_OUTPUT(Input, IN);
			IN.index = v.texcoord;
		}

		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		float3 triplanar(Input IN, float index) {
			float3 uv = float3(IN.worldPos.x * _Tiling.x,
				IN.worldPos.y * _Tiling.y,
				IN.worldPos.z * _Tiling.z) * 0.02;

			float3 blend = abs(IN.worldNormal);
			blend /= dot(blend, 1);

			float3 cx = UNITY_SAMPLE_TEX2DARRAY(_MainTex,
				float3(uv.yz, index)) * blend.x;
			float3 cy = UNITY_SAMPLE_TEX2DARRAY(_MainTex,
				float3(uv.xz, index)) * blend.y;
			float3 cz = UNITY_SAMPLE_TEX2DARRAY(_MainTex,
				float3(uv.xy, index)) * blend.z;

			return cx + cy + cz;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Texture and intensity in channel r
			float3 c1 = triplanar(IN, IN.index.r) * IN.color.r;

			// Texture and intensity in channel g
			float3 c2 = triplanar(IN, IN.index.g) * IN.color.g;

			o.Albedo = c1 + c2;
			
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
}
