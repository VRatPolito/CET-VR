Shader "Unlit/CircleEffect_Shader"
{
    	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} //include a texture as a property
		_SecondaryTexture("SecTexture", 2D) = "white" {}
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
			sampler2D _SecondaryTexture;
			float4 _SecondaryTexture_TexelSize;
			float _blackRatio = 1;
			float _viewRadius = .25;
			float _leftEye = 0;
			float _rightEye = 0;
            

			float4 gridOverPixel(sampler2D tex, float2 uv, float4 size) //average values surrounding pixel together and return the result
			{

                float4 newFragColor = 0;
               
				newFragColor += UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uv + float2(0, 0)); //this should be the same
				//we can just use sin and cos 
				return newFragColor;
				
			
			}

            //returns color in float 4 variable given our v2f
			float4 giveColor (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
	
				float4 col = gridOverPixel(_MainTex, i.uv, _MainTex_TexelSize);
				//float4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
				float2 coord = i.uv;
				coord -= float2(.5,.5);
				float4 col_2 = gridOverPixel(_SecondaryTexture, i.uv, _MainTex_TexelSize);
				if(sqrt((coord.y * coord.y) + (coord.x) * (coord.x )) > _viewRadius){
						//return col * _blackRatio ;
						return col_2;
				}

				return col;
				//so this returns the rgb
				//i.uv.x and i.uv.y returns the color for that position
				//return float4(0, 0, 0, 1);
			}
			ENDCG
		}
	}
    /*Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //Insert
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {    
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
    
                // sample the texture
                fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }*/
}
