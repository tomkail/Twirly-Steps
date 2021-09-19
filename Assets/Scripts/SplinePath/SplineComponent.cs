using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SplineSystem;

[System.Serializable]
public class SplineHandleSettings {
    public bool showWhenNotSelected;
} 
[ExecuteInEditMode]
public class SplineComponent : MonoBehaviour {
    public Spline spline;
    public SplineHandleSettings handleSettings;

    public void Reset() {
        spline.Validate();
        Refresh();
    }
    public void OnValidate() {
        Refresh();
    }

    protected void Update() {
        Refresh();
    }

    public void Refresh () {
        spline.RefreshCurveData();
    }

    public Vector3 GetPointAtArcLength (float arcLength) {
        return spline.GetPointAtArcLength(arcLength, transform.localToWorldMatrix);
    }
    
    
    public Quaternion GetRotationAtArcLength (float arcLength) {
        return spline.GetRotationAtArcLength(arcLength, transform.localToWorldMatrix);
    }


    public Vector3 GetDirectionAtArcLength (float arcLength) {
        return spline.GetDirectionAtArcLength(arcLength, transform.localToWorldMatrix);
    }

    public float EstimateArcLengthAlongCurve (Vector3 position, bool clampAtStart = false, bool clampAtEnd = false) {
        return spline.EstimateArcLengthAlongCurve(transform.InverseTransformPoint(position), clampAtStart, clampAtEnd);
    }
}