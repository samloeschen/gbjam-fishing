Shader "Unlit/FishingLine"
{
    Properties
    {
        _PaletteTexture("Palette Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        GrabPass
        {
            "_GrabTexture"
        }

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
                float s = step(dot(grabColor, W), 0.5);
                return tex2Dlod(_PaletteTexture, float4(s, 0.0, 0.0, 0.0));
            }
            ENDCG
        }
    }
}
