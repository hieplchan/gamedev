// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Foam tweakable param on spline points
    /// </summary>
    [AddComponentMenu("")]
    public sealed partial class FoamSplinePointData : SplinePointData
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal const float k_DefaultAmount = 1f;

        [Tooltip("Amount of foam emitted.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FoamAmount = k_DefaultAmount;

        internal override Vector4 GetData(Vector4 _)
        {
            return new(_FoamAmount, 0f, 0f, 0f);
        }
    }
}
