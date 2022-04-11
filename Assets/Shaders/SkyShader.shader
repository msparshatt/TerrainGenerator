Shader "Unlit/SkyShader"
{
    Properties
    {
        _SunColor("Sun color", Color) = (1, 1, 1, 1)
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"
        _CloudStart("Cloud Start", float) = -1
        _CloudEnd("Cloud End", float) = 1
        _XOffset("X offset", float) = 0
        _YOffset("Y offset", float) = 0
        _Scale("Scale", float) = 5
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
            float4 _SunColor;
            float _XOffset;
            float _YOffset;
            float _Scale;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float value = snoise(float2((i.uv.x + _XOffset) * _Scale, (i.uv.y + _YOffset) * _Scale));
                value = SmoothStep(_CloudStart, _CloudEnd, value);


                fixed4 col = fixed4(_SunColor.xyz, value);

                
                return col;
            }
            ENDCG
        }
    }
}
