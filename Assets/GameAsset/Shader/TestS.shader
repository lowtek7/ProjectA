Shader "Custom/AlphaTextureBlend"
{
	Properties
	{
		_MainTex ("Texture A", 2D) = "white" {}
		_BTexture ("Texture B", 2D) = "white" {}
		_BTexturePosition ("Texture B Position", Vector) = (0, 0, 0, 0)
		_GridSize ("Texture B Grid Size", Vector) = (8, 8, 0, 0)
		_CellSize ("Texture B Cell Size", Vector) = (0.125, 0.125, 0, 0)
		_SelectedIndex ("Texture B Selected Index", Range(0, 63)) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _BTexture;
			float _BTextureIndex;
			float2 _BTextureSize;
			float2 _BTexturePosition;
			float2 _GridSize;
			float2 _CellSize;
			int _SelectedIndex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 텍스처 A와 B에서 색상과 알파 값을 가져옵니다.
				fixed4 colorA = tex2D(_MainTex, i.uv);

				// 알파 값이 0보다 작으면 완전히 투명하게 처리합니다.
				if (colorA.a < 0.001)
				{
					return fixed4(0.0, 0.0, 0.0, 0.0);
				}

				// B 텍스처의 그리드 크기를 계산합니다.
				// float gridSizeX = _BTextureSize.x;
				// float gridSizeY = _BTextureSize.y;
				// int gridIndex = int(_BTextureIndex);
				// float gridWidth = 1.0 / gridSizeX;
				// float gridHeight = 1.0 / gridSizeY;
				// float gridX = (gridIndex % gridSizeX) * gridWidth + _BTexturePosition.x;
				// float gridY = floor(gridIndex / gridSizeX) * gridHeight + _BTexturePosition.y;
				//
				// // 현재 픽셀이 B 텍스처의 그리드 인덱스에 해당하는지 확인합니다.
				// float2 bUV = float2(i.uv.x - gridX, i.uv.y - gridY);
				//
				// // B 텍스처의 픽셀 색상과 알파 값을 가져옵니다.
				// fixed4 bColor = tex2D(_BTexture, float3(bUV, _BTextureIndex));

				float x = _SelectedIndex % _GridSize.x;
				float y = (_GridSize.y - 1) - floor(_SelectedIndex / _GridSize.x);
				float4 bColor = tex2D(_BTexture, (i.uv + float2(x, y)) * _CellSize);

				// 회색 영역인 경우에만 B 텍스처를 적용합니다.
				float grayThreshold = 86.0 / 255.0;
				if (colorA.a < 1)
				{
					return fixed4(bColor.rgb, bColor.a * colorA.a);
				}


				return colorA;
			}
			ENDCG
		}
	}
}
