Shader "Decalery/DecalerySimpleURPNormalOnly"
{
    Properties
    {
        _MainTex ("Opacity map (A)", 2D) = "white" {}
        _BumpMap ("Normal map", 2D) = "bump" {}

        _Fade ("Fade", Range(0,1)) = 1.0

        _Bias ("Bias", Range(0,0.1)) = 0.003

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" "ForceNoShadowCasting"="True" "LightMode"="UniversalGBuffer" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ INDIRECT_DRAW

            #include "UnityCG.cginc"

#ifdef INDIRECT_DRAW
            #include "decalShared.cginc"
#endif

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;

                float3 normal : NORMAL;
                float4 tangent : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float fade : COLOR0;

                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 binormal : TEXCOORD3;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex, _BumpMap;
            float _Fade;
            float _Bias;

            v2f vert (appdata v, uint vertexID : SV_VertexID)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.fade = v.color;

#ifdef INDIRECT_DRAW

                v.vertex.xyz = GetDecalLocalPos(vertexID);
                v.uv = GetDecalUV0(vertexID);
                o.fade = GetDecalFade(vertexID);
#endif
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1)).xyz;
                worldPos += normalize(_WorldSpaceCameraPos - worldPos) * _Bias;// 0.001f; // hardcoded, but pretty good
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos,1));
                o.uv = v.uv;

                float sign = v.tangent.w * (unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0);
                o.normal =  normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.tangent = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
                o.binormal = cross(o.normal, o.tangent) * sign;

                return o;
            }

            float4 frag (v2f IN) : SV_Target2
            {
                float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv));
                normal = mul(normal, float3x3(IN.tangent, IN.binormal, IN.normal));

                float alpha = tex2D(_MainTex, IN.uv).a;
                alpha *= IN.fade * _Fade;

                return float4(normal, alpha);
            }
            ENDCG
        }
    }
}
