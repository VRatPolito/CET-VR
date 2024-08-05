Shader "Hidden/Blur"
{
	HLSLINCLUDE
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float4 _MainTex_TexelSize;
	uniform float _Parameter;

	half4 FragPrefilter4(VaryingsDefault i) : SV_Target
	{
		half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
		return SafeHDR(color);
	}

		static const half4 curve4[7] = { half4(0.0205,0.0205,0.0205,0), half4(0.0855,0.0855,0.0855,0), half4(0.232,0.232,0.232,0),
			half4(0.324,0.324,0.324,1), half4(0.232,0.232,0.232,0), half4(0.0855,0.0855,0.0855,0), half4(0.0205,0.0205,0.0205,0) };

	struct v2f_withBlurCoords8
	{
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		half2 offs : TEXCOORD1;
	};

	v2f_withBlurCoords8 vertBlurHorizontal(AttributesDefault v)
	{
		v2f_withBlurCoords8 o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter;

		return o;
	}

	v2f_withBlurCoords8 vertBlurVertical(AttributesDefault v)
	{
		v2f_withBlurCoords8 o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
#if UNITY_UV_STARTS_AT_TOP
		o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter;
		return o;
	}

	half4 fragBlur8(v2f_withBlurCoords8 i) : SV_Target
	{
		half2 uv = i.texcoord.xy;
		half2 netFilterWidth = i.offs;
		half2 coords = uv - netFilterWidth * 3.0;

		half4 color = 0;
		for (int l = 0; l < 7; l++)
		{
			half4 tap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, coords);
			color += tap * curve4[l];
			coords += netFilterWidth;
		}
		return color;
	}
		ENDHLSL

		SubShader
	{
		ZTest Off Cull Off ZWrite Off Blend Off

			// 0
			Pass
		{
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment FragPrefilter4
			ENDHLSL
		}

			// 1
			Pass
		{
			ZTest Always
			Cull Off
			HLSLPROGRAM
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur8
			ENDHLSL
		}

			// 2
			Pass
		{
			ZTest Always
			Cull Off
			HLSLPROGRAM
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur8
			ENDHLSL
		}
	}
}