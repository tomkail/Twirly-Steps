using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityX.Geometry;

[RequireComponent(typeof(MeshFilter))]
public class ExtrudedPolygonRenderer : MonoBehaviour {
    public bool editable = true;
	MeshFilter _meshFilter;
    public MeshFilter meshFilter {
        get {
            if(_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            return _meshFilter;
        }
    }
	MeshCollider _meshCollider;
    public MeshCollider meshCollider {
        get {
            if(_meshCollider == null) _meshCollider = GetComponent<MeshCollider>();
            return _meshCollider;
        }
    }

	public ExtrudedPolygonMeshParams input;
	[AssetSaver]
	public Mesh mesh;


	protected void Reset () {
        DestroyMesh();
        GetMesh();
    }
    
    protected void OnEnable () {
        // GetMesh();
        // if(Application.isPlaying) {
        //     DestroyMesh();
        // } else {
        //     mesh.Clear();
        // }
        // RebuildMesh();
    }

    void OnValidate () {
        RebuildMesh();
    }

    protected void OnDestroy () {
        DestroyMesh();
    }
    protected void DestroyMesh () {
        if(mesh != null) {
            ObjectX.DestroyAutomatic(mesh);
            mesh = null;
        }
    }

    protected void GetMesh () {
        if(mesh != null && mesh.name != "Region Renderer Mesh "+ GetInstanceID()) {
            mesh = null;
        }
        if(mesh == null) {
            if(meshFilter != null && meshFilter.name == "Region Renderer Mesh "+ GetInstanceID()) {
                mesh = meshFilter.mesh;
            } else {
                mesh = new Mesh();
                mesh.name = "Region Renderer Mesh "+ GetInstanceID();
            }
        }
        if(meshFilter != null) meshFilter.mesh = mesh;
        if(meshCollider != null) meshCollider.sharedMesh = mesh;
    }
    
    public void RebuildMesh () {
        GetMesh();
		ExtrudedPolygonMeshGenerator.Create(input, ref mesh);
    }
}
