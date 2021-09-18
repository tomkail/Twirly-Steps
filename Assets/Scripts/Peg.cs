using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peg : MonoBehaviour {
    void OnDrawGizmos () {
        GizmosX.BeginColor(Color.white.WithAlpha(0.25f));
        Gizmos.DrawWireSphere(transform.position, TwirlyCharacterController.Instance.settings.fixedGait);
        GizmosX.EndColor();
    }
}
