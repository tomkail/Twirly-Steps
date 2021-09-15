using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class RenderTextureCamera : MonoBehaviour {
    new Camera camera;

    public enum UpdateMode {
        Manual,
        Update,
        LateUpdate
    }
    public UpdateMode updateMode = UpdateMode.LateUpdate;

    [SerializeField]
    RenderTexture rt;
    public RenderTexture renderTexture {
        get {
            return rt;
        }
    }
    
    public enum RenderTextureDepth {
        _0 = 0, 
        _16 = 16, 
        _24 = 24, 
        _32 = 32
    }
    public float resolutionScale = 1f;
    public RenderTextureDepth renderTextureDepth = RenderTextureDepth._24;
    public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
    public System.Action<RenderTexture> OnCreateRenderTexture;
    

    public Material material;
    public System.Action OnPreOnRenderImage;

    // Unity triggers a canvas render each time camera.Render is called.
    // This exists to block willRenderCanvases where a canvas update isn't required.
    public bool forceIgnoreCanvasUpdate = true;
    static System.Reflection.FieldInfo canvasHackField;
    static object canvasHackObject;

    void OnEnable () {
        camera = GetComponent<Camera>();
        camera.enabled = false;
        TryCreateRenderTexture();
        canvasHackField = typeof(Canvas).GetField("willRenderCanvases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        canvasHackObject = canvasHackField.GetValue(null);
    }
    
    void OnDisable () {
        ReleaseRenderTexture();
        camera.targetTexture = null;
    }

    void Update () {
        if(updateMode == UpdateMode.Update)
            Render();
    }
    void LateUpdate () {
        if(updateMode == UpdateMode.LateUpdate)
            Render();
    }

    public void Render () {
        if(forceIgnoreCanvasUpdate) canvasHackField.SetValue(null, null);

        camera.enabled = false;
        TryCreateRenderTexture();
        camera.targetTexture = rt;
        camera.Render();
        
        if(forceIgnoreCanvasUpdate) canvasHackField.SetValue(null, canvasHackObject);
    }

    // private void OnPreRender() {}
    // private void OnPostRender() {}

    void TryCreateRenderTexture () {
        var targetSize = new Vector2Int(Mathf.CeilToInt(Screen.width*resolutionScale), Mathf.CeilToInt(Screen.height*resolutionScale));
        if(rt != null && (rt.width != targetSize.x || rt.height != targetSize.y || rt.depth != (int)renderTextureDepth || rt.format != renderTextureFormat)) {
            ReleaseRenderTexture();
        }
        if(rt == null && targetSize.x > 0 && targetSize.y > 0) {
            rt = new RenderTexture (targetSize.x, targetSize.y, (int)renderTextureDepth, renderTextureFormat);
            rt.filterMode = FilterMode.Bilinear;
            rt.hideFlags = HideFlags.HideAndDontSave;
            if(OnCreateRenderTexture != null) OnCreateRenderTexture(rt);
        }
    }

    void ReleaseRenderTexture () {
        if(rt == null) return;
        rt.Release();
        rt = null;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if(OnPreOnRenderImage != null) OnPreOnRenderImage();
        if (material != null) {
            Graphics.Blit(src, dest, material);
        } else {
            Graphics.Blit(src, dest);
        }
    }
}