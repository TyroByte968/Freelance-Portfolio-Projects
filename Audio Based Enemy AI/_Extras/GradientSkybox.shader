Shader "FlatKit/GradientSkybox" {

    Properties {
        _Color2 ("Top Color", Color) = (0.97, 0.67, 0.51, 0)
        _Color1 ("Bottom Color", Color) = (0, 0.7, 0.74, 0)

        [Space]
        _Intensity ("Intensity", Range (0, 2)) = 1.0
        _Exponent ("Exponent", Range (0, 3)) = 1.0

        [Space]
        _DirectionYaw ("Direction X angle", Range (0, 180)) = 0
        _DirectionPitch ("Direction Y angle", Range (0, 180)) = 0

        //Added these values
        _SunPosition("Sun Position", Vector) = (0.0, 0.0, 1.0)

       [HDR] _SunColor("Sun Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SunSize("Sun Size", Range(0, 1)) = 0.04
        _SunHardness("Sun Hardness", Float) = 0.1
        _SunGradient("Sun Gradient", Range(0, 1)) = 0.0
        
        [HideInInspector]
        _Direction ("Direction", Vector) = (0, 1, 0, 0)
    }

    CGINCLUDE

    #include "Lighting.cginc"
    #include "UnityCG.cginc"

    //added these as well
    uniform half3 _SunPosition, _SunColor;
    uniform half _SunSize, _SunHardness;

    #define HARDNESS_EXPONENT_BASE 0.125

    struct appdata {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };
    
    struct v2f {
        float4 position : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

    half3 calcSunSpot(half3 sunDirPos, half3 skyDirPos)
    {
        half3 delta = sunDirPos - skyDirPos;
        half dist = length(delta);
        half spot = 1.0 - smoothstep(0.0, _SunSize, dist);
        return 1.0 - pow(HARDNESS_EXPONENT_BASE, spot * _SunHardness);
    }

    
    half4 _Color1;
    half4 _Color2;
    half3 _Direction;
    half _Intensity;
    half _Exponent;
    
    v2f vert (appdata v) {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = v.texcoord;
        return o;
    }
    
    //changed this from fixed4 to half4
    half4 frag (v2f i) : COLOR {
        half d = dot(normalize(i.texcoord), _Direction) * 0.5f + 0.5f;

        half3 mie = calcSunSpot(_SunPosition.xyz, i.texcoord.xyz) * _SunColor;

        
        //need to return the sun function here, the half3 mie but it throws up an error
        //"cannot implictly convert from half3 to half4" 
        // I understand the meaning but not how to fix it.

        half3 col = lerp (_Color1, _Color2, pow(d, _Exponent)) * _Intensity + mie;

        return half4(col,1.0); 
    }

    ENDCG

    SubShader {
        Tags { "RenderType"="Background" "Queue"="Background" }

        Pass {
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    CustomEditor "GradientSkyboxEditor"
}
