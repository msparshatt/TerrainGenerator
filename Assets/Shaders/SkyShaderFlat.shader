Shader "Unlit/SkyShaderFlat"
{
    Properties
    {
        _CloudColor("Cloud Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _CloudStart("Cloud Start", float) = -1
        _CloudEnd("Cloud End", float) = 1
        _XOffset("X offset", float) = 0
        _YOffset("Y offset", float) = 0
        _Scale("Scale", float) = 5
        _Iterations("Iterations", int) = 5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "noiseSimplex.cginc"
            #include "UnityLightingCommon.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CloudStart;
            float _CloudEnd;
            float _XOffset;
            float _YOffset;
            float _Scale;
            float _Iterations;
            float4 _CloudColor;

            float SmoothStep(float start, float end, float value)
            {
                float lambda = max(0, (value - start) / (end - start));
                lambda = min(1, lambda);

                return (3 * lambda * lambda - 2 * pow(lambda, 3));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 0);

                float value = 0;
                float amplitude = 1;

                for(int index = 0; index < _Iterations; index++) {
                    value += snoise(float2((i.uv.x + _XOffset) * _Scale, (i.uv.y + _YOffset) * _Scale)) * amplitude;
                    _Scale *= 2;
                    amplitude /= 2;
                }

                value = SmoothStep(_CloudStart, _CloudEnd, value);

                if(i.uv.x < 0.1)
                    value *= (i.uv.x * 10);
                if(i.uv.x > 0.9)
                    value *= ((1 - i.uv.x) * 10);
                if(i.uv.y < 0.1)
                    value *= (i.uv.y * 10);
                if(i.uv.y > 0.9)
                    value *= ((1 - i.uv.y) * 10);

                col = fixed4(lerp(_LightColor0, _CloudColor.xyz, value) , value);
                
                return col;
            }
            ENDCG
        }
    }
}
