using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAudio : MonoSingleton<PickupAudio> {
    public float pitchResetTime = 0.5f;
    public float pitchTimer = 0;
    public float pitch;
    public AudioSource audioSource;
    public void Play() {
        pitch *= 1.122324159021407f;
        audioSource.pitch = pitch;
        audioSource.Play();
        pitchTimer = pitchResetTime;
    }

    void Update () {
        pitchTimer -= Time.deltaTime;
        if(pitchTimer < 0) {
            pitch = 1;
        }
    }
}