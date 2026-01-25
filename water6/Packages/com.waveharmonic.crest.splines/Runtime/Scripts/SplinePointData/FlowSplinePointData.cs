// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for flow
    /// </summary>
    [AddComponentMenu("")]
    public sealed partial class FlowSplinePointData : SplinePointData
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        internal const float k_DefaultSpeed = 2f;

        [Tooltip("Flow velocity (speed of flow in direction of spline).\n\nCan be negative to flip direction.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FlowVelocity = k_DefaultSpeed;

        internal override Vector4 GetData(Vector4 _)
        {
            return new(_FlowVelocity, 0f, 0f, 0f);
        }
    }
}
