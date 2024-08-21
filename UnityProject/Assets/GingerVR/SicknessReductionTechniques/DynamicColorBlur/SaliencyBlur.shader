// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SaliencyBlur"
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

			float _kernel[121]; //this is our gaussian kernel
			float _kernelSum; //whatever to divide by
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
			float _brightnessThreshold;
			float _redThreshold;
			float _greenThreshold;
			float _blueThreshold;
			float _darkSaliency;
            

			float4 gridOverPixel(sampler2D tex, float2 uv, float4 size) //average values surrounding pixel together and return the result
			{
				
				float4 currentColor = tex2D(tex,uv+float2(size.x * 0, size.y * 0));

				if(_darkSaliency == 1){
					if(dot(currentColor, float4(1,1,1,0)) <= _brightnessThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(1,0,0,0)) <= _redThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(0,1,0,0)) <= _greenThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(0,0,1,0)) <= _blueThreshold){
						return currentColor;
					}
				}else{
					if(dot(currentColor, float4(1,1,1,0)) >= _brightnessThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(1,0,0,0)) >= _redThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(0,1,0,0)) >= _greenThreshold){
						return currentColor;
					}
					if(dot(currentColor, float4(0,0,1,0)) >= _blueThreshold){
						return currentColor;
					}
				}

				

			
				


				float4 newFragColor = 0;

				// float newFragColor = 0;
				// newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uv+float2(-size.x*5, size.y*5)) * _kernel[0];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * -5)) *_kernel[0];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * -5)) *_kernel[1];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * -5)) *_kernel[2];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * -5)) *_kernel[3];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * -5)) *_kernel[4];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * -5)) *_kernel[5];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * -5)) *_kernel[6];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * -5)) *_kernel[7];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * -5)) *_kernel[8];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * -5)) *_kernel[9];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * -5)) *_kernel[10];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * -4)) *_kernel[11];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * -4)) *_kernel[12];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * -4)) *_kernel[13];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * -4)) *_kernel[14];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * -4)) *_kernel[15];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * -4)) *_kernel[16];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * -4)) *_kernel[17];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * -4)) *_kernel[18];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * -4)) *_kernel[19];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * -4)) *_kernel[20];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * -4)) *_kernel[21];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * -3)) *_kernel[22];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * -3)) *_kernel[23];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * -3)) *_kernel[24];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * -3)) *_kernel[25];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * -3)) *_kernel[26];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * -3)) *_kernel[27];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * -3)) *_kernel[28];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * -3)) *_kernel[29];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * -3)) *_kernel[30];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * -3)) *_kernel[31];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * -3)) *_kernel[32];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * -2)) *_kernel[33];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * -2)) *_kernel[34];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * -2)) *_kernel[35];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * -2)) *_kernel[36];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * -2)) *_kernel[37];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * -2)) *_kernel[38];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * -2)) *_kernel[39];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * -2)) *_kernel[40];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * -2)) *_kernel[41];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * -2)) *_kernel[42];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * -2)) *_kernel[43];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * -1)) *_kernel[44];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * -1)) *_kernel[45];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * -1)) *_kernel[46];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * -1)) *_kernel[47];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * -1)) *_kernel[48];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * -1)) *_kernel[49];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * -1)) *_kernel[50];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * -1)) *_kernel[51];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * -1)) *_kernel[52];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * -1)) *_kernel[53];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * -1)) *_kernel[54];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 0)) *_kernel[55];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 0)) *_kernel[56];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 0)) *_kernel[57];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 0)) *_kernel[58];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 0)) *_kernel[59];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 0)) *_kernel[60];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 0)) *_kernel[61];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 0)) *_kernel[62];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 0)) *_kernel[63];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 0)) *_kernel[64];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 0)) *_kernel[65];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 1)) *_kernel[66];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 1)) *_kernel[67];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 1)) *_kernel[68];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 1)) *_kernel[69];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 1)) *_kernel[70];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 1)) *_kernel[71];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 1)) *_kernel[72];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 1)) *_kernel[73];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 1)) *_kernel[74];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 1)) *_kernel[75];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 1)) *_kernel[76];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 2)) *_kernel[77];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 2)) *_kernel[78];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 2)) *_kernel[79];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 2)) *_kernel[80];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 2)) *_kernel[81];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 2)) *_kernel[82];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 2)) *_kernel[83];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 2)) *_kernel[84];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 2)) *_kernel[85];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 2)) *_kernel[86];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 2)) *_kernel[87];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 3)) *_kernel[88];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 3)) *_kernel[89];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 3)) *_kernel[90];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 3)) *_kernel[91];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 3)) *_kernel[92];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 3)) *_kernel[93];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 3)) *_kernel[94];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 3)) *_kernel[95];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 3)) *_kernel[96];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 3)) *_kernel[97];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 3)) *_kernel[98];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 4)) *_kernel[99];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 4)) *_kernel[100];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 4)) *_kernel[101];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 4)) *_kernel[102];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 4)) *_kernel[103];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 4)) *_kernel[104];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 4)) *_kernel[105];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 4)) *_kernel[106];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 4)) *_kernel[107];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 4)) *_kernel[108];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 4)) *_kernel[109];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -5, size.y * 5)) *_kernel[110];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -4, size.y * 5)) *_kernel[111];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -3, size.y * 5)) *_kernel[112];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -2, size.y * 5)) *_kernel[113];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * -1, size.y * 5)) *_kernel[114];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 0, size.y * 5)) *_kernel[115];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 1, size.y * 5)) *_kernel[116];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 2, size.y * 5)) *_kernel[117];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 3, size.y * 5)) *_kernel[118];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 4, size.y * 5)) *_kernel[119];
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,uv+float2(size.x * 5, size.y * 5)) *_kernel[120];
					

				return newFragColor;
			}

            //returns color in float 4 variable given our v2f
			float4 giveColor (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
	
				float4 col = gridOverPixel(_MainTex, i.uv, _MainTex_TexelSize);
				return col;
			}
			ENDCG
		}
	}
}