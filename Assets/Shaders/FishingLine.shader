﻿Shader "Unlit/FishingLine"
{
    Properties
    {
        _PaletteTexture("Palette Texture", 2D) = "white" {}
        _PaletteA("Palette A", Range(0, 1)) = 0
        _PaletteB("Palette B", Range(0, 1)) = 0
        _MidPoint("Mid Point", Range(0, 1)) = 0.5

    }

    SubShader
    {
        Tags { "Queue"="Overlay""RenderType"="Transparent" }
        LOD 100

        GrabPass
        {
            "_GrabTexture"
        }
        ZWrite Off
        // ZTest Always

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
                float4 grabPos : TEXCOORD2;
            };

            sampler2D _GrabTexture;
            sampler2D _PaletteTexture;
            float _PaletteA;
            float _PaletteB;
            float _MidPoint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half3 grabColor = tex2Dproj(_GrabTexture, i.grabPos).rgb;
                const float3 W = float3(0.2125, 0.7154, 0.0721);
                float s = lerp(_PaletteA, _PaletteB, step(dot(grabColor, W), _MidPoint));
                return tex2Dlod(_PaletteTexture, float4(s, 0.0, 0.0, 0.0));
            }
            ENDCG
        }
    }
}
