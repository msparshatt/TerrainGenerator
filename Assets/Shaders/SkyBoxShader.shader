// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SkyBoxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HorizonColor ("Horizon Color", Color) = (0.5, 0.5, 1, 1)
        _SkyColor ("Sky Color", Color) = (0.7, 0.7, 1, 1)
        _GroundColor ("Ground Color", Color) = (0.3, 0.7, 0.3, 1)
        _SunSize("Sun Size", float) = 0.2
        _SunColor("Sun Color", Color) = (1,1,1,1)
        _CutOff ("Cut Off", float) = 1
        _StarBrightness ("Star Brightness", float) = 1
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
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"
            #include "noiseSimplex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 eyeRay : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _HorizonColor;
            float4 _GroundColor;
            float4 _SkyColor;
            float _CutOff;
            float _SunSize;
            float4 _SunColor;
            float _StarBrightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                return o;
            }

            float SmoothStep(float start, float end, float value)
            {
                float lambda = max(0, (value - start) / (end - start));
                lambda = min(1, lambda);

                return (3 * lambda * lambda - 2 * pow(lambda, 3));
            }

            float InverseLerp(float start, float end, float value)
            {
                return min(1, max(0, (value - start) / (end - start)));               
            }


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float height = i.uv.y;

                fixed4 col = float4(1,1,1,1);

                if(height > 0) {
                    height = max(0, height);
                    height /= _CutOff;
                    height = min(height, 1.0);

                    col =  lerp(_HorizonColor, _SkyColor, height);

                    float scale = 100;
                    float star = snoise(float2(i.uv.x * scale, i.uv.y * scale));
                    star = (max(0.95, star) - 0.95) * 20;
                    col += (1, 1, 1, 1) * star * _StarBrightness;

                    float4 sunPosition = _WorldSpaceLightPos0;

                    //float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, i.vertex.xyz));
                    //float sunIntensity = max(0, dot(-i.eyeRay, sunPosition));
                    float3 delta = i.eyeRay - sunPosition;
                    float sunIntensity = length(delta);
                    sunIntensity = 1 - SmoothStep(0.0, _SunSize, sunIntensity);

                    col += _LightColor0 * sunIntensity;
                } else {
                    col = _GroundColor;
                }
                return col;
            }
            ENDCG
        }
    }
}
