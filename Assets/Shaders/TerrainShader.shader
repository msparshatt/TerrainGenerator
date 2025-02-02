// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AOTexture ("AmbientOcclusion", 2D) = "white" {}
        _PaintMask ("Paint Mask", 2D) = "white" {}
        _ContourMask ("Contour Mask", 2D) = "white" {}
        _OverlayTexture("Overlay", 2D) = "RGBA 0,0,0,0" {}
        _CursorLocation("Cursor Location", Vector) = (0, 0, 0.5, 0.5)
        _CursorTexture("Cursor Texture", 2D) = "white" {}
        _CursorRotation("Cursor Rotation", float) = 0

        _ApplyLighting("ApplyLighting", int) = 0

        _ApplyMask("ApplyMask", int) = 0
        _ApplyContours("ApplyContours", int) = 0
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
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

	            float3 normal : NORMAL;   
                float3 tangent : TANGENT;             
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 ov : TEXCOORD2;

                /*float3 lightdir : TEXCOORD2;

				float3 T : TEXCOORD3;
				float3 B : TEXCOORD4;
				float3 N : TEXCOORD5;*/
                fixed4 diff : COLOR0; // diffuse lighting color
            };

            sampler2D _MainTex;
            sampler2D _AOTexture;
            sampler2D _OverlayTexture;
            sampler2D _CursorTexture;
            sampler2D _PaintMask;
            sampler2D _ContourMask;
            float _CursorRotation;

            float4 _MainTex_ST;
            float4 _OverlayTexture_ST;
            float4 _CursorLocation;

            int _ApplyLighting;

            int _ApplyMask;
            int _ApplyContours;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.ov = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _OverlayTexture);

                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

                // factor in the light color
                o.diff = nl * _LightColor0;

                return o;
            }

            fixed4 Base(v2f i)
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed4 AO = tex2D(_AOTexture, i.uv);
                col *= AO;

                return col;
            }

            fixed4 Overlay(fixed4 col, v2f i)
            {
                fixed4 overlay =  tex2D(_OverlayTexture, i.uv2);

                col = (col * (1 - overlay.a)) + (overlay * overlay.a);

                return col;
            }

            fixed4 AddMask(fixed4 col, v2f i)
            {
                fixed4 mask = tex2D(_PaintMask, i.uv);

                if(_ApplyMask > 0) {
                    col = (col * 0.75) + (mask * 0.25);
                }

                return col;
            }

            fixed4 AddContours(fixed4 col, v2f i)
            {
                fixed4 mask = tex2D(_ContourMask, i.uv);

                if(_ApplyContours > 0) {
                    col = col * (1 - mask.a) + mask * mask.a;
                }
                return col;
            }

            fixed4 AddCursor(fixed4 col, v2f i)
            {
                float angle = radians(_CursorRotation);

                float centerX = _CursorLocation.x + _CursorLocation.z / 2;
                float centerY = _CursorLocation.y + _CursorLocation.w / 2;

                float diffX = i.uv2.x - centerX;
                float diffY = i.uv2.y - centerY;

                float rotdiffX = diffX * cos(angle) - diffY * sin(angle);
                float rotdiffY = diffX * sin(angle) + diffY * cos(angle);


                float rotx = centerX + rotdiffX;
                float roty = centerY + rotdiffY;
                if((rotx >= _CursorLocation.x && rotx <= _CursorLocation.x +  _CursorLocation.z) && 
                    (roty >= _CursorLocation.y && roty <= _CursorLocation.y + _CursorLocation.w)) {
                        float uvX = (rotx - _CursorLocation.x) / _CursorLocation.z;
                        float uvY = (roty - _CursorLocation.y) / _CursorLocation.w;



                        fixed4 brushCol = {0, 0, tex2D(_CursorTexture, float2(uvX, uvY)).b, 0};
                        brushCol.a = brushCol.b * 0.7;
                        col = (col * (1 - brushCol.a)) + (brushCol * brushCol.a);
                    }

                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = Base(i);          
                col = Overlay(col, i);

                if(_ApplyLighting > 0) {
                    float3 ambient = float3(0.1f, 0.1f, 0.1f) * 3 * col;

                    float3 diffuse = col * i.diff;
                    col = float4((ambient + diffuse), 1.0);
                }

                col = AddMask(col, i);
                col = AddContours(col, i);
                col = AddCursor(col, i);
                return col;
            }

            ENDCG
        }
    }
}
