// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest.Splines
{
    /// <summary>
    /// Base class for components which hold point-level spline data.
    /// </summary>
    [@HelpURL("Packages/Splines/Manual.html")]
    public abstract class SplinePointData : ManagedBehaviour<WaterRenderer>
    {
        internal abstract Vector4 GetData(Vector4 data);

#if UNITY_EDITOR
        [@OnChange(skipIfInactive: false)]
        private protected virtual void OnChange(string path, object previous)
        {
            if (!isActiveAndEnabled) return;

            if (transform.parent == null)
            {
                return;
            }

            Spline.NotifyReceivers(transform.parent);
        }
#endif
    }
}
