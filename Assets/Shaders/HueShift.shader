Shader "Custom/HueShiftPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShiftDistance("Shift Distance", Range(-1, 1)) = 0
        _ValueDistance("Value Distance", Range(-1, 1)) = 0


        _TintColor("Tint Color", Color) = (0,0,0,0)
        _TintStrength("Tint Strength", Float) = 0.0
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

            half3 rgb2hsv(half3 c)
            {
                half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
                half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            half3 hsv2rgb(half3 c)
            {
                half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            sampler2D _MainTex;
            float _ShiftDistance;
            float _ValueDistance;

            half4 _TintColor;
            half _TintStrength;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                half3 hsv = rgb2hsv(col.rgb);
                hsv.x += _ShiftDistance * lerp(1.0, 1.0, (1.0 - hsv.z));
                hsv.z += _ValueDistance;
                half3 rgb = hsv2rgb(hsv);

                // col.rgb = lerp(col.rgb, _TintColor, _TintStrength);
                // return col;

                return fixed4(rgb, col.a);
            }
            ENDCG
        }
    }
}
