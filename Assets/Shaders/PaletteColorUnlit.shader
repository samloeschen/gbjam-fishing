Shader "Unlit/PaletteColorUnlit"
{
    Properties
    {
        _PaletteTexture ("Palette Texture", 2D) = "white" {}
        _PaletteValue ("Palette Slider", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _PaletteTexture;
            uniform half _PaletteValue;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_PaletteTexture, float2(_PaletteValue, 0.5));
            }
            ENDCG
        }
    }
}
