﻿Shader "HLFx/TextureColorTransparentPM"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Texture",2D)="white"{}
        _Color("Color",Color)=(1,1,1,1)
        _OpacityRef("OpacityRef",Range(0,1))=0.9
            
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend One OneMinusSrcAlpha // Premultiplied transparency

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
                float2 uvmask : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            
            float4 _Color;

            float _OpacityRef;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvmask = TRANSFORM_TEX(v.uv, _MaskTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float mask = tex2D(_MaskTex, i.uvmask);
                col*=_Color;

                col.w*=mask;
                col.xyz*=col.w;
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //col=fixed4(col.w,col.w,col.w,1);
                if(col.w<=_OpacityRef)discard;
                return col;
            }
            ENDCG
        }
    }
}
