Shader "Custom/water"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Albedo ("Albedo", Range(0, 1)) = 0.5
        _Exponent ("Exponent", Float) = 4.0

        _PaletteTexture("PaletteTexture", 2D) = "white" {}

        [Header(Normals Calculation)]
        _NormalsOffsetMin("Normals Offset Min", Float) = 0.5
        _NormalsOffsetMax("Normals Offset Max", Float) = 0.5
        _NormalsOffsetExponent("Normaals Offset Exponent", Float) = 1.0

        _BaseDisplacement("Base Displacement", Float) = 1.0
        _BaseScale("Base Scale", Vector) = (1,1,1,1)

        [Header(Displacement)]
        _Displacement("Displacement", Float) = 1.0
        _DisplacementTexture("Displacement Texture", 2D) = "white" {}
        _DisplacementSpeed("Displacement Speed", Vector) = (1,1,0,0)
        _DisplacementMagnitude("Displacement Strength", Float) = 1.0

        [Header(Noise A)]
        _NoiseTextureA("Noise Texture A", 2D) = "white" {}
        _NoiseSpeedA("Noise Speed", Vector) = (1,1,0,0)
        _NoiseContrastA("Noise Contrast", Float) = 1.0
        _NoiseMagnitudeA("Noise Magnitude", Float) = 1.0


        [Header(Noise B)]
        _NoiseTextureB("Noise Texture A", 2D) = "white" {}
        _NoiseSpeedB("Noise Speed", Vector) = (1,1,0,0)
        _NoiseContrastB("Noise Contrast", Float) = 1.0
        _NoiseMagnitudeB("Noise Magnitude", Float) = 1.0

        

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite On
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardPalette vertex:vert

        #include "UnityPBSLighting.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        uniform half _Metallic;
        uniform half _Glossiness;
        uniform half _Albedo;
        uniform half _Exponent;


        // normals calculation
        uniform float _NormalsOffsetMin;
        uniform float _NormalsOffsetMax;
        uniform float _NormalsOffsetExponent;

        // vertex base displacement
        uniform float2 _BaseScale;
        uniform float _BaseDisplacement;

        // displacement properties
        uniform float _Displacement;
        uniform sampler2D _DisplacementTexture;
        uniform float4 _DisplacementTexture_ST;
        uniform float2 _DisplacementSpeed;
        uniform float _DisplacementMagnitude;

        // noise properties
        uniform sampler2D _NoiseTextureA;
        uniform float4 _NoiseTextureA_ST;
        uniform float2 _NoiseSpeedA;
        uniform float _NoiseContrastA;
        uniform float _NoiseMagnitudeA;

        uniform sampler2D _NoiseTextureB;
        uniform float4 _NoiseTextureB_ST;
        uniform float2 _NoiseSpeedB;
        uniform float _NoiseContrastB;
        uniform float _NoiseMagnitudeB;

        uniform sampler2D _CameraDepthTexture;

        uniform sampler2D _PaletteTexture;

        float noiseSample(float2 p, sampler2D tex, float2 speed, float contrast, float magnitude, float2 scale, float2 displacement) {
            float4 tc = float4(p * scale + displacement + _Time.y * speed, 0.0, 0.0);
            float n = tex2Dlod(tex, tc);
            n = (saturate(lerp(0.5, n, contrast)) * 2.0 - 1.0) * magnitude;
            return n;
        }

        float noiseOffset(float2 p) {
            float4 dispCoord = float4(p * _DisplacementTexture_ST.xy + _Time.y * _DisplacementSpeed, 0.0, 0.0);
            float2 displacement = tex2Dlod(_DisplacementTexture, dispCoord).xy * 0.5 + 0.5;
            displacement *= _DisplacementMagnitude;

            float noiseA = noiseSample(p, _NoiseTextureA, _NoiseSpeedA, _NoiseContrastA, _NoiseMagnitudeA, _NoiseTextureA_ST, displacement);
            float noiseB = noiseSample(p, _NoiseTextureB, _NoiseSpeedB, _NoiseContrastB, _NoiseMagnitudeB, _NoiseTextureB_ST, displacement);

            return noiseA * noiseB;
        }

        half luminance (half3 rgb) {
            half3 W = half3(0.2125, 0.7154, 0.0721);
            return dot(rgb, W);
        }

        half4 LightingStandardPalette(SurfaceOutputStandard s, float3 viewDir, UnityGI gi) {
            half3 c = LightingStandard(s, viewDir, gi);
            half l =  pow(luminance(c), _Exponent);

            // c.rgb = max(0.2, c);

            // c.rgb = lerp(0.125, 1.0, pow(luminance(c), 2.0));

            return float4(c, 1.0);
            // return tex2D(_PaletteTexture, half2(l, 0));
        }

        void LightingStandardPalette_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi) {
            LightingStandard_GI(s, data, gi);
        }

        void vert(inout appdata_full v) {

            float4 v0 = v.vertex;
            float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(v0.xyz));
            float depth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(screenPos.xy / screenPos.w, 0.0, 0.0)));
            
            // todo interpolate normal central differences based on depth
            float h = lerp(_NormalsOffsetMin, _NormalsOffsetMax, pow(depth, _NormalsOffsetExponent));
            float4 v1 = v0 + float4(h, 0.0, 0.0, 0.0);
            float4 v2 = v0 + float4(0.0, 0.0, h, 0.0);

            float vertexOffset = noiseOffset(mul(unity_ObjectToWorld, v0).xz);

            v0.y += noiseOffset(mul(unity_ObjectToWorld, v0).xz * _BaseScale) * _BaseDisplacement;
            v1.y += noiseOffset(mul(unity_ObjectToWorld, v1).xz * _BaseScale) * _BaseDisplacement;
            v2.y += noiseOffset(mul(unity_ObjectToWorld, v2).xz * _BaseScale) * _BaseDisplacement;

            float3 vn = cross(v2.xyz - v0.xyz, v1.xyz - v0.xyz);
            v.normal = normalize(vn);
            v.vertex = v0;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Albedo;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
