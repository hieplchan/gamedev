// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEditor;
using UnityEngine.Splines;
using WaveHarmonic.Crest.Editor;

using static WaveHarmonic.Crest.Editor.ValidatedHelper;
using MessageType = WaveHarmonic.Crest.Editor.ValidatedHelper.MessageType;

namespace WaveHarmonic.Crest.Splines.Editor
{
    static class Validators
    {
        [Validator(typeof(LodInput))]
        static bool Validate(LodInput target, ShowMessage messenger)
        {
            if (target.Data is not SplineLodInputData data) return true;

            var isValid = true;

            if (data._Spline != null)
            {
                if (!ExecuteValidators(data._Spline, Suppressed))
                {
                    messenger
                    (
                        "A <i>Spline</i> component is attached but it has validation errors.",
                        "Check this component in the Inspector for issues.",
                        MessageType.Error, target
                    );

                    isValid = false;
                }
            }
            else
            {
                var found = target.TryGetComponent<Spline>(out var spline);

                messenger
                (
                    "A <i>Crest Spline</i> component is required to drive this data and none is attached to this water input GameObject.",
                    $"{(found ? "Set the" : "Add a")} <i>Crest Spline</i> component.",
                    MessageType.Error, target,
                    (_, y) =>
                    {
                        if (!found) spline = Undo.AddComponent<Spline>(target.gameObject);
                        y.objectReferenceValue = spline;
                    },
                    $"{nameof(LodInput._Data)}.{nameof(SplineLodInputData._Spline)}"
                );

                isValid = false;
            }

            return isValid;
        }

        [Validator(typeof(Spline))]
        static bool Validate(Spline target, ShowMessage messenger)
        {
            var isValid = true;

            var points = target.GetComponentsInChildren<SplinePoint>();

#if d_UnitySplines
            if (target.HasSource)
            {
                var source = target._Source;

                // Skip validation as there may be temporary hidden objects in the hierarchy.
                if (source.Splines.Count > 1)
                {
                    messenger
                    (
                        $"The SplineContainer has multiple splines, but currently only a single spline is supported. " +
                        "Ignoring other splines.",
                        "",
                        MessageType.Warning, target
                    );
                }

                var modification = SplineModification.Default;

                if (points.Length > source.Spline.Count)
                {
                    modification = SplineModification.KnotRemoved;
                }
                else if (points.Length < source.Spline.Count)
                {
                    modification = SplineModification.KnotInserted;
                }
                else
                {
                    for (var i = 0; i < points.Length; i++)
                    {
                        // TODO: Handle KnotReordered
                        if (points[i].transform.position != source.transform.TransformPoint(source.Spline[i].Position))
                        {
                            modification = SplineModification.KnotModified;

                            foreach (var knot in source.Spline)
                            {
                                if (points[i].transform.position == source.transform.TransformPoint(source.Spline[i].Position))
                                {
                                    modification = SplineModification.KnotReordered;
                                }
                            }
                        }
                    }
                }

                if (modification != SplineModification.Default)
                {
                    messenger
                    (
                        $"Spline points do not match the source knots ({modification}).",
                        "Attempt to synchronize (this may cause loss of spline point overrides)." +
                        "If this does nothing, you will need to start again.",
                        MessageType.Warning,
                        target,
                        (_, _) => target.OnSplineChanged(source.Spline, 0, modification)
                    );
                }
            }
#endif

            for (var i = 0; i < target.transform.childCount; i++)
            {
                if (!target.transform.GetChild(i).TryGetComponent<SplinePoint>(out _))
                {
                    messenger
                    (
                        $"All child GameObjects under <i>Spline</i> must have <i>SplinePoint</i> component added. Object <i>{target.transform.GetChild(i).gameObject.name}</i> does not have one.",
                        $"Add a <i>SplinePoint</i> component to object {target.transform.GetChild(i).gameObject.name}, or move this object out in the hierarchy.",
                        MessageType.Error, target
                    );

                    isValid = false;

                    // One error is enough probably - don't fill the Inspector with tons of errors
                    break;
                }
            }

            if (points.Length < 2)
            {
                messenger
                (
                    "Spline must have at least 2 spline points.",
                    "Click the <i>Add Point</i> button in the Inspector, or add a child GameObject and attach <i>SplinePoint</i> component to it.",
                    MessageType.Error, target,
                    FixAddSplinePoints
                );

                isValid = false;
            }
            else if (target._Closed && points.Length < 3)
            {
                messenger
                (
                    "Closed splines must have at least 3 spline points. See the <i>Closed</i> parameter and tooltip.",
                    "Add a point by clicking the <i>Add Point</i> button in the Inspector.",
                    MessageType.Error, target,
                    FixAddSplinePoints
                );

                isValid = false;
            }

            return isValid;
        }

        static void FixAddSplinePoints(SerializedObject splineComponent, SerializedProperty _)
        {
            var spline = splineComponent.targetObject as Spline;
            var requiredPoints = spline._Closed ? 3 : 2;
            var needToAdd = requiredPoints - spline.GetComponentsInChildren<SplinePoint>().Length;

            for (var i = 0; i < needToAdd; i++)
            {
                SplineEditor.ExtendSpline(spline);
            }
        }
    }
}
