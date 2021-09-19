using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class SplineRenderer : MonoBehaviour {
    [SerializeField]
    SplineComponent spline;
    LineRenderer lineRenderer => GetComponent<LineRenderer>();
    
    void OnValidate() {
        Refresh();
    }
    void Update() {
        Refresh();
    }

    void Refresh () {
        if(spline == null) return;
        int i = 0;
        var verts = spline.spline.GetVertsWithPointsPerMeter(1);
        lineRenderer.positionCount = verts.Count();
        foreach(var vert in verts) {
            lineRenderer.SetPosition(i, spline.transform.TransformPoint(vert));
            i++;
        }
    }
}
