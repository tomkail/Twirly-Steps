using UnityX.Geometry;
using UnityEngine;

[System.Serializable]
public class ExtrudedPolygonMeshParams {
	public Polygon polygon;
	
	public Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
	
	public float[] topHeights;
	public float[] bottomHeights;
	public float topHeight = 1;
	public float bottomHeight = 0;

	public Color topColor = Color.white;
	public Color bottomColor = Color.white;
	public Color[] topColors;
	public Color[] bottomColors;

	public MeshDrawFaces topFaces = MeshDrawFaces.Front;
	public MeshDrawFaces bottomFaces = MeshDrawFaces.Front;
	public MeshDrawFaces sideFaces = MeshDrawFaces.Front;
	public Matrix4x4 offsetMatrix = Matrix4x4.identity;
}
