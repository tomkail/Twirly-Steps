using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peg : MonoBehaviour {
    public bool reverseDirection;
    void OnDrawGizmos () {
        GizmosX.BeginColor(Color.white.WithAlpha(0.5f));
        GizmosX.DrawWireCircle(transform.position, Quaternion.LookRotation(Vector3.up, Vector3.forward), 0.025f);
        GizmosX.EndColor();
        GizmosX.BeginColor(Color.white.WithAlpha(0.15f));
        GizmosX.DrawWireCircle(transform.position, Quaternion.LookRotation(Vector3.up, Vector3.forward), TwirlyCharacterController.Instance.settings.fixedGait);
        GizmosX.EndColor();
    }
}
