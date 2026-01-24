// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest.Splines.Editor
{
    [CustomEditor(typeof(Spline))]
    sealed partial class SplineEditor : Inspector
    {
        enum WavesPreset
        {
            None = -1,
            River,
            Shoreline,
        }

        static readonly GUIContent s_FFTWaves = new("FFT Waves");
        static readonly GUIContent s_GerstnerWaves = new("Gerstner Waves");
        static readonly string[] s_WaveLabels = new string[] { "River", "Shoreline" };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targetSpline = target as Spline;

            EditorGUILayout.Space();

#if d_UnitySplines
            if (!targetSpline.HasSource && targetSpline.TryGetComponent<UnityEngine.Splines.SplineContainer>(out var spline))
            {
                targetSpline.GetComponentsInChildren(s_SplinePoints);
                if (s_SplinePoints.Count == 0 && GUILayout.Button("Use Unity Spline"))
                {
                    Undo.RecordObject(target, "Connect Spline Source");
                    targetSpline._Source = spline;
                    targetSpline.InitializeFromContainer();
                }
            }
#endif // d_UnitySplines

            if (!targetSpline.HasSource)
            {
                if (GUILayout.Button("Add point (extend)"))
                {
                    ExtendSpline(targetSpline);
                }

                GUILayout.BeginHorizontal();
                var pointCount = targetSpline.transform.childCount;
                GUI.enabled = pointCount > 0;
                if (GUILayout.Button("Select first point"))
                {
                    Selection.activeGameObject = targetSpline.transform.GetChild(0).gameObject;
                }
                if (GUILayout.Button("Select last point"))
                {
                    Selection.activeGameObject = targetSpline.transform.GetChild(pointCount - 1).gameObject;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Reverse"))
                {
                    for (var i = 1; i < targetSpline.transform.childCount; i++)
                    {
                        targetSpline.transform.GetChild(i).SetSiblingIndex(0);
                    }
                }
            }

            // Helpers to quickly attach water inputs
            EditorGUILayout.Space();
            GUILayout.Label("Add Feature", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            FeatureButton<LevelLodInput>("Level", targetSpline.gameObject);
            FeatureButton<FlowLodInput>("Flow", targetSpline.gameObject);
            FeatureButton<FoamLodInput>("Foam", targetSpline.gameObject);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FeatureButton<AbsorptionLodInput>("Absorption", targetSpline.gameObject);
            FeatureButton<ScatteringLodInput>("Scattering", targetSpline.gameObject);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FeatureButton<ShapeFFT>(s_FFTWaves, s_WaveLabels, targetSpline.gameObject);
            FeatureButton<ShapeGerstner>(s_GerstnerWaves, s_WaveLabels, targetSpline.gameObject);
            GUILayout.EndHorizontal();
        }

        internal static void ExtendSpline(Spline spline)
        {
            var newPoint = SplinePointEditor.AddSplinePointAfter(spline.transform);

            Undo.RegisterCreatedObjectUndo(newPoint, "Add Crest Spline Point");
        }

        internal static void FeatureButton<T>(string label, GameObject go) where T : Component
        {
            using (new EditorGUI.DisabledGroupScope(go.TryGetComponent<T>(out _)))
            {
                if (GUILayout.Button(label))
                {
                    Undo.AddComponent<T>(go);
                }
            }
        }

        static void FeatureButton<T>(GUIContent label, string[] labels, GameObject go) where T : ShapeWaves
        {
            using (new EditorGUI.DisabledGroupScope(go.TryGetComponent<T>(out _)))
            {
                if (EditorHelpers.Button(label, out var choice, labels))
                {
                    var preset = (WavesPreset)choice;
                    var waves = Undo.AddComponent<T>(go);

                    if (preset != WavesPreset.None)
                    {
                        if (waves is ShapeFFT fft)
                        {
                            fft.OverrideGlobalWindTurbulence = true;
                        }
                    }

                    switch (preset)
                    {
                        case WavesPreset.River:
                            waves.OverrideGlobalWindSpeed = true;
                            waves.OverrideGlobalWindDirection = true;
                            waves.WaveDirectionHeadingAngle = 90f;
                            waves.Blend = LodInputBlend.Alpha;
                            waves.Queue = 1;
                            break;
                        case WavesPreset.Shoreline:
                            waves.OverrideGlobalWindSpeed = true;
                            waves.WindSpeed = 150f;
                            waves.OverrideGlobalWindDirection = true;
                            if (waves is ShapeGerstner gerstner)
                            {
                                gerstner.Swell = true;
                                gerstner.ReverseWaveWeight = 0f;
                            }
                            break;
                    }
                }
            }
        }
    }

#if d_UnitySplines
    sealed partial class SplineEditor
    {
        static readonly List<SplinePoint> s_SplinePoints = new();

        static readonly GUIContent s_UnitySplineIcon = new();
        static readonly GUIContent s_UnitySplineText = new("Spline is being controlled by a <i>Unity Spline</i>. Click the Disconnect button to disconnect from it.");
        static readonly GUIContent s_DisconnectButton = new("Disconnect", "Disconnect from the Unity Spline. Apart from undo, this is irreversible.");

        static readonly System.Reflection.MethodInfo s_GetIcon = typeof(UnityEditor.Splines.EditorSplineUtility)
            .Assembly.GetType("UnityEditor.Splines.PathIcons")
            .GetMethod("GetIcon", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        static readonly object[] s_SplineEditModeInfo = new object[] { "SplineEditMode-Info" };

        protected override void RenderBeforeInspectorGUI()
        {
            base.RenderBeforeInspectorGUI();

            var target = this.target as Spline;

            if (!target.HasSource)
            {
                return;
            }

            s_UnitySplineIcon.image = (Texture)s_GetIcon.Invoke(null, s_SplineEditModeInfo);

            var choice = EditorHelpers.HelpBox
            (
                s_UnitySplineText,
                s_UnitySplineIcon,
                s_DisconnectButton
            );

            if (choice != null)
            {
                Undo.RecordObject(target, "Disconnect Spline Source");
                target._Source = null;
            }
        }
    }
#endif // d_UnitySplines
}
