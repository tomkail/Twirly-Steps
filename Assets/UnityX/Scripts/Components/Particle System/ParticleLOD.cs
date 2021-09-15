using UnityEngine;

[System.Serializable]
public struct ParticleLOD {
    public float normalizedTransitionDistance;
    public float fadeTransitionWidth;
    public ParticleSystemAlphaController[] renderers;

    public ParticleLOD (float normalizedTransitionDistance, ParticleSystemAlphaController[] renderers) {
        this.normalizedTransitionDistance = normalizedTransitionDistance;
        this.fadeTransitionWidth = 0;
        this.renderers = renderers;
    }
}
