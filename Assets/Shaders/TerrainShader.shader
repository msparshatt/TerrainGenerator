// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AOTexture ("AmbientOcclusion", 2D) = "white" {}
        _ApplyAO("ApplyAO", Float) = 0
        _OverlayTexture("Overlay", 2D) = "RGBA 0,0,0,0" {}
        _CursorLocation("Cursor Location", Vector) = (0, 0, 0.5, 0.5)
        _CursorTexture("Cursor Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        // inside SubShader
 
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
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _ShapeTexture;
            sampler2D _MainTex;
            sampler2D _AOTexture;
            sampler2D _OverlayTexture;
            float4 _MainTex_ST;
            float4 _OverlayTexture_ST;
            int _ApplyAO;
            float4 _CursorLocation;
            sampler2D _CursorTexture;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _OverlayTexture);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture                
                fixed4 col = tex2D(_MainTex, i.uv);

                if(_ApplyAO > 0) {
                    col *= tex2D(_AOTexture, i.uv);
                } 

                fixed4 overlay =  tex2D(_OverlayTexture, i.uv2);

                col = (col * (1 - overlay.a)) + (overlay * overlay.a);

                if((i.uv2.x >= _CursorLocation.x && i.uv2.x <= _CursorLocation.x +  _CursorLocation.z) && 
                    (i.uv2.y >= _CursorLocation.y && i.uv2.y <= _CursorLocation.y + _CursorLocation.w)) {
                        float uvX = (i.uv2.x - _CursorLocation.x) / _CursorLocation.z;
                        float uvY = (i.uv2.y - _CursorLocation.y) / _CursorLocation.w;
                        fixed4 brushCol = {0, 0, tex2D(_CursorTexture, float2(uvX, uvY)).b, 0};
                        brushCol.a = brushCol.b * 0.7;
                        col = (col * (1 - brushCol.a)) + (brushCol * brushCol.a);
                    }
//                fixed4 col = float4(i.uv.x, i.uv.y, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
