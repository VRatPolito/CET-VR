// jave.lin 2020.03.21 Radial Blur-Radial Blur
Shader "Custom/RadialBlur" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Intensity("Intensity", Float) = 1
		_FadeRadius("FadeRadius", Float) = 0.38
		_SampleDistance("SampleDistance", Float) = 0.25
	}
	CGINCLUDE
	#pragma multi_compile __ MASK_ENABLED
	#pragma multi_compile __ VR_DISABLED
	#include "UnityCG.cginc"

	#define CLIP_FAR 1
	#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN)	
	#define CLIP_SCREEN CLIP_FAR
	#else
	#define CLIP_SCREEN 1-UNITY_NEAR_CLIP_VALUE
	#endif

				samplerCUBE _Skybox;
			float4x4 _EyeProjection[2];
			float4x4 _EyeToWorld[2];

			struct appdata {
				float4 vertex : POSITION;
	#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
				half2 uv : TEXCOORD0;
	#else
				float2 uv : TEXCOORD0;
	#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
	
	struct v2f {
		float4 vertex : SV_POSITION;
#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
		half2 uv : TEXCOORD0;
#else
		float2 uv : TEXCOORD0;
#endif
		UNITY_VERTEX_OUTPUT_STEREO
	};
	v2f vert(appdata v) {
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	float4 _MainTex_ST;
	fixed4 _Color;
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);		  // The original image
	float _Intensity;									  // Strength of the radial effect
	float _FadeRadius;									  // Fade out the radius of the radial effect
	float _SampleDistance;								  // The distance of each sample
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_DownSampleRT);     // The original image after downsampling
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MaskTex);          // Mask texture

	fixed4 frag(v2f i) : SV_Target{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)

		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(VR_DISABLED)
			float2 uv = i.uv;
		#else
			float2 uv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
		
		#endif

		fixed4 original;

		original = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv); //Insert

		const int sampleCount = 5; // Just think about the number of samples, multiplied by 2 is the true total number of times
		const float invSampleCount = 1.0 / ((float)sampleCount * 2);
		float2 vec = i.uv - 0.5;
		float len = length(vec);
		float fade = smoothstep(0, _FadeRadius, len); // Smoothly fade out the radial effect value
		float2 stepDir = normalize(vec) * _SampleDistance; // Each sampling step direction
		float stepLenFactor = len * 0.1 * _Intensity; // len: 0~0.5 multiplied by 0.1 is 0~0.05. The closer to the center, the smaller the sampling distance and the smaller the ambiguity relative to the edge
		stepDir *= stepLenFactor; // Control the step length value, stepLenFactor=len * 0.1 * _Intensity: 0.1 is the empirical value can be ignored, or external public control is also possible
		fixed4 sum = 0;
		float2 appliedStep;
		for (int it = 0; it < sampleCount; it++) {
			appliedStep = stepDir * it;
			sum += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_DownSampleRT, i.uv + appliedStep); // Forward sampling
			sum += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_DownSampleRT, i.uv - appliedStep); // reverse sampling
		}
		sum *= invSampleCount; // The mean is blurred

		fixed4 blurred = lerp(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv), sum, fade * _Intensity);

		// Sample mask
		#if MASK_ENABLED
			float maskVal = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MaskTex, uv).r;
			blurred = lerp(original, blurred, maskVal);
		#endif
		return blurred * _Color;
	}
		
		ENDCG
		SubShader {
		Cull Off 
		ZWrite Off 
		ZTest Always
			
			Pass{
				NAME "RADIAL_BLUR"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				ENDCG
		}
	}
}