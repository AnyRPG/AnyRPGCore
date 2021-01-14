Shader "Custom/UVScrollShader" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_TextureColor("Texture Color", Color) = (1, 1, 1, 1)
		_ScrollXSpeed("X Scroll Speed", Range(-5, 5)) = 0
		_ScrollYSpeed("Y Scroll Speed", Range(-5, 5)) = 0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			#pragma surface surf Lambert alpha

			sampler2D _MainTex;
			fixed4 _TextureColor;
			fixed _ScrollXSpeed;
			fixed _ScrollYSpeed;

			struct Input {
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				fixed varX = _ScrollXSpeed * _Time;
				fixed varY = _ScrollYSpeed * _Time;
				fixed2 uv_Tex = IN.uv_MainTex + fixed2(varX, varY);
				half4 c = tex2D(_MainTex, uv_Tex) * _TextureColor;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}