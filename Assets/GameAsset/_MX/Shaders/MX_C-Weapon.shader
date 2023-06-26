Shader "MX/C-Weapon" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Main Tex", 2D) = "white" {}
		[NoScaleOffset] _SourceTex ("Mask Tex", 2D) = "white" {}
		_ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
		_ShadowStrong ("Shadow Strength", Float) = 10
		_LightValue ("Light Threshold ", Range(0, 1)) = 0.5
		_LightStrong ("Light Strength", Float) = 10
		_SpecStrong ("Spec Color Strength", Range(0, 1)) = 0.1
		_ShadowTint ("Shadow Tint", Vector) = (0.5,0.5,0.5,1)
		_SpecColor ("Spec Tint", Vector) = (1,1,1,1)
		_FakeLightDir ("Light Direction", Vector) = (0.1,0.65,0,0)
		[Header(Glow Option)] [Toggle(_GLOW_0)] _UseGlow ("use glow?", Float) = 0
		_GlowMaskColor0 ("Glow Mask Color", Vector) = (1,1,1,1)
		_GlowStrictness0 ("Glow Mask Tolerance", Range(30, 0.34)) = 3
		[HDR] _GlowTint0 ("Glow Tint", Vector) = (0,0,0,1)
		_GlowStrength0 ("Glow Strength", Range(0, 10)) = 0
		[Space(10)] [Header(Outline Option)] _OutlineTint ("Outline Tint", Vector) = (0.5,0.5,0.5,1)
		[HideInInspector] _CodeAddColor ("_CodeAddColor", Vector) = (0,0,0,0)
		[HideInInspector] _CodeMultiplyColor ("_CodeMultiplyColor", Vector) = (1,1,1,1)
		[HideInInspector] _CodeAddRimColor ("_CodeAddRimColor", Vector) = (0,0,0,0)
		[HideInInspector] _GrayBrightness ("_GrayBrightness", Float) = 1
		[Space(10)] [Toggle(_DITHER_HORIZONTAL_LINES)] _IsDither ("_DITHER_HORIZONTAL_LINES", Float) = 0
		_DitherThreshold ("_DitherThreshold", Range(0, 1)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Hidden/InternalErrorShader"
}