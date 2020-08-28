Shader "Unlit/SplashParticles"
{
    Properties
    {
        _PaletteTexture("Palette Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Blend One OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float _PaletteIndex;
            uniform sampler2D _PaletteTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color: TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = tex2Dlod(_PaletteTexture, float4(v.color.r, 0.0, 0.0, 0.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float len = length(i.uv - 0.5);
                return step(len, 0.5) * i.color;
            }
            ENDCG
        }
    }
}
