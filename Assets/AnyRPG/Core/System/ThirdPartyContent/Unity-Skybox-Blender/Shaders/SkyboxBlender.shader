// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SkyboxBlender/BlendedSkybox" {
	Properties{
		_Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0
		_Blend("Blend", Range(0.0, 1.0)) = 0.0
		_BlendMode("Blend Mode", int) = 0
		_InvertColors("Invert Colors", Range(0,1)) = 0

		[NoScaleOffset] _FrontTex_1("Front_1 [+Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _BackTex_1("Back_1 [-Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _LeftTex_1("Left_1 [+X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _RightTex_1("Right_1 [-X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _UpTex_1("Up_1 [+Y]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _DownTex_1("Down_1 [-Y]   (HDR)", 2D) = "grey" {}

		[NoScaleOffset] _FrontTex_2("Front_2 [+Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _BackTex_2("Back_2 [-Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _LeftTex_2("Left_2 [+X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _RightTex_2("Right_2 [-X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _UpTex_2("Up_2 [+Y]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _DownTex_2("Down_2 [-Y]   (HDR)", 2D) = "grey" {}
	}

	SubShader{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off

		CGINCLUDE
		#include "UnityCG.cginc"

		half4 _Tint;
		half _Exposure;
		float _Rotation;
		float _Blend;
		int _BlendMode;
		float _InvertColors;

		float4 RotateAroundYInDegrees(float4 vertex, float degrees)
		{
			float alpha = degrees * UNITY_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float4(mul(m, vertex.xz), vertex.yw).xzyw;
		}

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
		};

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(RotateAroundYInDegrees(v.vertex, _Rotation));
			o.texcoord = v.texcoord;
			return o;
		}

		half4 skybox_frag(v2f i, sampler2D smp1, half4 smpDecode1, sampler2D smp2, half4 smpDecode2)
		{
			half4 tex1 = tex2D(smp1, i.texcoord);
			half4 tex2 = tex2D(smp2, i.texcoord);

			half3 c1 = DecodeHDR(tex1, smpDecode1);
			half3 c2 = DecodeHDR(tex2, smpDecode2);

			half3 c = half3(0, 0, 0);

			if (_BlendMode == 0) {
				//Linear blend
				c = lerp(c1, c2, _Blend);
			} else if (_BlendMode == 1) {
				//Maximum
				c = max(c1 * (1 - _Blend), c2 * _Blend);
			} else if (_BlendMode == 2) {
				//Add
				c = c1 + c2 * _Blend;
			} else if (_BlendMode == 3) {
				//Substract
				c = max(0, c1 - c2 * _Blend);
			} else if (_BlendMode == 4) {
				//Multiply
				c = c1 * lerp(1, c2, _Blend);
			}
			else if (_BlendMode == 5) {
				//Smoothstep
				c = lerp(c1, c2, smoothstep(0, 1, _Blend));
			}

			
			c = c * _Tint.rgb * unity_ColorSpaceDouble;
			c = lerp(c, 1 - c, _InvertColors);
			c *= _Exposure;
			return half4(c, 1);
		}
		ENDCG

		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _FrontTex_1;
			sampler2D _FrontTex_2;
			half4 _FrontTex_1_HDR;
			half4 _FrontTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_FrontTex_1, _FrontTex_1_HDR, _FrontTex_2, _FrontTex_2_HDR); }
			ENDCG
		}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _BackTex_1;
			sampler2D _BackTex_2;
			half4 _BackTex_1_HDR;
			half4 _BackTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_BackTex_1, _BackTex_1_HDR, _BackTex_2, _BackTex_2_HDR); }
			ENDCG
		}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _LeftTex_1;
			sampler2D _LeftTex_2;
			half4 _LeftTex_1_HDR;
			half4 _LeftTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_LeftTex_1, _LeftTex_1_HDR, _LeftTex_2, _LeftTex_2_HDR); }
			ENDCG
		}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _RightTex_1;
			sampler2D _RightTex_2;
			half4 _RightTex_1_HDR;
			half4 _RightTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_RightTex_1, _RightTex_1_HDR, _RightTex_2, _RightTex_2_HDR); }
			ENDCG
		}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _UpTex_1;
			sampler2D _UpTex_2;
			half4 _UpTex_1_HDR;
			half4 _UpTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_UpTex_1, _UpTex_1_HDR, _UpTex_2, _UpTex_2_HDR); }
			ENDCG
		}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _DownTex_1;
			sampler2D _DownTex_2;
			half4 _DownTex_1_HDR;
			half4 _DownTex_2_HDR;
			half4 frag(v2f i) : SV_Target{ return skybox_frag(i,_DownTex_1, _DownTex_1_HDR, _DownTex_2, _DownTex_2_HDR); }
			ENDCG
		}
	}
}