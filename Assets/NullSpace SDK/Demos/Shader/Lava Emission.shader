Shader "Custom/Lava Emission" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_ColorB("ColorB", Color) = (1,1,1,1)
		_MainTex("Main texture (RGB)", 2D) = "white" {}
		_ScrollXSpeed("X Scroll Speed", Range(-10,10)) = -1
		_ScrollYSpeed("Y Scroll Speed", Range(-10,10)) = .25
		_MainTexB("Main textureB (RGB)", 2D) = "white" {}
		_ScrollXSpeedB("X Scroll SpeedB", Range(-10,10)) = .1
		_ScrollYSpeedB("Y Scroll SpeedB", Range(-10,10)) = -1
		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows alpha:blend

			// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MainTexB;
		sampler2D _EmissionMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_MainTexB;
			float2 uv_EmissionMap;
			float4 screenPos;
			float3 worldNormal;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		float _DotProduct;
		fixed4 _Color;
		fixed4 _ColorB;
		fixed _ScrollXSpeed;
		fixed _ScrollYSpeed;
		fixed _ScrollXSpeedB;
		fixed _ScrollYSpeedB;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed2 scrolledUV = IN.uv_MainTex;
			fixed2 scrolledUVB = IN.uv_MainTexB;
			fixed2 emissionUV = IN.uv_EmissionMap;
			scrolledUV += fixed2(_Time.x * _ScrollXSpeed, _Time.x * _ScrollYSpeed);
			scrolledUVB += fixed2(_Time.x * _ScrollXSpeedB, _Time.x * _ScrollYSpeedB);

			fixed4 c = tex2D(_MainTex, scrolledUV) * _Color * (tex2D(_MainTexB, scrolledUVB) * _ColorB);

			fixed4 emissSample = tex2D(_EmissionMap, emissionUV);

			o.Albedo = c.rgb;// *emissSample.rgb;
			o.Emission = emissSample.rgb / 4;

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}


		ENDCG
		}
			FallBack "Diffuse"
}