Shader "Custom/Anime"
{
    Properties
    {
        //[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _GlossinessMap ("Smoothness Map", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _ParTex ("Parameter Texture", 2D) = "white" {}
        
        _BumpMap ("Normal Map", 2D) = "bump" {}
        
        [HDR] _Emission("Emission", Color) = (0,0,0,1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        _EmissionIntensity("Emission Intensity", Range(0, 10)) = 0.0
        
        _AOMap("Occlusion Map", 2D) = "white" {}
        _AOWeight("Ambient Occlusion", Range(0, 1)) = 0
        
        _ShadowAttenuation ("Shadow Attenuation", Range(0,1)) = 0.0
        _HighDivision("High Division", Range(0, 1)) = 0.0
        _MidDivision("Mid Division", Range(0, 1)) = 0.0
        _LowDivision("Dark Division", Range(0, 1)) = 0.0
        
        _SpecMap("Specular Map", 2D) = "white" {}
        _SpecDivision("Specular Division", Range(0, 1)) = 0.0
        _SpecIntensity("Specular Intensity", Range(0, 1)) = 1.0
        
        _FresnelIntensity("Fresnel Effect", Range(0, 1)) = 1.0
        
        _ScatteringColor ("Subsurface Scattering Color", Color) = (1,0,0,1)
		_ScatteringColorSub("Subsurface Scattering 2nd Color", Color) = (0.8,0,0.2,1)
		_ScatteringWeight ("Subsurface Scattering Weight", Range(0,1)) = 0.5
		_ScatteringSize("Subsurface Scattering Size", Range(0,1)) = 0.5
		_ScatteringAttenuation("Forward Scattering Attenuation", Range(0,1)) = 0.5
        
		_OutlineWidth("Outline Width", Range(0, 0.5)) = 0.025
		_OutlineColor("Outline Color", Color) = (0.0,0.0,0.0,1.0)
		
        // Settings
		[HideInInspector] _Fold("__fld", Float) = 3.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //Cull [_Cull]
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon fullforwardshadows

        #pragma shader_feature _PARAMETER_TEXTURE
        #pragma shader_feature _SUBSURFACE_SCATTERING

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #define _NORMAL_MAP 1
        
        #include "AnimeInc.cginc"

        ENDCG
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
    
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
        
            #pragma shader_feature _OUTLINE_ENABLED

			#include "UnityCG.cginc"
            #include "OutlineInc.cginc"

			ENDCG

		}//Pass
    }
    FallBack "Diffuse"
    
    CustomEditor "CartoonShaderGUI"
}
