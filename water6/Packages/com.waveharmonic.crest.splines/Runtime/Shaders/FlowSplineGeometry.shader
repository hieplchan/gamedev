// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// Generates waves from geometry that is rendered into the water simulation from a top down camera. Expects
// following data on verts:
//   - POSITION: Vert positions as normal.
//   - TEXCOORD0: Axis - direction for waves to travel. "Forward vector" for waves.
//   - TEXCOORD1: X - 0 at start of waves, 1 at end of waves
//
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ uv1.x = 0             |
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  |                    |  uv0 - wave direction vector
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  |                   \|/
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ uv1.x = 1
//  ------------------- shoreline --------------------
//

Shader "Hidden/Crest/Inputs/Flow/Spline Geometry"
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
            // #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Globals.hlsl"
            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/InputsDriven.hlsl"
            #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Helpers.hlsl"
            #include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Common.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 parameters : TEXCOORD0;
                float speed : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 axis : TEXCOORD0;
                float uCoordinates : TEXCOORD1;
                float speed : TEXCOORD2;
            };

            CBUFFER_START(CrestPerWaterInput)
            half _Crest_FeatherWidth;
            float _Crest_Weight;
            CBUFFER_END

            Varyings Vert(Attributes v)
            {
                Varyings o;

                const float3 positionOS = v.positionOS;
                o.positionCS = UnityObjectToClipPos(positionOS);

                m_Crest::Splines::Parameters unpacked = m_Crest::Splines::Unpack(v.parameters);

                o.uCoordinates = unpacked._UV.x;

                // Rotate local-space sideays axis around y-axis, by 90deg, and by object to world to move into world space
                o.axis = unpacked._Axis.y * unity_ObjectToWorld._m00_m20 - unpacked._Axis.x * unity_ObjectToWorld._m02_m22;

                o.speed = v.speed;

                return o;
            }

            float2 Frag(Varyings input) : SV_Target
            {
                float wt = _Crest_Weight;

                // Feather at front/back
                if( input.uCoordinates > 0.5 ) input.uCoordinates = 1.0 - input.uCoordinates;
                wt *= min( input.uCoordinates / _Crest_FeatherWidth, 1.0 );

                return wt * input.speed * input.axis;
            }
            ENDCG
        }
    }
}
