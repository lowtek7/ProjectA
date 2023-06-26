Shader "MX/C-General/Layer4" {
	Properties {
		[HDR] _Tint ("Tint", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Main Tex", 2D) = "white" {}
		_Cutoff ("Cutoff", Range(0, 1)) = 0.2
		[HDR] _TwoSideTint ("Two Side Tint", Vector) = (1,1,1,1)
		[NoScaleOffset] _MaskTex ("Layer / Mask Tex", 2D) = "gray" {}
		_MaskGSensitivity ("Mask G Sensitivity", Range(0, 3)) = 1
		_BaseBrightness4 ("Base Brightness", Vector) = (0,0,0,0)
		_ShadowTintR4 ("Shadow Tint R", Vector) = (1,1,1,1)
		_ShadowTintG4 ("Shadow Tint G", Vector) = (1,1,1,1)
		_ShadowTintB4 ("Shadow Tint B", Vector) = (1,1,1,1)
		_ShadowThreshold4 ("Shadow Threshold", Vector) = (0,0,0,0)
		_ViewOffset4 ("View Light Angle Offset", Vector) = (0,0,0,0)
		_ViewPower4 ("View Light Sharpness", Vector) = (1,1,1,1)
		_ViewStrength4 ("View Light Strength", Vector) = (0,0,0,0)
		_InvViewPower4 ("Inv.View Light Sharpness", Vector) = (1,1,1,1)
		_InvViewStrength4 ("Inv.View Light Strength", Vector) = (0,0,0,0)
		_ViewLightEdge4 ("View Light Edge", Vector) = (0,0,0,0)
		_RimAreaMultiplier4 ("Rim Sharpness", Vector) = (3,3,3,3)
		_RimStrength4 ("Rim Strength", Vector) = (1,1,1,1)
		[HDR] _GlowTint ("Glow Tint", Vector) = (0,0,0,1)
		_GlowStrength ("Glow Strength", Range(0, 5)) = 0
		[HDR] _OutlineTint ("Outline Tint", Vector) = (0.5,0.5,0.5,1)
		_OutlineZCorrection ("Outline Correction (world z)", Range(-0.0005, 0.0005)) = 0
		[HideInInspector] _CodeAddColor ("_CodeAddColor", Vector) = (0,0,0,0)
		[HideInInspector] _CodeMultiplyColor ("_CodeMultiplyColor", Vector) = (1,1,1,1)
		[HideInInspector] _CodeAddRimColor ("_CodeAddRimColor", Vector) = (0,0,0,0)
		[HideInInspector] _DitherThreshold ("_DitherThreshold", Range(0, 1)) = 0
		[HideInInspector] _GrayBrightness ("_GrayBrightness", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off,0, On,1)] _ZWrite ("Z Write", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "MXCharacterGeneralV2ShaderGUI"
}