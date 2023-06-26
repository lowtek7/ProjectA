sampler2D _MainTex;
sampler2D _AOMap;
sampler2D _EmissionMap;
#if _PARAMETER_TEXTURE
sampler2D _ParTex;
#else
sampler2D _GlossinessMap;
sampler2D _SpecMap;
#endif
#if _NORMAL_MAP
sampler2D _BumpMap;
#endif

struct Input
{
    float2 uv_MainTex;
#if _NORMAL_MAP
    float2 uv_BumpMap;
#endif
};

half _Glossiness;
half _Metallic;
fixed4 _Color;
half _ShadowAttenuation;

float3 _Emission;
half _EmissionIntensity;

fixed _AOWeight;

fixed _HighDivision;
fixed _MidDivision;
fixed _LowDivision;
fixed _SpecDivision;
fixed _SpecIntensity;
fixed _FresnelIntensity;

#if _SUBSURFACE_SCATTERING
fixed4 _ScatteringColor;
fixed4 _ScatteringColorSub;
half _ScatteringWeight;
half _ScatteringSize;
half _ScatteringAttenuation;
#endif

struct ToonSurfaceOutput
{
    fixed3 Albedo;  // diffuse color
    fixed3 Normal;  // tangent space normal, if written
    fixed3 Emission;
    fixed3 diffColor;
    half Specular;  // specular power in 0..1 range
    fixed3 SpecularMap;
    fixed AmbientOcclusion;
    fixed Gloss;    // specular intensity
    fixed Alpha;    // alpha for transparencies
    fixed Smoothness;
};

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
    // put more per-instance properties here
UNITY_INSTANCING_BUFFER_END(Props)

float Pow2(float x)
{
    return x * x;
}

float Pow3(float x)
{
    return x * x * x;
}

float Pow5(float x)
{
    return x * x * x * x * x;
}

float sigmoid(float x, float center, float sharp) 
{
    float s;
    s = 1 / (1 + pow(100000, (-3 * sharp * (x - center))));
    return s;
}

float ndc2Normal(float x) 
{
    return x * 0.5 + 0.5;
}

// a2 is the roughness^2
float D_GGX(float a2, float NoH) 
{
    float d = (NoH * a2 - NoH) * NoH + 1;
    return a2 / (3.14159 * d * d);
}

float3 Fresnel_schlick(float VoN, float3 rF0) 
{
    return rF0 + (1 - rF0) * Pow5(1 - VoN);
}

float3 Fresnel_extend(float VoN, float3 rF0) 
{
    return rF0 + (1 - rF0) * Pow3(1 - VoN);
}

float Gaussion(float x, float center, float var) 
{
    return pow(2.718, -1 * Pow2(x - center) / var);
}

float3 warp(float3 x, float3 w) 
{
    return (x + w) / (1 + w);
}

float3 warp(float3 x, float w) 
{
    return (x + w.xxx) / (1 + w.xxx);
}

half4 LightingToon(ToonSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
{
    half4 c;
    
    half3 nNormal = normalize(s.Normal);			
    float3 reflectDir = reflect(-viewDir, s.Normal);

    half NoL = dot(nNormal, lightDir) + _ShadowAttenuation * (atten - 1);
    half3 HDir = normalize(lightDir + viewDir);
    half NoH = Pow2(dot(nNormal, HDir)) + _ShadowAttenuation * (atten - 1);
    half VoN = dot(nNormal, viewDir);
    half VoL = dot(viewDir, lightDir);
    half VoH = dot(viewDir, HDir) + _ShadowAttenuation * 2 * (atten - 1);

    half _BoundSharp = 9.5 * Pow2(s.Smoothness) + 0.5;
    
    
    // Diffuse
    half HLightSig = sigmoid(NoL, _HighDivision, _BoundSharp);
    half MidSig = sigmoid(NoL, _MidDivision, _BoundSharp);
    half DarkSig = sigmoid(NoL, _LowDivision, _BoundSharp);

    half HLightWin = HLightSig;
    half MidLWin = MidSig - HLightSig;
    half MidDWin = DarkSig - MidSig;
    half DarkWin = 1 - DarkSig;
    
    
    half diffuseLumin0 = (1 + ndc2Normal(_HighDivision)) / 2;
    half diffuseLumin1 = (ndc2Normal(_HighDivision) + ndc2Normal(_MidDivision)) / 2;
    half diffuseLumin2 = (ndc2Normal(_MidDivision) + ndc2Normal(_LowDivision)) / 2;
    half diffuseLumin3 = ndc2Normal(_LowDivision) / 2;

    half diffuse = HLightWin * diffuseLumin0 + MidLWin * diffuseLumin1;
    diffuse += MidDWin * diffuseLumin2 + DarkWin * diffuseLumin3;
    
    
    // Specular
    half roughness = 1 - s.Smoothness;
    half a2 = Pow2(roughness);
    
    half NDF0 = D_GGX(a2, 1);
    half NDF_HBound = NDF0 * _SpecDivision;
    half NDF = D_GGX(a2, saturate(NoH));
    
    half specularWin = sigmoid(NDF, NDF_HBound, _BoundSharp);
    
    half specular = specularWin * (NDF0 + NDF_HBound) / 2 * _SpecIntensity;
    
    // Fresnel
    half3 fresnel = Fresnel_extend(VoN, float3(0.1, 0.1, 0.1));
    half3 fresnelResult = _FresnelIntensity * fresnel * (1 - VoL) / 2;
    
    
    // Scattering
#if _SUBSURFACE_SCATTERING
    half SSLambert = warp(NoL, _ScatteringWeight);
    
    half SSMidLWin_M = Gaussion(NoL, _MidDivision, _ScatteringAttenuation * _ScatteringSize);
    half SSMidDWin_M = Gaussion(NoL, _MidDivision, _ScatteringSize);

    half SSMidLWin2_M = Gaussion(NoL, _MidDivision, _ScatteringAttenuation * _ScatteringSize * 0.01);
    half SSMidDWin2_M = Gaussion(NoL, _MidDivision, _ScatteringSize * 0.01);
    
    half SSMidLWin_D = Gaussion(NoL, _LowDivision, _ScatteringAttenuation * _ScatteringSize);
    half SSMidDWin_D = Gaussion(NoL, _LowDivision, _ScatteringSize);

    half SSMidLWin2_D = Gaussion(NoL, _LowDivision, _ScatteringAttenuation * _ScatteringSize * 0.01);
    half SSMidDWin2_D = Gaussion(NoL, _LowDivision, _ScatteringSize * 0.01);

    half3 SSLumin1_M = MidLWin * diffuseLumin2 * _ScatteringAttenuation * (SSMidLWin_M + SSMidLWin2_M + SSMidLWin_D + SSMidLWin2_D);
    half3 SSLumin2_M = ((MidDWin + DarkWin) * diffuseLumin2) * (SSMidDWin_M + SSMidDWin2_M + SSMidDWin_D + SSMidDWin2_D);

    half3 SS = _ScatteringWeight * (SSLumin1_M + SSLumin2_M) * _ScatteringColor.rgb;
#endif
    
    half3 Intensity = diffuse.xxx * s.AmbientOcclusion + specular.xxx * s.SpecularMap + fresnelResult.xxx;
    c = half4(s.diffColor.rgb, 1.0) * half4(Intensity.xxx, 1.0) * half4(_LightColor0.rgb, 1.0);

#if _SUBSURFACE_SCATTERING
    c = c + half4(SS, 0.0f);
#endif

    c.a = s.Alpha;
    //c = c * atten;
    return c;
    
    //half3 lightResult = specular * _LightColor0.rgb + (1 - specular) * diffuse * s.diffColor.rgb + fresnelResult * s.diffColor.rgb;
    //return half4(lightResult.rgb, 1.0);
}

void surf(Input IN, inout ToonSurfaceOutput o)
{
    // Albedo comes from a texture tinted by color
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

    fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
    o.Albedo = 0.5 * ambient;
#if _NORMAL_MAP
    o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
#endif
#if _PARAMETER_TEXTURE
    o.SpecularMap = tex2D(_ParTex, IN.uv_MainTex).ggg;
#else
    o.SpecularMap = tex2D(_SpecMap, IN.uv_MainTex);
#endif
    
    o.Emission = _Emission * tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionIntensity;

#if _PARAMETER_TEXTURE
    o.Smoothness = saturate(_Glossiness * saturate(1.0f - tex2D(_ParTex, IN.uv_MainTex).b));
#else
    o.Smoothness = saturate(_Glossiness * saturate(1.0f - tex2D(_GlossinessMap, IN.uv_MainTex).r));
#endif
    o.diffColor = c.rgb * _Color.rgb;
    o.Alpha = c.a;
    
    o.AmbientOcclusion = tex2D(_AOMap, IN.uv_MainTex);
    o.AmbientOcclusion = o.AmbientOcclusion * (1 - _AOWeight) + _AOWeight;
}