// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine;
using WaveHarmonic.Crest.Editor;

namespace WaveHarmonic.Crest.Splines.Editor
{
    [CustomEditor(typeof(SplinePoint))]
    sealed class SplinePointEditor : Inspector
    {
        protected override void RenderInspectorGUI()
        {
            base.RenderInspectorGUI();

            var thisSP = target as SplinePoint;
            var thisIdx = thisSP.transform.GetSiblingIndex();

            var parent = thisSP.transform.parent;
            if (parent == null || !parent.TryGetComponent<Spline>(out var spline))
            {
                EditorGUILayout.HelpBox("Spline component must be present on parent of this GameObject.", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

            GUILayout.Label("Selection", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (thisIdx == 0) GUI.enabled = false;
            if (GUILayout.Button("Select previous"))
            {
                Selection.activeObject = parent.GetChild(thisIdx - 1).gameObject;
            }
            GUI.enabled = true;
            if (thisIdx == parent.childCount - 1) GUI.enabled = false;
            if (GUILayout.Button("Select next"))
            {
                Selection.activeObject = parent.GetChild(thisIdx + 1).gameObject;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Select spline"))
            {
                Selection.activeObject = parent.gameObject;
            }

#if d_UnitySplines
            if (!spline.HasSource)
#endif
            {

                EditorGUILayout.Space();

                GUILayout.Label("Actions", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                string label;

                label = thisIdx == 0 ? "Add before (extend)" : "Add before";
                if (GUILayout.Button(label))
                {
                    var newPoint = AddSplinePointBefore(parent, thisIdx);

                    Undo.RegisterCreatedObjectUndo(newPoint, "Add Crest Spline Point");

                    Selection.activeObject = newPoint;
                }

                label = (thisIdx == parent.childCount - 1 || parent.childCount == 0) ? "Add after (extend)" : "Add after";
                if (GUILayout.Button(label))
                {
                    var newPoint = AddSplinePointAfter(parent, thisIdx);

                    Undo.RegisterCreatedObjectUndo(newPoint, "Add Crest Spline Point");

                    Selection.activeObject = newPoint;
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Delete"))
                {
                    if (thisIdx > 0)
                    {
                        Selection.activeObject = parent.GetChild(thisIdx - 1);
                    }
                    else
                    {
                        // If there is more than one child, select the first
                        if (parent.childCount > 1)
                        {
                            Selection.activeObject = parent.GetChild(1);
                        }
                        else
                        {
                            // No children - select the parent
                            Selection.activeObject = parent;
                        }
                    }
                    Undo.DestroyObjectImmediate(thisSP.gameObject);
                }
            }

            // Helpers to quickly attach point data.
            EditorGUILayout.Space();
            GUILayout.Label("Feature Override", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            FeatureButton<FlowSplinePointData, FlowLodInput>("Flow", thisSP.gameObject);
            FeatureButton<WavesSplinePointData, ShapeWaves>("Waves", thisSP.gameObject);
            FeatureButton<FoamSplinePointData, FoamLodInput>("Foam", thisSP.gameObject);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FeatureButton<AbsorptionSplinePointData, AbsorptionLodInput>("Absorption", thisSP.gameObject);
            FeatureButton<ScatteringSplinePointData, ScatteringLodInput>("Scattering", thisSP.gameObject);
            GUILayout.EndHorizontal();
        }

        static void FeatureButton<DataType, InputType>(string label, GameObject gameObject)
            where DataType : SplinePointData
            where InputType : LodInput
        {
            using (new EditorGUI.DisabledGroupScope(!gameObject.transform.parent.TryGetComponent<InputType>(out _)))
            {
                SplineEditor.FeatureButton<DataType>(label, gameObject);
            }
        }

        static GameObject CreateNewSP(Transform spline)
        {
            var newPoint = new GameObject();
            newPoint.name = "SplinePoint";
            newPoint.AddComponent<SplinePoint>();
            newPoint.transform.parent = spline;

            return newPoint;
        }

        public static GameObject AddSplinePointBefore(Transform parent, int beforeIdx = 0)
        {
            var newPoint = CreateNewSP(parent);

            // Put in front of child at beforeIdx
            newPoint.transform.SetSiblingIndex(beforeIdx);
            // Inserting has moved the before point forwards, update its index to simplify the below
            beforeIdx++;

            if (parent.childCount == 1)
            {
                // New point is sole point, place at center
                newPoint.transform.localPosition = Vector3.zero;
            }
            else if (parent.childCount == 2)
            {
                // New point has one sibling, place nearby it
                newPoint.transform.position = parent.GetChild(beforeIdx).position - 10f * Vector3.forward;
            }
            else if (beforeIdx > 1)
            {
                // New point being inserted between two existing points, bisect them
                var beforeNewPoint = parent.GetChild(beforeIdx);
                var afterNewPoint = parent.GetChild(beforeIdx - 2);
                newPoint.transform.position = Vector3.Lerp(beforeNewPoint.position, afterNewPoint.position, 0.5f);
            }
            else
            {
                // New point being inserted before first point, and spline has multiple points, extrapolate backwards
                var newPos = 2f * parent.GetChild(1).position - parent.GetChild(2).position;
                newPoint.transform.position = newPos;
            }

            return newPoint;
        }

        public static GameObject AddSplinePointAfter(Transform parent, int afterIdx = -1)
        {
            // If no index specified, assume adding after last point
            if (afterIdx == -1) afterIdx = parent.childCount - 1;

            var newPoint = CreateNewSP(parent);

            var newIdx = afterIdx + 1;
            newPoint.transform.SetSiblingIndex(newIdx);

            if (parent.childCount == 1)
            {
                // New point is sole point, place at center
                newPoint.transform.localPosition = Vector3.zero;
            }
            else if (parent.childCount == 2)
            {
                // New point has one sibling, place nearby it
                newPoint.transform.position = parent.GetChild(afterIdx).position + 10f * Vector3.forward;
            }
            else if (newIdx < parent.childCount - 1)
            {
                // New point being inserted between two existing points, bisect them
                var beforeNewPoint = parent.GetChild(afterIdx);
                var afterNewPoint = parent.GetChild(afterIdx + 2);
                newPoint.transform.position = Vector3.Lerp(beforeNewPoint.position, afterNewPoint.position, 0.5f);
            }
            else
            {
                // New point being added after last point, and spline has multiple points, extrapolate forwards
                newPoint.transform.position = 2f * parent.GetChild(newIdx - 1).position - parent.GetChild(newIdx - 2).position;
            }

            return newPoint;
        }
    }
}
