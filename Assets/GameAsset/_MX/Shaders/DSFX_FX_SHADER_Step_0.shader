Shader "DSFX/FX_SHADER_Step_0" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_Multiply ("Multiply", Float) = 1
		_Tex_Main ("Tex_Main", 2D) = "white" {}
		[Toggle] _RGBRGBA ("RGB>RGBA", Float) = 0
		[Toggle] _Custom_Data_Main_Offset_Use ("Custom_Data_Main_Offset_Use", Float) = 0
		_Main_Speed_X ("Main_Speed_X", Float) = 0
		_Main_Speed_Y ("Main_Speed_Y", Float) = 0
		_Tex_Mask ("Tex_Mask", 2D) = "white" {}
		[Toggle] _Custom_Data_Mask_Offset_Use ("Custom_Data_Mask_Offset_Use", Float) = 0
		_Mask_Speed_X ("Mask_Speed_X", Float) = 0
		_Mask_Speed_Y ("Mask_Speed_Y", Float) = 0
		[Toggle] _Step_Scroll_Use ("Step_Scroll_Use", Float) = 1
		_Step_Power ("Step_Power", Range(0, 1)) = 1
		[Toggle] _Step_Custom_DataVertex_color_Use ("Step_Custom_Data/Vertex_color_Use", Float) = 0
		[Toggle] _Vertex_Alpha_Use ("Vertex_Alpha_Use", Float) = 0
		[Toggle] _ZWrite_Mode ("ZWrite_Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull_Mode ("Cull_Mode", Float) = 2
		[Enum()] _ZOffsetFactor ("ZOffsetFactor", Float) = 0
		[Enum()] _ZOffsetUnits ("ZOffsetUnits", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}