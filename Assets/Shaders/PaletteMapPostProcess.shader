Shader "PostProcess/PaletteMapPostProcess" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _PaletteMap("Palette Map", 2D) = "white" {}
        _Exponent("Exponent", Float) = 3
        _SkyColor("Sky Color", Color) = (0,0,0,0) 
    }
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _PaletteMap;
            uniform sampler2D _MainTex;
            uniform float _Exponent;
            uniform sampler2D _CameraDepthTexture;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half luminance (half3 rgb) {
               half3 W = half3(0.2125, 0.7154, 0.0721);
                return dot(rgb, W);
            }

            fixed4 frag (v2f i) : SV_Target {
                half4 c = tex2D(_MainTex, i.uv);
                half l = luminance(c);
                half map = lerp(0.25, 1.0, (pow(l, _Exponent)));
                map *= step(0.25, l);
                return tex2D(_PaletteMap, half2(map, 0));
            }
            ENDCG
        }
    }
}
