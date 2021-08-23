Shader "Unlit/BrushShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
               "Queue" = "Transparent"}
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend One One //additive

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

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
                fixed4 col = {0, 0, tex2D(_MainTex, i.uv).b, 0};
                if(col.r > 0) {
                    col.a = 0.2f;
                    //col.a = 1.0f;
                } else {
                    col.a = 0.0f;
                }
                return col;
            }
            ENDCG
        }
    }
}
