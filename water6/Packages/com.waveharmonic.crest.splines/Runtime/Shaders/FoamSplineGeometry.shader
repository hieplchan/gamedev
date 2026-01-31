// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

Shader "Hidden/Crest/Inputs/Foam/Spline Geometry"
{
    SubShader
    {
        Blend [_Crest_BlendSource] [_Crest_BlendTarget]
        BlendOp [_Crest_BlendOperation]
        Cull Back
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            CBUFFER_START(CrestPerWaterInput)
            float _Crest_SimDeltaTime;
            float3 _Crest_DisplacementAtInputPosition;
            float _Crest_Weight;
            half _Crest_FeatherWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float invNormDistToShoreline : TEXCOORD1;
                float weight : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 invNormDistToShoreline_weight : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;

                float3 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
                // Correct for displacement
                worldPos.xz -= _Crest_DisplacementAtInputPosition.xz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                o.invNormDistToShoreline_weight.x = input.invNormDistToShoreline;

                o.invNormDistToShoreline_weight.y = input.weight;

                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float wt = input.invNormDistToShoreline_weight.y;

                // Feather at front/back
                if( input.invNormDistToShoreline_weight.x > 0.5 )
                {
                    input.invNormDistToShoreline_weight.x = 1.0 - input.invNormDistToShoreline_weight.x;
                }
                wt *= min( input.invNormDistToShoreline_weight.x / _Crest_FeatherWidth, 1.0 );

                return 0.25 * wt * _Crest_SimDeltaTime;
            }
            ENDCG
        }
    }
}
