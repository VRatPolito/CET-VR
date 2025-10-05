Shader "Unlit/CircleEffect_Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecondaryTexture("SecTexture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vertexToFragment
			#pragma fragment giveColor

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vertexToFragment (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			float4 _MainTex_TexelSize;
			sampler2D _SecondaryTexture;
			float4 _SecondaryTexture_TexelSize;
			float _blackRatio = 1;
			float _viewRadius = .25;
			float _leftEye = 0;
			float _rightEye = 0;

			// FIX: no sampler parameter; sample _MainTex directly
			float4 gridOverPixel(float2 uv, float4 size)
			{
				float4 newFragColor = 0;
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + float2(0, 0));
				return newFragColor;
			}

			float4 giveColor (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float4 col = gridOverPixel(i.uv, _MainTex_TexelSize);

				float2 coord = i.uv;
				coord -= float2(.5,.5);

				// Sample secondary normally (not with UNITY macro)
				float4 col_2 = tex2D(_SecondaryTexture, i.uv);

				if (sqrt((coord.y * coord.y) + (coord.x) * (coord.x )) > _viewRadius)
				{
					return col_2;
				}

				return col;
			}
			ENDCG
		}
	}
}
