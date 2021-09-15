using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Flags]
public enum MeshDrawFaces {
	Front = 1 << 0,
	Back = 1 << 1,
	Both = Front | Back,
	None = 0
}

public class ExtrudedPolygonMeshGenerator {	
	public static Mesh Create (ExtrudedPolygonMeshParams input) {
		var mesh = new Mesh();
		mesh.name = "Polygon Mesh";
		Create(input, ref mesh);
		return mesh;
	}

	public static void Create (ExtrudedPolygonMeshParams input, ref Mesh mesh) {
		mesh.Clear();

		if(input == null || !input.polygon.IsValid()) return;
		
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		// List<Vector3> normals = new List<Vector3>();
		List<Color> colors = new List<Color>();

		bool flipDirection = !input.polygon.GetIsClockwise() ^ (input.offsetMatrix.determinant < 0 ? true : false);

		
		var faceVerts2D = input.polygon.vertices;
		List<int> faceTrisFacingUp = new List<int>();
		Triangulator.GenerateIndices(faceVerts2D, faceTrisFacingUp);

		bool useTopHeightArray = input.topHeights != null && input.topHeights.Length == faceVerts2D.Length;
		bool useBottomHeightArray = input.bottomHeights != null && input.bottomHeights.Length == faceVerts2D.Length;
		bool useColorArrays = input.topColors != null && input.topColors.Length == faceVerts2D.Length && input.bottomColors != null && input.bottomColors.Length == faceVerts2D.Length;
		var rect = input.polygon.GetRect();
		Vector2 offset = new Vector3(rect.width * (input.pivot.x-0.5f), rect.height * (input.pivot.z-0.5f), 0);
		
		Vector3[] topVerts = new Vector3[faceVerts2D.Length];
		Vector3[] bottomVerts = new Vector3[faceVerts2D.Length];

		bool drawTop = input.topFaces != MeshDrawFaces.None;
		bool drawBottom = input.bottomFaces != MeshDrawFaces.None;
		bool drawSides = input.sideFaces != MeshDrawFaces.None;
		for(int i = 0; i < faceVerts2D.Length; i++) {
			float x = faceVerts2D[i].x+offset.x;
			float y = 0;
			float z = faceVerts2D[i].y+offset.y;
			float topY = input.topHeight + y;
			float bottomY = input.bottomHeight + y;
			if(useTopHeightArray) topY = input.topHeights[i];
			if(useBottomHeightArray) bottomY = input.bottomHeights[i];
			if(drawTop || drawSides) topVerts[i] = input.offsetMatrix.MultiplyPoint(new Vector3(x, z, topY));
			if(drawBottom || drawSides) bottomVerts[i] = input.offsetMatrix.MultiplyPoint(new Vector3(x, z, bottomY));
		}
		int triOffset = verts.Count;
		
		if(drawTop) {
			bool drawBack = FlagsX.IsSet((int)input.topFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.IsSet((int)input.topFaces, (int)MeshDrawFaces.Front);
			if(drawFront) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					verts.Add(topVerts[faceTrisFacingUp[i+2]]);
					verts.Add(topVerts[faceTrisFacingUp[i+1]]);
					verts.Add(topVerts[faceTrisFacingUp[i]]);
					tris.Add(triOffset + i);
					tris.Add(triOffset + i+1);
					tris.Add(triOffset + i+2);
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+2]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+1]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i]]));
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i+2]] : input.topColor);
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i+1]] : input.topColor);
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i]] : input.topColor);
				}
			}
			
			if(drawBack) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					verts.Add(topVerts[faceTrisFacingUp[i]]);
					verts.Add(topVerts[faceTrisFacingUp[i+1]]);
					verts.Add(topVerts[faceTrisFacingUp[i+2]]);
					tris.Add(triOffset + i);
					tris.Add(triOffset + i+1);
					tris.Add(triOffset + i+2);
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+1]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+2]]));
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i]] : input.topColor);
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i+1]] : input.topColor);
					colors.Add(useColorArrays ? input.topColors[faceTrisFacingUp[i+2]] : input.topColor);
				}
			}
		}

		if(drawBottom) {
			bool drawBack = FlagsX.IsSet((int)input.bottomFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.IsSet((int)input.bottomFaces, (int)MeshDrawFaces.Front);
			if(drawFront) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					verts.Add(bottomVerts[faceTrisFacingUp[i]]);
					verts.Add(bottomVerts[faceTrisFacingUp[i+1]]);
					verts.Add(bottomVerts[faceTrisFacingUp[i+2]]);
					tris.Add(triOffset + i);
					tris.Add(triOffset + i+1);
					tris.Add(triOffset + i+2);
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+1]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+2]]));
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i]] : input.bottomColor);
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i+1]] : input.bottomColor);
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i+2]] : input.bottomColor);
				}
			}
			
			if(drawBack) {
				triOffset = verts.Count;
				for(int i = 0; i < faceTrisFacingUp.Count; i += 3) {
					verts.Add(bottomVerts[faceTrisFacingUp[i+2]]);
					verts.Add(bottomVerts[faceTrisFacingUp[i+1]]);
					verts.Add(bottomVerts[faceTrisFacingUp[i]]);
					tris.Add(triOffset + i);
					tris.Add(triOffset + i+1);
					tris.Add(triOffset + i+2);
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+2]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i+1]]));
					uvs.Add(rect.GetNormalizedPositionInsideRect(topVerts[faceTrisFacingUp[i]]));
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i+2]] : input.bottomColor);
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i+1]] : input.bottomColor);
					colors.Add(useColorArrays ? input.bottomColors[faceTrisFacingUp[i]] : input.bottomColor);
				}
			}
			// triOffset = verts.Count;
			// int[] bottomTris = new int[faceTrisFacingUp.Count];
			// int triLengthMinusOne = faceTrisFacingUp.Count-1;
			// for(int i = 0; i < bottomTris.Length; i++) bottomTris[i] = triOffset + faceTrisFacingUp[triLengthMinusOne - i];
			// verts.AddRange(bottomVerts);
			// tris.AddRange(bottomTris);
		}
		
		if(drawSides) {
			bool drawBack = FlagsX.IsSet((int)input.sideFaces, (int)MeshDrawFaces.Back);
			bool drawFront = FlagsX.IsSet((int)input.sideFaces, (int)MeshDrawFaces.Front);
			
			var numFaces = faceVerts2D.Length;
			Vector3 topLeft;
			Vector3 bottomLeft;
			Vector3 topRight;
			Vector3 bottomRight;

			Color topLeftColor;
			Color bottomLeftColor;
			Color topRightColor;
			Color bottomRightColor;


			if(flipDirection ? drawBack : drawFront) {
				int numVerts = numFaces * 6;
				int numTris = numFaces * 6;
				
				Vector3[] vertArray = new Vector3[numVerts];
				int[] triArray = new int[numTris];
				Vector2[] uvArray = new Vector2[numTris];
				Color[] colorsArray = new Color[numVerts];
				
				int vertIndex = 0;
				triOffset = verts.Count;

				for(int i = 0; i < numFaces; i++) {
                    topLeft = topVerts[i];
					bottomLeft = bottomVerts[i];
					topRight = topVerts.GetRepeating(i-1);
					bottomRight = bottomVerts.GetRepeating(i-1);

					topLeftColor = useColorArrays ? input.topColors[i] : input.topColor;
					bottomLeftColor = useColorArrays ? input.bottomColors[i] : input.bottomColor;
					topRightColor = useColorArrays ? input.topColors.GetRepeating(i-1) : input.topColor;
					bottomRightColor = useColorArrays ? input.bottomColors.GetRepeating(i-1) : input.bottomColor;

					vertArray[vertIndex] = bottomLeft;
					vertArray[vertIndex + 1] = topRight;
					vertArray[vertIndex + 2] = topLeft;
					vertArray[vertIndex + 3] = bottomLeft;
					vertArray[vertIndex + 4] = bottomRight;
					vertArray[vertIndex + 5] = topRight;
					
					triArray[vertIndex] = triOffset + 0;
					triArray[vertIndex + 1] = triOffset + 1;
					triArray[vertIndex + 2] = triOffset + 2;
					triArray[vertIndex + 3] = triOffset + 3;
					triArray[vertIndex + 4] = triOffset + 4;
					triArray[vertIndex + 5] = triOffset + 5;
					
					uvArray[vertIndex] = uvBottomLeft;
					uvArray[vertIndex + 1] = uvTopRight;
					uvArray[vertIndex + 2] = uvTopLeft;
					uvArray[vertIndex + 3] = uvBottomLeft;
					uvArray[vertIndex + 4] = uvBottomRight;
					uvArray[vertIndex + 5] = uvTopRight;
					
					colorsArray[vertIndex] = bottomLeftColor;
					colorsArray[vertIndex + 1] = topRightColor;
					colorsArray[vertIndex + 2] = topLeftColor;
					colorsArray[vertIndex + 3] = bottomLeftColor;
					colorsArray[vertIndex + 4] = bottomRightColor;
					colorsArray[vertIndex + 5] = topRightColor;
					
					vertIndex += 6;
					triOffset += 6;
				}

				verts.AddRange(vertArray);
				tris.AddRange(triArray);
				uvs.AddRange(uvArray);
				colors.AddRange(colorsArray);
			}

			if(flipDirection ? drawFront : drawBack) {
				int numVerts = numFaces * 6;
				int numTris = numFaces * 6;
				
				Vector3[] vertArray = new Vector3[numVerts];
				int[] triArray = new int[numTris];
				Vector2[] uvArray = new Vector2[numTris];
				Color[] colorsArray = new Color[numVerts];
				
				int vertIndex = 0;
				triOffset = verts.Count;
				
				for(int i = 0; i < numFaces; i++) {
					topLeft = topVerts[i];
					bottomLeft = bottomVerts[i];
					topRight = topVerts.GetRepeating(i-1);
					bottomRight = bottomVerts.GetRepeating(i-1);

					topLeftColor = useColorArrays ? input.topColors[i] : input.topColor;
					bottomLeftColor = useColorArrays ? input.bottomColors[i] : input.bottomColor;
					topRightColor = useColorArrays ? input.topColors.GetRepeating(i-1) : input.topColor;
					bottomRightColor = useColorArrays ? input.bottomColors.GetRepeating(i-1) : input.bottomColor;
					
					vertArray[vertIndex] = topLeft;
					vertArray[vertIndex + 1] = topRight;
					vertArray[vertIndex + 2] = bottomLeft;
					vertArray[vertIndex + 3] = topRight;
					vertArray[vertIndex + 4] = bottomRight;
					vertArray[vertIndex + 5] = bottomLeft;
					
					triArray[vertIndex] = triOffset + 0;
					triArray[vertIndex + 1] = triOffset + 1;
					triArray[vertIndex + 2] = triOffset + 2;
					triArray[vertIndex + 3] = triOffset + 3;
					triArray[vertIndex + 4] = triOffset + 4;
					triArray[vertIndex + 5] = triOffset + 5;
					
					uvArray[vertIndex] = uvTopLeft;
					uvArray[vertIndex + 1] = uvTopRight;
					uvArray[vertIndex + 2] = uvBottomLeft;
					uvArray[vertIndex + 3] = uvTopRight;
					uvArray[vertIndex + 4] = uvBottomRight;
					uvArray[vertIndex + 5] = uvBottomLeft;

					colorsArray[vertIndex] = topLeftColor;
					colorsArray[vertIndex + 1] = topRightColor;
					colorsArray[vertIndex + 2] = bottomLeftColor;
					colorsArray[vertIndex + 3] = topRightColor;
					colorsArray[vertIndex + 4] = bottomRightColor;
					colorsArray[vertIndex + 5] = bottomLeftColor;
					
					vertIndex += 6;
					triOffset += 6;
				}
				
				verts.AddRange(vertArray);
				tris.AddRange(triArray);
				uvs.AddRange(uvArray);
				colors.AddRange(colorsArray);
			}
		}
		
		mesh.SetVertices(verts);
		mesh.SetTriangles(tris, 0);
		mesh.SetUVs(0, uvs);
		mesh.SetColors(colors);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	static Vector2 uvTopLeft = new Vector2(0,1);
	static Vector2 uvTopRight = new Vector2(1,1);
	static Vector2 uvBottomRight = new Vector2(1,0);
	static Vector2 uvBottomLeft = new Vector2(0,0);
	/*
	void AddPlane (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, bool front, bool back, List<Vector3> verts, List<int> tris, List<Vector2> uvs) {
		if(front) {
			List<Vector3> frontVerts = new List<Vector3>(){
				topLeft,topRight,bottomLeft,
				topRight,bottomRight,bottomLeft
			};
			verts.AddRange(frontVerts);
			int t = tris.Count;
			for(int j = 0; j < 6; j++){
				tris.Add(j+t);
			}
			uvs.AddRange(
				new List<Vector2>(){
					uvTopLeft,uvTopRight,uvBottomLeft,
					uvTopRight,uvBottomRight,uvBottomLeft
				}
			);
		}
		if(back) {
			List<Vector3> backVerts = new List<Vector3>(){
				bottomLeft,topRight,topLeft,
				bottomLeft,bottomRight,topRight
			};
			verts.AddRange(backVerts);
			int t = tris.Count;
			for(int j = 0; j < 6; j++){
				tris.Add(j+t);
			}
			uvs.AddRange(
				new List<Vector2>(){
					uvBottomLeft,uvTopRight,uvTopLeft,
					uvBottomLeft,uvBottomRight,uvTopRight
				}
			);
		}
	}
	 */
}
