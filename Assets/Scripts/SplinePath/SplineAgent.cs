using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineAgent : MonoBehaviour {
    public SplineComponent splineComponent;
    public float arcLength;
    public float speed = 1;
    void Update() {
        if(splineComponent == null) return;
        if(Application.isPlaying) {
            arcLength += Time.deltaTime * speed;
        }
        transform.position = splineComponent.spline.GetPointAtArcLength(arcLength);
        transform.rotation = splineComponent.spline.GetRotationAtArcLength(arcLength);
    }
}
