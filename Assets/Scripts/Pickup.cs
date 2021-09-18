using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Leg>()) {
            Collect();
        }
    }

    void Collect () {
        PickupAudio.Instance.Play();
        Destroy(gameObject);
    }
}
