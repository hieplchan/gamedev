// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for scattering.
    /// </summary>
    [AddComponentMenu("")]
    public sealed partial class ScatteringSplinePointData : SplinePointData
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("Whether to override the scattering color instead of just the weight.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        bool _OverrideScattering;

        [Tooltip("The scattering color.")]
        [@Predicated(nameof(_OverrideScattering), hide: true)]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        Color _Scattering = s_DefaultScattering;

        [Tooltip("The weight of the scattering color.")]
        [@Predicated(nameof(_OverrideScattering), hide: true, inverted: true)]
        [@Range(0, 1)]
        [@GenerateAPI]
        [SerializeField]
        float _Weight = 1f;

        internal static readonly Color s_DefaultScattering = ScatteringLod.s_DefaultColor;

        internal override Vector4 GetData(Vector4 data)
        {
            data.w = _Weight;
            return _OverrideScattering ? _Scattering.MaybeLinear() : data;
        }
    }
}
