Shader "Custom/GridTextureShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_GridSize ("Grid Size", Vector) = (8, 8, 0, 0)
		_CellSize ("Cell Size", Vector) = (0.125, 0.125, 0, 0)
		_SelectedIndex ("Selected Index", Range(0, 63)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		LOD 100

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		float2 _GridSize;
		float2 _CellSize;
		int _SelectedIndex;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			float x = _SelectedIndex % _GridSize.x;
			float y = (_GridSize.y - 1) - floor(_SelectedIndex / _GridSize.x);
			float4 color = tex2D(_MainTex, (IN.uv_MainTex + float2(x, y)) * _CellSize);

			o.Albedo = color.rgb;
			o.Alpha = color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
