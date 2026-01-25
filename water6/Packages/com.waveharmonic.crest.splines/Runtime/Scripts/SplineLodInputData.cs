// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Data storage for for the Spline input mode.
    /// </summary>
    [System.Serializable]
    public abstract partial class SplineLodInputData : LodInputData, IReceiveSplineChangeMessages
    {
        [Tooltip("The <i>Crest Spline</i> to use with this input.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal Spline _Spline;

        [Tooltip("Whether to override the settings with the same name on the spline component.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal bool _OverrideSplineSettings = false;

        [Tooltip("The radius of the spline.")]
        [@Predicated(nameof(_OverrideSplineSettings))]
        [@GenerateAPI]
        [UnityEngine.Serialization.FormerlySerializedAs("_Width")]
        [@DecoratedField, SerializeField]
        internal float _Radius = 20f;

        [Tooltip("Increasing subdivision increases the geometry density.\n\nMostly useful for water level changes. High values can reduce staircasing effect.")]
        [@Predicated(nameof(_OverrideSplineSettings))]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal int _Subdivisions = 1;

        // Why not a spline reference? Meshes will have custom data per input so they cannot be shared on a spline.
        internal Mesh _Mesh;
        internal Vector3[] _SplineBoundingPoints = new Vector3[0];
        internal Material _Material;

        // Used to collapse spline change requests.
        private protected bool _IsDirty;

        /// <summary>
        /// The mesh generated from the spline.
        /// </summary>
        /// <remarks>
        /// The mesh should be available by Start.
        /// </remarks>
        public Mesh Mesh => _Mesh;

        private protected abstract void CreateOrUpdateSplineMesh();

        private protected abstract Shader SplineShader { get; }
        private protected abstract Vector4 DefaultCustomSplineData { get; }

        internal override bool IsEnabled => _Spline != null && _Material != null;

        internal override void RecalculateRect()
        {
            if (_SplineBoundingPoints.Length < 2)
            {
                _Rect = Rect.zero;
            }
            else
            {
                var bounds = Bounds;
                _Rect = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
            }
        }

        internal override void RecalculateBounds()
        {
            if (_SplineBoundingPoints.Length > 1)
            {
                _Bounds = GeometryUtility.CalculateBounds(_SplineBoundingPoints, _Input.transform.localToWorldMatrix);
            }
        }

        internal override void OnEnable()
        {
            CreateOrUpdateSplineMesh();
        }

        internal override void OnDisable()
        {
            // Blank
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();

            // Call here as it will update bounds which reporters depend on. They are called in
            // update, but after data update.
            if (_IsDirty)
            {
                CreateOrUpdateSplineMesh();
            }

            if (_Material == null) return;
            _Material.SetFloat(ShaderIDs.s_FeatherWidth, _Input.FeatherWidth);
        }

        internal override void Draw(Lod lod, Component component, CommandBuffer buffer, RenderTargetIdentifier target, int slice)
        {
            var mesh = _Mesh;
            var material = _Material;

            if (mesh != null && material != null)
            {
#if UNITY_EDITOR
                // Weird things happen when hitting save if this is not set here. Hitting save will
                // still flicker the input, but without this it almost looks like it only renders
                // the largest wavelength.
                if (!Application.isPlaying) LodInput.SetBlendFromPreset(material, _Input.Blend);
#endif
                var pass = ShapeWaves.s_RenderPassOverride > -1 ? ShapeWaves.s_RenderPassOverride : 0;
                buffer.DrawMesh(mesh, component.transform.localToWorldMatrix, material, submeshIndex: 0, pass);
            }
        }

        void IReceiveSplineChangeMessages.OnSplineChange()
        {
            _IsDirty = true;
        }

#if UNITY_EDITOR
        [@OnChange]
        internal override void OnChange(string propertyPath, object previousValue)
        {
            if (_Input == null || !_Input.isActiveAndEnabled) return;

            switch (propertyPath)
            {
                case "../" + nameof(LodInput._Blend):
                    LodInput.SetBlendFromPreset(_Material, _Input.Blend);
                    break;
                default:
                    CreateOrUpdateSplineMesh();
                    break;
            }
        }

        internal override bool InferMode(Component component, ref LodInputMode mode)
        {
            if (component.TryGetComponent(out _Spline))
            {
                mode = LodInputMode.Spline;
                return true;
            }

            return false;
        }
#endif
    }

    /// <inheritdoc/>
    [MovedFrom(false, sourceNamespace: "WaveHarmonic.Crest.Spline", sourceAssembly: "WaveHarmonic.Crest.Spline")]
    [ForLodInput(typeof(FlowLodInput), LodInputMode.Spline)]
    public sealed partial class FlowSplineLodInputData : Internal.SplineLodInputData<FlowSplinePointData>
    {
        [Tooltip("Flow velocity (speed of flow in direction of spline). Can be negative to flip direction.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FlowVelocity = FlowSplinePointData.k_DefaultSpeed;

        private protected override Shader SplineShader => WaterResources.Instance.Shaders._FlowSpline;
        private protected override Vector4 DefaultCustomSplineData => new(_FlowVelocity, 0f, 0f, 0f);
    }

    /// <inheritdoc/>
    [MovedFrom(false, sourceNamespace: "WaveHarmonic.Crest.Spline", sourceAssembly: "WaveHarmonic.Crest.Spline")]
    [ForLodInput(typeof(FoamLodInput), LodInputMode.Spline)]
    public sealed partial class FoamSplineLodInputData : Internal.SplineLodInputData<FoamSplinePointData>
    {
        [Tooltip("Amount of foam emitted.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FoamAmount = FoamSplinePointData.k_DefaultAmount;

        private protected override Shader SplineShader => WaterResources.Instance.Shaders._FoamSpline;
        private protected override Vector4 DefaultCustomSplineData => new(_FoamAmount, 0f, 0f, 0f);
    }

    /// <inheritdoc/>
    [MovedFrom(false, sourceNamespace: "WaveHarmonic.Crest.Spline", sourceAssembly: "WaveHarmonic.Crest.Spline")]
    [ForLodInput(typeof(LevelLodInput), LodInputMode.Spline)]
    public sealed partial class LevelSplineLodInputData : Internal.SplineLodInputData<SplinePointData>
    {
        private protected override Shader SplineShader => WaterResources.Instance.Shaders._LevelGeometry;
        private protected override Vector4 DefaultCustomSplineData => Vector4.zero;
    }

    /// <inheritdoc/>
    [ForLodInput(typeof(ScatteringLodInput), LodInputMode.Spline)]
    public sealed partial class ScatteringSplineLodInputData : Internal.SplineLodInputData<ScatteringSplinePointData>
    {
        [Tooltip("The color of the scattering.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        Color _ScatteringColor = ScatteringSplinePointData.s_DefaultScattering;

        private protected override Shader SplineShader => WaterResources.Instance.Shaders._ColorSpline;
        private protected override Vector4 DefaultCustomSplineData => _ScatteringColor.MaybeLinear();
    }

    /// <inheritdoc/>
    [ForLodInput(typeof(AbsorptionLodInput), LodInputMode.Spline)]
    public sealed partial class AbsorptionSplineLodInputData : Internal.SplineLodInputData<AbsorptionSplinePointData>
    {
        [Tooltip("The color of water due to absorption.")]
        [@GenerateAPI(Setter.Custom)]
        [@DecoratedField, SerializeField]
        Color _AbsorptionColor = AbsorptionSplinePointData.s_DefaultAbsorption;

        Vector4 _Absorption = WaterRenderer.UpdateAbsorptionFromColor(AbsorptionSplinePointData.s_DefaultAbsorption);

        private protected override Shader SplineShader => WaterResources.Instance.Shaders._ColorSpline;
        private protected override Vector4 DefaultCustomSplineData => _Absorption;

        void SetAbsorptionColor(Color previous, Color current)
        {
            if (previous == current) return;
            _Absorption = WaterRenderer.UpdateAbsorptionFromColor(current);
        }

        internal override void OnEnable()
        {
            base.OnEnable();

            _Absorption = WaterRenderer.UpdateAbsorptionFromColor(_AbsorptionColor);
        }

#if UNITY_EDITOR
        internal override void OnChange(string path, object previous)
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

    /// <inheritdoc/>
    [MovedFrom(false, sourceNamespace: "WaveHarmonic.Crest.Spline", sourceAssembly: "WaveHarmonic.Crest.Spline")]
    [ForLodInput(typeof(ShapeWaves), LodInputMode.Spline)]
    public sealed partial class ShapeWavesSplineLodInputData : Internal.SplineLodInputData<WavesSplinePointData>
    {
        [@Label("Wave Multiplier")]
        [Tooltip("Weight multiplier to scale waves.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _Weight = WavesSplinePointData.k_DefaultWeight;

        [Tooltip("Feathers waves across the spline (ie across width). Reverse the spline to swap direction.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _FeatherWaveStart = 0.1f;

        private protected override Shader SplineShader => WaterResources.Instance.Shaders._WaveSpline;
        private protected override Vector4 DefaultCustomSplineData => new(_Weight, 0f, 0f, 0f);

        static partial class ShaderIDs
        {
            public static readonly int s_FeatherWaveStart = Shader.PropertyToID("_Crest_FeatherWaveStart");
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();
            if (_Material == null) return;
            _Material.SetFloat(ShaderIDs.s_FeatherWaveStart, _FeatherWaveStart);
        }
    }
}

namespace WaveHarmonic.Crest.Splines.Internal
{
    /// <inheritdoc/>
    public abstract partial class SplineLodInputData<T> : SplineLodInputData
        where T : SplinePointData
    {
        private protected override void CreateOrUpdateSplineMesh()
        {
            _IsDirty = false;

            if (_Material == null)
            {
                _Material = new(SplineShader);
            }

            LodInput.SetBlendFromPreset(_Material, _Input.Blend);

            if (_Spline == null)
            {
                Helpers.Destroy(_Mesh);
                _Mesh = null;
                return;
            }

            var radius = _OverrideSplineSettings ? _Radius : _Spline.Radius;
            var subdivs = _OverrideSplineSettings ? _Subdivisions : _Spline.Subdivisions;

            SplineMeshUtility.GenerateMeshFromSpline<T>
            (
                _Spline,
                _Spline.transform,
                subdivs,
                radius,
                DefaultCustomSplineData,
                ref _Mesh,
                ref _SplineBoundingPoints
            );

            RecalculateCulling();
        }
    }
}
