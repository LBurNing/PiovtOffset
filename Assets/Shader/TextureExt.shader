Shader "Framework/TextureExt"
{
	Properties{
		_MainTex("Main Tex", 2D) = "white" {}
		_AlphaTex("Alpha Tex", 2D) = "white" {}
		_Alpha("Alpha", float) = 1
	}

		CGINCLUDE

		#include "UnityCG.cginc"
		#pragma shader_feature _GRAY

		sampler2D _MainTex;
		sampler2D _AlphaTex;
		half4 _MainTex_ST;
		half4 _AlphaTex_ST;
		float _Alpha;

		float4x4 _ColorMatrix;
		float4 _ColorOffset;
		int _Gray;

		struct v2f {
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		float4 frag(v2f i) : COLOR{
			float4 texcol = tex2D(_MainTex, i.uv);
			texcol.a = tex2D(_AlphaTex, i.uv) * texcol.a * _Alpha;

#if _GRAY
			float grayColor = texcol.r * _ColorMatrix[0] + texcol.g * _ColorMatrix[1] + texcol.b * _ColorMatrix[2];
			texcol.rgb = float3(grayColor, grayColor, grayColor);
#else
			texcol.r = dot(texcol, _ColorMatrix[0]) + _ColorOffset.x;
			texcol.g = dot(texcol, _ColorMatrix[1]) + _ColorOffset.y;
			texcol.b = dot(texcol, _ColorMatrix[2]) + _ColorOffset.z;
#endif
			texcol.a = dot(texcol, _ColorMatrix[3]) + _ColorOffset.w;
			return texcol;
		}

			ENDCG

			SubShader {
			Tags{ "Queue" = "Transparent" }

				Pass{
					Lighting Off
					cull off
					ZWrite Off
					ZTest Off
					Blend SrcAlpha OneMinusSrcAlpha

					CGPROGRAM
					#pragma vertex vert  
					#pragma fragment frag  
					ENDCG
			}
		}

		FallBack "Diffuse"
}