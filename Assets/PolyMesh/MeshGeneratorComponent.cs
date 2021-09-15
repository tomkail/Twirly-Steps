using UnityEngine;
using UnityX.MeshBuilder;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter))]
public abstract class MeshGeneratorComponent : MonoBehaviour {
	public MeshFilter meshFilter {
		get {
			return GetComponent<MeshFilter>();
		}
	}
	public MeshCollider meshCollider {
		get {
			return GetComponent<MeshCollider>();
		}
	}
	public bool front = true;
	public bool back;

    public MeshBakeParams bakeParams = new MeshBakeParams(true, true);

	void OnEnable () {
		#if UNITY_EDITOR
		if(UnityEditor.BuildPipeline.isBuildingPlayer) return;
		#endif
		// if(Application.isPlaying)
		// 	Refresh();
	}
	void OnDisable () {
		Clear ();
	}

	void Clear () {
		ObjectX.DestroyAutomatic(meshFilter.sharedMesh);
		if(meshCollider != null) ObjectX.DestroyAutomatic(meshCollider.sharedMesh);
	}

	[ContextMenu("Refresh")]
	public void Refresh () {
		Clear();
		meshFilter.sharedMesh = GetMesh();
		if(meshCollider != null) meshCollider.sharedMesh = GetMesh();
	}

	protected abstract Mesh GetMesh ();
}