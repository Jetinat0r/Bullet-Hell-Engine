Shader "Custom/SpriteAnimationUnlit"
{
    Properties
    {
        _MainTex ("Texture Array", 2DArray) = "white" {}
        _ArrayIndex ("Array Index", Integer) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off


            HLSLPROGRAM
            #pragma require 2darray

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
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            float _ArrayIndex;
            float _UVScale;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = (v.vertex.xy + 0.5);
                o.uv.z = _ArrayIndex;
                
                return o;
            }
            /*
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, vertex);
                o.uv = vertex.xyz;
                return o;
            }
            */

            fixed4 frag (v2f i) : SV_Target
            {
                return UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.uv.xy, _ArrayIndex));
            }
            ENDHLSL
        }
    }
}
