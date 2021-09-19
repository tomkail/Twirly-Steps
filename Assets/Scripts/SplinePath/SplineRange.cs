using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineRange : MonoBehaviour {
    public SplineComponent spline;
    public float startArcLength;
    public float endArcLength;

    [Space]
    public bool setSpeedMultiplier;
    public float speedMultiplier = 1;
    
    [Space]
    public bool setAnimation;
    public string animationName;

    [Space]
    public bool usePeriodicGate;
    public float holdTime = 4;
    public float releaseTime = 2;
    public bool holding;
    public float timer;

    void OnValidate () {
        SetRange();
    }
    void Update () {
        SetRange();
        
        if(usePeriodicGate && Application.isPlaying) {
            timer += Time.deltaTime;
            if(holding && timer > holdTime) {
                holding = false;
                timer = 0;
            } else if(!holding && timer > releaseTime) {
                holding = true;
                timer = 0;
            }
        }
    }
    void SetRange () {
        if(spline == null) return;

        startArcLength = spline.EstimateArcLengthAlongCurve(transform.position+transform.TransformVector(Vector3.left * 0.5f));
        endArcLength = spline.EstimateArcLengthAlongCurve(transform.position+transform.TransformVector(Vector3.right * 0.5f));
        if(startArcLength > endArcLength) {
            var tmp = startArcLength;
            startArcLength = endArcLength;
            endArcLength = tmp;
        }
    }

    void OnDrawGizmos () {
        if(spline == null) return;

        var color = Gizmos.color;
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position+transform.TransformVector(Vector3.left * 0.5f), 0.1f);
        Gizmos.DrawSphere(transform.position+transform.TransformVector(Vector3.right * 0.5f), 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spline.GetPointAtArcLength(startArcLength), 0.25f);
        Gizmos.DrawSphere(spline.GetPointAtArcLength(endArcLength), 0.25f);
        SplineSystem.Spline.DrawSplineGizmos(spline.spline, spline.transform.localToWorldMatrix, startArcLength, endArcLength, 50);
        Gizmos.color = color;
    }
}
