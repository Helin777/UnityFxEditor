﻿Shader "HLFx/Smoothstep"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorStart("start",Vector)=(0,0,0,0)
        _ColorEnd("end",Vector)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _ColorStart;
            float4 _ColorEnd;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            
            float4 frag (v2f i) : SV_Target
            {
                float2 uv=i.uv;
                
                float4 col = tex2D(_MainTex, uv);
                col=smoothstep(_ColorStart,_ColorEnd,col);
                
                return col;
            }
            ENDCG
        }
    }
}
