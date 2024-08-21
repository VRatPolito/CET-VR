﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/HeadSnapperShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} //include a texture as a property
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off //ZTest Always

		Pass
		{
			CGPROGRAM


			#pragma vertex vertexToFragment
			#pragma fragment giveColor
			
			#include "UnityCG.cginc"

            //info from vertex on the mesh
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
	
				UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
			};

            //info for fragment function
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
		
				UNITY_VERTEX_OUTPUT_STEREO //Insert
			};

			v2f vertexToFragment (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v); //Insert
				UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
	
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			//sampler2D _MainTex;
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //Insert
			float4 _MainTex_TexelSize;
			
			float _darkness;
            //returns color in float 4 variable given our v2f
			float4 giveColor (v2f i) : SV_Target
			{    
				float4 col = UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(_MainTex, i.uv + float2(_MainTex_TexelSize.x * 0, _MainTex_TexelSize.y * 0));
				return col * (1.0 - _darkness);	
			}
			ENDCG
		}
	}
}