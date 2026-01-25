// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Where generated ribbon should lie relative to the <see cref="Spline"/>.
    /// </summary>
    [@GenerateDoc]
    public enum SplineOffset
    {
        /// <inheritdoc cref="Generated.SplineOffset.Left"/>
        [Tooltip("Left to the spline.")]
        Left,

        /// <inheritdoc cref="Generated.SplineOffset.Center"/>
        [Tooltip("Centered around the spline.")]
        Center,

        /// <inheritdoc cref="Generated.SplineOffset.Right"/>
        [Tooltip("Right to the spline.")]
        Right
    }

    /// <summary>
    /// Simple spline object. Spline points are child GameObjects.
    /// </summary>
    [@ExecuteDuringEditMode]
    [AddComponentMenu(Constants.k_MenuPrefixSpline + "Spline")]
    [@HelpURL("Packages/Splines/Manual.html")]
    public sealed partial class Spline : ManagedBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

#if d_UnitySplines
        [@Predicated(typeof(Spline), nameof(HasSplinePoints))]
        [@DecoratedField, SerializeField]
        internal SplineContainer _Source;
#endif

        [Tooltip("Where generated ribbon should lie relative to spline.\n\nIf set to Center, ribbon is centered around spline.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal SplineOffset _Offset = SplineOffset.Center;

        [Tooltip("Connect start and end point to close spline into a loop.\n\nRequires at least 3 spline points.")]
#if d_UnitySplines
        [@Predicated(nameof(_Source), inverted: true)]
#endif
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal bool _Closed = false;

        [Tooltip("The radius of the spline.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        float _Radius = 10f;

        [Tooltip("Increasing subdivision increases the geometry density.\n\nMostly useful for water level changes. High values can reduce staircasing effect.")]
        [@Delayed]
        [@GenerateAPI]
        [SerializeField]
        int _Subdivisions = 1;

        [@Space(10)]

        [@DecoratedField, SerializeField]
        internal DebugFields _Debug = new();

        [System.Serializable]
        internal class DebugFields
        {
            [Tooltip("Forces the spline to update every frame.")]
            [@DecoratedField, SerializeField]
            internal bool _UpdateEveryFrame;
        }

        static readonly List<LodInput> s_Inputs = new();
        static readonly List<IReceiveSplineChangeMessages> s_Receivers = new();

        internal bool HasSource =>
#if d_UnitySplines
            _Source != null;
#else
            false;
#endif

        bool HasSplinePoints()
        {
            return GetComponentsInChildren<SplinePoint>().Length > 0;
        }

        internal static void NotifyReceivers(Transform sibling)
        {
            sibling.GetComponents(s_Receivers);
            foreach (var receiver in s_Receivers)
            {
                receiver.OnSplineChange();
            }

            sibling.GetComponents(s_Inputs);
            foreach (var receiver in s_Inputs)
            {
                (receiver.Data as IReceiveSplineChangeMessages)?.OnSplineChange();
            }
        }

        /// <summary>
        /// Applies any changes to the spline meshes.
        /// </summary>
        public void UpdateSpline()
        {
            NotifyReceivers(transform);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void Update()
        {
            if (_Debug._UpdateEveryFrame)
            {
                UpdateSpline();
            }
        }
#endif

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string path, object previous)
        {
            if (!isActiveAndEnabled) return;
            UpdateSpline();
        }
#endif
    }

#if d_UnitySplines
    sealed partial class Spline
    {
        private protected override void OnEnable()
        {
            base.OnEnable();
            UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
            UnityEngine.Splines.Spline.Changed += OnSplineChanged;
        }

        private protected override void OnDisable()
        {
            base.OnDisable();
            UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
        }

        private protected override void Initialize()
        {
            base.Initialize();

            if (GetComponentsInChildren<SplinePoint>().Length == 0 && (_Source != null || TryGetComponent(out _Source)))
            {
                InitializeFromContainer();
            }
        }

        internal void InitializeFromContainer()
        {
            foreach (var knot in _Source.Spline)
            {
                var go = new GameObject();
                go.name = "Spline Point";
                go.transform.parent = transform;
                go.transform.position = _Source.transform.TransformPoint(knot.Position);
                go.AddComponent<SplinePoint>();
                Helpers.Undo.RegisterCreatedObjectUndo(go, "Add Spline");
            }
        }

        internal void OnSplineChanged(UnityEngine.Splines.Spline spline, int index, SplineModification modification)
        {
            var container = _Source;

            if (container == null || container.Spline != spline)
            {
                return;
            }

            // We use the knot/point world position to identify and link, as there appears to
            // be no unique way to identify knots when needed. This includes storing and
            // comparing the knot struct (auto tangents being the hurdle).

            var points = GetComponentsInChildren<SplinePoint>();

            switch (modification)
            {
                case SplineModification.ClosedModified:
                {
                    Helpers.Undo.RecordObject(this, "Update Spline Closed");
                    Closed = spline.Closed;
                    break;
                }
                // NOTE: It could be the curve type changing. No way to know it seems.
                case SplineModification.KnotModified:
                {
                    for (var iKnot = 0; iKnot < spline.Count; iKnot++)
                    {
                        var knot = spline[iKnot];
                        var point = points[iKnot];
                        Helpers.Undo.RecordObject(point.transform, "Update Spline Point");
                        Helpers.Undo.RecordObject(point, "Update Spline Point");
                        point.transform.position = container.transform.TransformPoint(knot.Position);
                        point._LocalPosition = point.transform.localPosition;
                    }

                    break;
                }
                case SplineModification.KnotInserted:
                {
                    for (var iKnot = 0; iKnot < spline.Count; iKnot++)
                    {
                        var knot = spline[iKnot];

                        if (iKnot < points.Length && points[iKnot].transform.position == container.transform.TransformPoint(knot.Position))
                        {
                            continue;
                        }

                        var go = new GameObject();
                        go.name = "Spline Point";
                        go.transform.parent = transform;
                        go.transform.position = container.transform.TransformPoint(knot.Position);
                        go.transform.SetSiblingIndex(iKnot);
                        go.AddComponent<SplinePoint>();
                        Helpers.Undo.RegisterCreatedObjectUndo(go, "Add Spline Point");
                        break;
                    }

                    break;
                }
                case SplineModification.KnotRemoved:
                {
                    var iKnot = 0;
                    for (var iPoint = 0; iPoint < points.Length; iPoint++)
                    {
                        var point = points[iPoint];

                        if (iKnot >= spline.Count)
                        {
                            Helpers.Destroy(point.gameObject, undo: true);
                            continue;
                        }

                        var knot = spline[iKnot];

                        if (point.transform.position != container.transform.TransformPoint(knot.Position))
                        {
                            Helpers.Destroy(point.gameObject, undo: true);
                        }
                        else
                        {
                            iKnot++;
                        }
                    }

                    break;
                }
                case SplineModification.KnotReordered:
                {
                    for (var iKnot = 0; iKnot < spline.Count; iKnot++)
                    {
                        var knot = spline[iKnot];

                        for (var iPoint = 0; iPoint < points.Length; iPoint++)
                        {
                            var point = points[iPoint];

                            if (point.transform.position == container.transform.TransformPoint(knot.Position))
                            {
                                Helpers.Undo.SetSiblingIndex(point.transform, iKnot, "Reorder Spline Point");
                                point.transform.SetSiblingIndex(iKnot);
                                // Assume knots are reordered once per event.
                                break;
                            }
                        }
                    }

                    break;
                }
            }

            UpdateSpline();
        }
    }
#endif // d_UnitySplines
}
