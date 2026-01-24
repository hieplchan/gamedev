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

Shader "Hidden/Crest/Inputs/Shape Waves/Spline"
{
    CGINCLUDE
    #pragma vertex Vertex
    #pragma fragment Fragment
    // #pragma enable_d3d11_debug_symbols

    #include "UnityCG.cginc"

    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Macros.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Globals.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/InputsDriven.hlsl"
    #include "Packages/com.waveharmonic.crest/Runtime/Shaders/Library/Cascade.hlsl"
    #include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Common.hlsl"

    Texture2DArray _Crest_WaveBuffer;

    CBUFFER_START(CrestPerWaterInput)
    half _Crest_FeatherWaveStart;
    float _Crest_RespectShallowWaterAttenuation;
    int _Crest_WaveBufferSliceIndex;
    float _Crest_AverageWavelength;
    float _Crest_AttenuationInShallows;
    float _Crest_Weight;
    float2 _Crest_AxisX;
    half _Crest_MaximumAttenuationDepth;
    half _Crest_FeatherWidth;
    CBUFFER_END

    m_CrestNameSpace

    struct Attributes
    {
        float4 vertex : POSITION;
        float4 parameters : TEXCOORD0;
        float weight : TEXCOORD1;
    };

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        float3 uv_slice : TEXCOORD1;
        float2 axis : TEXCOORD2;
        float3 worldPosScaled : TEXCOORD3;
        float2 invNormDistToShoreline_weight : TEXCOORD4;
        float2 worldPosXZ : TEXCOORD5;
    };

    Varyings Vertex(Attributes v)
    {
        Varyings o;

        const float3 positionOS = v.vertex.xyz;
        o.vertex = UnityObjectToClipPos(positionOS);
        const float3 worldPos = mul( unity_ObjectToWorld, float4(positionOS, 1.0) ).xyz;

        m_Crest::Splines::Parameters unpacked = m_Crest::Splines::Unpack(v.parameters);

        // UV coordinate into the cascade we are rendering into
        o.uv_slice = Cascade::MakeAnimatedWaves(_Crest_LodIndex).WorldToUV(worldPos.xz);

        o.worldPosXZ = worldPos.xz;

        // World pos prescaled by wave buffer size, suitable for using as UVs in fragment shader
        const float waveBufferSize = 0.5f * (1 << _Crest_WaveBufferSliceIndex);
        o.worldPosScaled = worldPos / waveBufferSize;

        o.invNormDistToShoreline_weight.x = unpacked._UV.x;
        o.invNormDistToShoreline_weight.y = v.weight * _Crest_Weight;

        // Rotate forward axis around y-axis into world space
        o.axis = dot( unpacked._Axis, _Crest_AxisX ) * unity_ObjectToWorld._m00_m20 + dot( unpacked._Axis, float2(-_Crest_AxisX.y, _Crest_AxisX.x) ) * unity_ObjectToWorld._m02_m22;

        return o;
    }

    float4 Fragment(Varyings input)
    {
        float wt = input.invNormDistToShoreline_weight.y;

        // Feature at away from shore.
        wt *= min( input.invNormDistToShoreline_weight.x / _Crest_FeatherWaveStart, 1.0 );

        // Feather at front/back
        if (input.invNormDistToShoreline_weight.x > 0.5)
        {
            input.invNormDistToShoreline_weight.x = 1.0 - input.invNormDistToShoreline_weight.x;
        }
        wt *= min( input.invNormDistToShoreline_weight.x / _Crest_FeatherWidth, 1.0 );

        float alpha = wt;

        // Attenuate if depth is less than half of the average wavelength
        const half depth = Cascade::MakeDepth(_Crest_LodIndex).SampleSignedDepthFromSeaLevel(input.worldPosXZ) +
            Cascade::MakeLevel(_Crest_LodIndex).SampleLevel(input.worldPosXZ);
        half depth_wt = saturate(2.0 * depth / _Crest_AverageWavelength);
        if (_Crest_MaximumAttenuationDepth < k_Crest_MaximumWaveAttenuationDepth)
        {
            depth_wt = lerp(depth_wt, 1.0, saturate(depth / _Crest_MaximumAttenuationDepth));
        }
        const float attenuationAmount = _Crest_AttenuationInShallows * _Crest_RespectShallowWaterAttenuation;
        wt *= attenuationAmount * depth_wt + (1.0 - attenuationAmount);

        // Quantize wave direction and interpolate waves
        float axisHeading = atan2( input.axis.y, input.axis.x ) + 2.0 * 3.141592654;
        const float dTheta = 0.5*0.314159265;
        float angle0 = axisHeading;
        const float rem = fmod( angle0, dTheta );
        angle0 -= rem;
        const float angle1 = angle0 + dTheta;

        float2 axisX0; sincos( angle0, axisX0.y, axisX0.x );
        float2 axisX1; sincos( angle1, axisX1.y, axisX1.x );
        float2 axisZ0; axisZ0.x = -axisX0.y; axisZ0.y = axisX0.x;
        float2 axisZ1; axisZ1.x = -axisX1.y; axisZ1.y = axisX1.x;

        const float2 uv0 = float2(dot( input.worldPosScaled.xz, axisX0 ), dot( input.worldPosScaled.xz, axisZ0 ));
        const float2 uv1 = float2(dot( input.worldPosScaled.xz, axisX1 ), dot( input.worldPosScaled.xz, axisZ1 ));

        // Sample displacement, rotate into frame
        float3 disp0 = _Crest_WaveBuffer.SampleLevel( sampler_Crest_linear_repeat, float3(uv0, _Crest_WaveBufferSliceIndex), 0 ).xyz;
        float3 disp1 = _Crest_WaveBuffer.SampleLevel( sampler_Crest_linear_repeat, float3(uv1, _Crest_WaveBufferSliceIndex), 0 ).xyz;
        disp0.xz = disp0.x * axisX0 + disp0.z * axisZ0;
        disp1.xz = disp1.x * axisX1 + disp1.z * axisZ1;
        float3 disp = lerp( disp0, disp1, rem / dTheta );

        disp *= wt;

        return float4(disp, alpha);
    }

    m_CrestNameSpaceEnd

    m_CrestVertex
    m_CrestFragment(float4)
    ENDCG

    SubShader
    {
        // Either additive or alpha blend for geometry waves.
        Cull Back
        ZTest Always
        ZWrite Off

        Pass
        {
            Blend [_Crest_BlendSource] [_Crest_BlendTarget]
            CGPROGRAM
            ENDCG
        }

        Pass
        {
            Blend One One
            CGPROGRAM
            ENDCG
        }
    }
}
