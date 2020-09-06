Shader "PostProcess/PixelPerfectScale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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


            uniform sampler2D _MainTex;
            uniform sampler2D _Frame;
            uniform float4 _Frame_TexelSize;

            uniform float _ViewportAspect;
            uniform float _FrameAspect;

            uniform float _PixelRatio;

            uniform float4 _ViewportSize;
            uniform float4 _FrameSize;

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

                float frameRatio = _FrameAspect / _ViewportAspect;
                float2 scale = 1.0;
                if (_ViewportAspect < _FrameAspect) {
                    scale.y = frameRatio;
                } else {
                    scale.x = 1.0 / frameRatio;
                }
                o.uv -= 0.5;
                o.uv *= 1.0 + (frac(_PixelRatio) / floor(_PixelRatio)); // NOTE this doesn't work with some viewport sizes but whatever
                o.uv *= scale;
                o.uv += 0.5;

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                // return float4(i.uv, 0.0, 0.0);
                float3 color = tex2D(_Frame, i.uv).rgb;
                color *= step(abs(i.uv.x - 0.5), 0.5);
                color *= step(abs(i.uv.y - 0.5), 0.5);
                return float4(color, 1.0);
                

                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
