// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/TerrainShader_withnormal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTexture("Overlay", 2D) = "RGBA 0,0,0,0" {}
    	_NormalMap("NormalMap", 2D) = "white" {}
        _MainLightPosition("MainLightPosition", Vector) = (0,0,0,0)
        _LightColor("LightColor", Color) = (1,1,1,1)
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
                float4 vertex : SV_POSITION;
                float3 lightdir : TEXCOORD1;

				float3 T : TEXCOORD2;
				float3 B : TEXCOORD3;
				float3 N : TEXCOORD4;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTexture;
            float4 _MainTex_ST;
            uniform sampler2D _NormalMap;
            uniform float3 _MainLightPosition;
            uniform float4 _LightColor;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
				float3 lightDir = worldPosition.xyz - _MainLightPosition.xyz;
				o.lightdir = normalize(lightDir);
                float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);
				
				float3 binormal = cross(v.normal, v.tangent.xyz); // *input.tangent.w;
				float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);
                
                o.N = normalize(worldNormal);
				o.T = normalize(worldTangent);
				o.B = normalize(worldBinormal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 tangentNormal = tex2D(_NormalMap, i.uv).xyz;
				// and change range of values (0 ~ 1)
				tangentNormal = normalize(tangentNormal * 2 - 1);

				// 'TBN' transforms the world space into a tangent space
				// we need its inverse matrix
				// Tip : An inverse matrix of orthogonal matrix is its transpose matrix
				float3x3 TBN = float3x3(normalize(i.T), normalize(i.B), normalize(i.N));
				TBN = transpose(TBN);

				// finally we got a normal vector from the normal map
				float3 worldNormal = mul(TBN, tangentNormal);

                // sample the texture                
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 overlay =  tex2D(_OverlayTexture, i.uv);

                col = (col * (1 - overlay.a)) + (overlay * overlay.a);

                float3 lightDir = normalize(i.lightdir);
				// calc diffuse, as we did in pixel shader
				float3 diffuse = saturate(dot(worldNormal, lightDir));
				diffuse = col.rgb * diffuse *_LightColor;

                float3 ambient = float3(0.1f, 0.1f, 0.1f) * 3 * col;

                return half4((ambient + diffuse), 1.0f);
            }
            ENDCG
        }
    }
}
