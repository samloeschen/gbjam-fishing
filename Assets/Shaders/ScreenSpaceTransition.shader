Shader "PostProcess/ScreenSpaceTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Palette ("Palette", 2D) = "white" {}
        _PaletteLookup("Palette Lookup", Range(0, 1)) = 0
        _PatternDensity("Pattern Density", Float) = 10
        _RadiusAnimation("Radius Animation", Range(0, 1)) = 0.5 
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _Palette;
            uniform float _PaletteLookup;
            uniform float _Aspect;
            uniform float _PatternDensity;
            uniform float _RadiusAnimation;
            uniform float2 _ScrollSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 patternUV: TEXCOORD2;
                float4 vertex : SV_POSITION;
                float4 bgColor: TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.patternUV = v.uv;
                o.patternUV -= 0.5;
                o.patternUV.x *= _Aspect;
                o.patternUV += 0.5;
                
                o.bgColor = tex2Dlod(_Palette, float4(_PaletteLookup, 0, 0, 0));
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                i.patternUV = frac(i.patternUV * _PatternDensity + _Time.y * _ScrollSpeed);
                fixed4 col = tex2D(_MainTex, i.uv);
                float r = length(i.patternUV - 0.5);
                if (r > 1.0 * _RadiusAnimation) {
                    col = i.bgColor;
                }

                return col;
            }
            ENDCG
        }
    }
}
