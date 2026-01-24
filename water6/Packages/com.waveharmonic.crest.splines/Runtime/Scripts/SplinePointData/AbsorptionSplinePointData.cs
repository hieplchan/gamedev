// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Custom spline point data for scattering.
    /// </summary>
    [AddComponentMenu("")]
    public sealed partial class AbsorptionSplinePointData : SplinePointData
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("Whether to override the scattering color instead of just the weight.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        bool _OverrideAbsorption;

        [Tooltip("The scattering color.")]
        [@Predicated(nameof(_OverrideAbsorption), hide: true)]
        [@GenerateAPI(Setter.Custom)]
        [@DecoratedField, SerializeField]
        Color _AbsorptionColor = s_DefaultAbsorption;

        [Tooltip("The weight of the scattering color.")]
        [@Predicated(nameof(_OverrideAbsorption), hide: true, inverted: true)]
        [@Range(0, 1)]
        [@GenerateAPI]
        [SerializeField]
        float _Weight = 1f;

        Vector4 _Absorption = WaterRenderer.UpdateAbsorptionFromColor(s_DefaultAbsorption);

        internal static readonly Color s_DefaultAbsorption = AbsorptionLod.s_DefaultColor;

        internal override Vector4 GetData(Vector4 data)
        {
            data.w = _Weight;
            return _OverrideAbsorption ? _Absorption : data;
        }

        void SetAbsorptionColor(Color previous, Color current)
        {
            if (previous == current) return;
            _Absorption = WaterRenderer.UpdateAbsorptionFromColor(current);
        }

        private protected override void Initialize()
        {
            base.Initialize();

            _Absorption = WaterRenderer.UpdateAbsorptionFromColor(_AbsorptionColor);
        }

#if UNITY_EDITOR
        private protected override void OnChange(string path, object previous)
        {
            switch (path)
            {
                case nameof(_AbsorptionColor):
                    SetAbsorptionColor((Color)previous, _AbsorptionColor);
                    break;
            }

            base.OnChange(path, previous);
        }
#endif
    }
}
