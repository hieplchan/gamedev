// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using MonoBehaviour = WaveHarmonic.Crest.Internal.EditorBehaviour;
#endif

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Spline point, intended to be child of Spline object
    /// </summary>
    [@ExecuteDuringEditMode]
    [AddComponentMenu(Constants.k_MenuPrefixSpline + "Spline Point")]
    public sealed partial class SplinePoint : MonoBehaviour
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("Multiplier for spline radius.")]
        [@GenerateAPI]
        [@DecoratedField, SerializeField]
        internal float _RadiusMultiplier = 1f;

        internal Vector3 _LocalPosition;

#if UNITY_EDITOR
        private protected override void Start()
        {
            base.Start();

            if (transform.parent.TryGetComponent<Spline>(out var spline) && spline.HasSource)
            {
                // We rely on hasChanged event to update the spline.
                _LocalPosition = transform.localPosition;
            }
        }

        void Update()
        {
            if (!transform.hasChanged)
            {
                return;
            }

            if (_LocalPosition == transform.localPosition)
            {
                return;
            }

            if (transform.parent.TryGetComponent<Spline>(out var spline) && spline.HasSource)
            {
                // Do not allow transform changed with source.
                transform.localPosition = _LocalPosition;
                transform.hasChanged = false;
                return;
            }

            // Do not set this during initialization as it will prevent initial update (like
            // when adding a new spline point).
            _LocalPosition = transform.localPosition;

            NotifyOnChange();
        }

        void LateUpdate()
        {
            transform.hasChanged = false;
        }

        void NotifyOnChange()
        {
            if (transform.parent == null)
            {
                return;
            }

            Spline.NotifyReceivers(transform.parent);
        }

        void OnDisable()
        {
            NotifyOnChange();
        }

        [@OnChange]
        void OnChange(string path, object previous)
        {
            if (!isActiveAndEnabled) return;
            NotifyOnChange();
        }
#endif
    }

    interface IReceiveSplineChangeMessages
    {
        void OnSplineChange();
    }
}
