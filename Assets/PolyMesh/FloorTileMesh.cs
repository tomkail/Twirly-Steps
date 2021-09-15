using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class FloorTileMesh : MonoBehaviour {
	public FloorTileMeshGenerator.FloorTileMeshBuilderParams meshParams;
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

    [ContextMenu("Generate")]
    public void Generate () {
        meshCollider.sharedMesh = meshFilter.sharedMesh = FloorTileMeshGenerator.GetMesh(meshParams);
    }
    void OnDrawGizmosSelected () {
        if(!Application.isPlaying)
            Generate();
    }
}
