using UnityEngine;

public enum ParticleSystemLODFadeMode
{
    None = 0,
    CrossFade = 1,
    SpeedTree = 2
}

[ExecuteInEditMode]
public class ParticleSystemLODGroup : MonoBehaviour {
    public Camera referenceCamera;
    public float m_FadeTransitionWidth = 10;
    public float m_CullingDistance = 100;
    public ParticleLOD[] m_LODs = new ParticleLOD[] {
        new ParticleLOD(0.1f, null),
        new ParticleLOD(0.25f, null),
        new ParticleLOD(0.7f, null),
    };

    #if UNITY_EDITOR
    void OnValidate () {
        if(m_LODs == null || m_LODs.Length == 0) {
            m_CullingDistance = 100;
            m_LODs = new ParticleLOD[] {
                new ParticleLOD(0.1f, null),
                new ParticleLOD(0.25f, null),
                new ParticleLOD(0.7f, null),
            };
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
    #endif
    
    void OnEnable() {
        Camera.onPreRender += PreRender;
    }
    void OnDisable() {
        Camera.onPreRender -= PreRender;
    }

    public void PreRender(Camera cam) {
        if(Application.isPlaying && referenceCamera != null && cam != referenceCamera) return;

        var distance = Vector3.Distance(cam.transform.position, transform.position);
        for (int i = 0; i < m_LODs.Length; i++) {
            ParticleLOD lod = m_LODs[i];
            // var alpha = DoubleInverseLerp(
            //     i == 0 ? 0 : (m_LODs[i-1].normalizedTransitionDistance * m_CullingDistance) - m_FadeTransitionWidth, 
            //     i == 0 ? 0 : m_LODs[i-1].normalizedTransitionDistance * m_CullingDistance, 
            //     lod.normalizedTransitionDistance * m_CullingDistance, 
            //     lod.normalizedTransitionDistance * m_CullingDistance + m_FadeTransitionWidth, 
            //     distance
            // );

            var a = i == 0 ? 0 : (m_LODs[i-1].normalizedTransitionDistance * m_CullingDistance) - m_FadeTransitionWidth * 0.5f;
            var b = i == 0 ? 0 : m_LODs[i-1].normalizedTransitionDistance * m_CullingDistance + m_FadeTransitionWidth * 0.5f;
            var c = lod.normalizedTransitionDistance * m_CullingDistance - m_FadeTransitionWidth * 0.5f;
            var d = lod.normalizedTransitionDistance * m_CullingDistance + m_FadeTransitionWidth * 0.5f;
            b = Mathf.Min(b,c);
            var alpha = DoubleInverseLerp(a,b,c,d,distance);
            foreach(var renderer in lod.renderers) {
                renderer.alphaMultiplier = alpha;
            }
        }
    }

    static float DoubleInverseLerp (float a, float b, float c, float d, float value) {
        Debug.Assert(a <= b);
        Debug.Assert(b <= c);
        Debug.Assert(c <= d);
        if(value >= d) return 0;
        else if(value >= c) return Mathf.InverseLerp(d, c, value);
        else if(value >= b) return 1;
        if(value >= a) return Mathf.InverseLerp(a, b, value);
        return 0;
    }
}