using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityX.Geometry;
using UnityX.MeshBuilder;
public static class FloorTileMeshGenerator {
	[System.Serializable]
	public class FloorTileMeshBuilderParams {
		public MeshDrawFaces faces = MeshDrawFaces.Front;
		public Vector2[] polygon;
		public RingParams[] rings;
		// public Vector3 eulerAngles;
		public Color color;
	}
	[System.Serializable]
	public class RingParams {
		public float normalizedDistanceFromCenter;
		// public float[] heights;
		public Vector3[] verts;
	}

	public static Mesh GetMesh (FloorTileMeshBuilderParams meshParams) {
		MeshBuilder mb = new MeshBuilder();
		// List<Vector3> normals = new List<Vector3>();

		// var rotation = Quaternion.Euler(-meshParams.eulerAngles);

		var cornersPolygon = new Polygon(meshParams.polygon);
		bool flipDirection = !cornersPolygon.GetIsClockwise();

		bool drawBack = FlagsX.IsSet((int)meshParams.faces, (int)MeshDrawFaces.Back) ^ flipDirection;
		bool drawFront = FlagsX.IsSet((int)meshParams.faces, (int)MeshDrawFaces.Front) ^ flipDirection;
		
		Vector3[][] rings = new Vector3[meshParams.rings.Length][];
		for(int r = 0; r < rings.Length; r++) {
			rings[r] = new Vector3[meshParams.polygon.Length];
			for(int i = 0; i < meshParams.polygon.Length; i++) {
				// var corner2D = meshParams.polygon[i];
				// corner2D = Vector2.Lerp(Vector2.zero, meshParams.polygon[i], meshParams.rings[r].normalizedDistanceFromCenter);
				// rings[r][i] = GetVector3FromProperties(corner2D, meshParams.rings[r].heights[i], rotation);
				rings[r][i] = meshParams.rings[r].verts[i];
			}
		}

		for(int r = 0; r < rings.Length-1; r++) {
			for(int i = 0; i < rings[r].Length; i++) {

				Vector3 startCornerOuter = rings[r][i];
				Vector3 endCornerOuter = rings[r].GetRepeating(i+1);
				Vector3 startCornerInner = rings[r+1][i];
				Vector3 endCornerInner = rings[r+1].GetRepeating(i+1);

				AddPlaneParams planeInput = new AddPlaneParams();
				planeInput.front = drawFront;
				planeInput.back = drawBack;
				planeInput.uvTopLeft = new Vector2(0,1);
				planeInput.uvTopRight = new Vector2(1,1);
				planeInput.uvBottomRight = new Vector2(1,0);
				planeInput.uvBottomLeft = new Vector2(0,0);
				planeInput.topLeft = startCornerOuter; planeInput.topRight = endCornerOuter; planeInput.bottomRight = endCornerInner; planeInput.bottomLeft = startCornerInner;
				planeInput.colorBottomLeft = planeInput.colorBottomRight = planeInput.colorTopLeft = planeInput.colorTopRight = meshParams.color;

				mb.AddPlane(planeInput);
			}
		}

		MeshBakeParams bakeParams = new MeshBakeParams();
		bakeParams.recalculateNormals = true;
		bakeParams.recalculateBounds = true;

		var mesh = mb.ToMesh(bakeParams);
		// NormalSolver.RecalculateNormals(mesh, 30);
		return mesh;
	}
}