struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 texCoord : TEXCOORD0;

};

struct v2f {
    float4 pos : SV_POSITION;
    float4 color : COLOR;
    float4 tex : TEXCOORD0;
};

uniform half _OutlineWidth;
fixed4 _OutlineColor;

v2f vert(appdata v) 
{
    // just make a copy of incoming vertex data but scaled according to normal direction
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
#if _OUTLINE_ENABLED
    float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
    float2 extendDir = normalize(TransformViewToProjection(norm.xy));

    o.pos.xy += extendDir * (o.pos.w * _OutlineWidth * 0.1);
#endif

    o.tex = v.texCoord;

    o.color = half4(0,0,0,1);
    return o;
}

half4 frag(v2f i) :COLOR
{
#if _OUTLINE_ENABLED
    return half4(_OutlineColor.rgb, 1.0);
#else
    clip(-1);
    return half4(0.0f, 0.0f, 0.0f, 0.0f);
#endif
}