// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#include "Packages/com.waveharmonic.crest.splines/Runtime/Shaders/Library/Macros.hlsl"

m_SplinesNameSpace

struct Parameters
{
    float2 _UV;
    float2 _Axis;
};

Parameters Unpack(float4 packed)
{
    Parameters unpacked;
    unpacked._UV = packed.xy;
    unpacked._Axis = packed.zw;
    return unpacked;
}

m_SplinesNameSpaceEnd
