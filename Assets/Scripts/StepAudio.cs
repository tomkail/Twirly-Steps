using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepAudio : MonoSingleton<StepAudio> {
    public float pitchResetTime = 0.5f;
    public float pitchTimer = 0;
    public float pitch;
    public AudioSource audioSource;
    public void Play() {
        audioSource.pitch = GetPitch(1, TwirlyCharacterController.Instance.stepCombo);
        audioSource.Play();
        pitchTimer = pitchResetTime;
    }

    float GetPitch (float initialPitch, float combo) {
        float pitch = initialPitch;
        pitch = initialPitch + Mathf.Pow(combo, 1.122324159021407f);
        // for(int i = 0; i < combo; i++) {
        //     pitch *= 1.122324159021407f;
        // }
        return pitch;
    }
}