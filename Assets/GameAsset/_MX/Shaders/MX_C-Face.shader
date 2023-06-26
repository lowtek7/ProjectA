Shader "MX/C-Face" {
	Properties {
		[HDR] _Tint ("Tint", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Main Tex", 2D) = "white" {}
		_ShadowLightDir ("Shadow Light Dir", Vector) = (0,-0.94,0.342,0)
		[HDR] _ShadowTint ("Shadow Tint", Vector) = (0.5,0.5,0.5,1)
		_ShadowThreshold ("Shadow Threshold", Range(-5, 5)) = 0.5
		[NoScaleOffset] _MaskTex ("Mask Tex", 2D) = "black" {}
		_MaskGSensitivity ("Mask G Sensitivity", Range(0, 3)) = 1
		_AdjustiveFaceShadow ("Apply Mask R", Range(0, 1)) = 0
		_RimAreaMultiplier ("Rim Area Multiplier", Range(1, 7)) = 3
		_RimAreaLeveler ("Rim Area Leveler", Float) = 2
		_RimStrength ("Rim Strength", Range(0, 10)) = 1
		[HDR] _OutlineTint ("Outline Tint", Vector) = (0.5,0.5,0.5,1)
		_OutlineZCorrection ("Outline Correction (world z)", Range(-0.0005, 0.0005)) = 0
		[HideInInspector] _CodeAddColor ("_CodeAddColor", Vector) = (0,0,0,0)
		[HideInInspector] _CodeMultiplyColor ("_CodeMultiplyColor", Vector) = (1,1,1,1)
		[HideInInspector] _CodeAddRimColor ("_CodeAddRimColor", Vector) = (0,0,0,0)
		[HideInInspector] _DitherThreshold ("_DitherThreshold", Range(0, 1)) = 0
		[HideInInspector] _GrayBrightness ("_GrayBrightness", Float) = 1
		[HideInInspector] _Cull ("__cull", Float) = 2
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
	//CustomEditor "MXCharacterFaceV2ShaderGUI"
}