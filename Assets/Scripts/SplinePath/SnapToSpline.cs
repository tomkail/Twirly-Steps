using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToSpline : MonoBehaviour {
    public SplineComponent spline;
    public bool position = true;
    public bool rotation = true;

    [Space]
    public float arcLength;

    void OnValidate () {
        Snap();
    }
    void OnEnable () {
        Snap();
    }
    void Update () {
        Snap();
    }

    void Snap () {
        if(spline == null || (!position && !rotation)) return;

        arcLength = spline.EstimateArcLengthAlongCurve(transform.position);
        if(position) {
            var positionOnSpline = spline.GetPointAtArcLength(arcLength);
            if(Vector3.Distance(transform.position, positionOnSpline) > 0.0001f) {
                transform.position = positionOnSpline;
            }
        }

        if(rotation) {
            var rotationOnSpline = spline.GetRotationAtArcLength(arcLength);
            if(Quaternion.Dot(transform.rotation, rotationOnSpline) > 0.001f) {
                transform.rotation = rotationOnSpline;
            }
        }
    }
}
