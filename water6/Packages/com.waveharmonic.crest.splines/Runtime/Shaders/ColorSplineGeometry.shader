// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

Shader "Hidden/Crest/Inputs/Scattering/Spline Geometry"
{
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Back
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"
            #include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Common.hlsl"

            CBUFFER_START(CrestPerWaterInput)
            float _Crest_Weight;
            float _Crest_FeatherWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 _PositionOS : POSITION;
                float4 _Parameters : TEXCOORD0;
                // We could use COLOR, but the potential quirks are unknown.
                float4 _Color : TEXCOORD1;
            };

            struct Varyings
            {
                float4 _PositionCS : SV_POSITION;
                float _CoordinatesU : TEXCOORD0;
                float4 _Color : TEXCOORD1;
            };

            Varyings Vertex(const Attributes i_Input)
            {
                Varyings output;
                output._PositionCS = UnityObjectToClipPos(float4(i_Input._PositionOS, 1.0));
                m_Crest::Splines::Parameters unpacked = m_Crest::Splines::Unpack(i_Input._Parameters);
                output._CoordinatesU = unpacked._UV.x;
                output._Color = i_Input._Color;
                return output;
            }

            half4 Fragment(const Varyings i_Input) : SV_Target
            {
                float4 color = i_Input._Color;
                float distance = i_Input._CoordinatesU;

                // Feather at front/back.
                if (distance > 0.5)
                {
                    distance = 1.0 - distance;
                }

                color.a *= min(distance / _Crest_FeatherWidth, 1.0);
                color.a *= _Crest_Weight;

                return color;
            }
            ENDCG
        }
    }
}
