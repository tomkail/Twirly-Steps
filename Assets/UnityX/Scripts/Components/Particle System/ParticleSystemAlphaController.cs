using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleSystemAlphaController : MonoBehaviour {
    public new ParticleSystem particleSystem => GetComponent<ParticleSystem>();
    public string colorParamName = "_Color";
    [Range(0f,1f)]
    public float alphaMultiplier = 1f;
    public bool pauseWhenAlphaIsZero = true;

    public Material originalMaterial;
    [System.NonSerialized]
    Material material;
    [Disable]
    public float initialMaterialAlpha;
    
    public bool shouldBePaused => alphaMultiplier == 0;

    bool initialized;

    void OnEnable () {
        Initialize();
    }

    void Initialize () {
        var particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if(particleSystemRenderer == null) {
            enabled = false;
            return;
        }
        if(originalMaterial == null && particleSystemRenderer.sharedMaterial == null) {
            enabled = false;
            Debug.LogWarning("ParticleSystemRenderer has no material, so ParticleSystemAlphaController cannot be initialised.");
            return;
        }

        initialMaterialAlpha = originalMaterial.GetColor(colorParamName).a;
        material = new Material(originalMaterial);
        material.name = material.name+" (ParticleSystemAlphaController Clone)";
        particleSystemRenderer.material = material;
    
        initialized = true;
    }

    void OnDisable () {
        if(!initialized) return;

        if(material != null) {
            if(Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
            material = null;
        }
        var particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if(particleSystemRenderer != null) {
            particleSystemRenderer.material = originalMaterial;
        } else {
            Debug.LogWarning("ParticleSystemAlphaController lost reference to particleSystemRenderer and can't re-set its prefab material to "+originalMaterial+"!");
        }
        initialized = false;
    }

    void Update () {
        if(!initialized) {
            Initialize();
            if(!initialized) return;
        }
        var particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if(particleSystemRenderer != null) {
            particleSystemRenderer.material = material;
            var color = material.GetColor(colorParamName);
            color.a = alphaMultiplier * initialMaterialAlpha;
            material.SetColor(colorParamName, color);

            if(shouldBePaused && !particleSystem.isPaused) particleSystem.Pause();
            else if (!shouldBePaused && particleSystem.isPaused) particleSystem.Play();
        }
    }
}