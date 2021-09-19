using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(SnapToGrid))]
public class SnapToGridEditor : BaseEditor<SnapToGrid> {
	public override void OnInspectorGUI () {
		base.OnInspectorGUI();
	}

	public override void OnSceneGUI () {
		base.OnSceneGUI();
		var overlaps = Physics.OverlapSphere(data.transform.position, 3, LayerMask.GetMask("Peg"), QueryTriggerInteraction.Collide);
        foreach(var overlap in overlaps) {
			if(overlap.transform == data.transform || data.transform.GetChildren().Any(x => x == overlap.transform)) continue;
			// var snapToGrid = overlap.GetComponent<SnapToGrid>();
			// if(snapToGrid == null || !snapToGrid.addToGrid) continue;
			// Handles.DrawLine(data.transform.position, overlap.transform.position);

			foreach(var direction in SnapDirections()) {
				HandlesX.BeginColor(Color.white.WithAlpha(0.5f));
				// Handles.DrawLine(overlap.transform.position, overlap.transform.position + direction*0.1f);
				var snapPoint = overlap.transform.position + direction * TwirlyCharacterController.Instance.settings.fixedGait;
				Handles.DrawLine(overlap.transform.position, snapPoint);
				Handles.DrawWireDisc(snapPoint, Vector3.up, 0.05f);
				if(Vector2.Distance(data.transform.position.XZ(), snapPoint.XZ()) < 1f) {
					Handles.DrawLine(overlap.transform.position, snapPoint);
					Handles.DrawWireDisc(snapPoint, Vector3.up, 0.15f);
				}
				if(Vector2.Distance(data.transform.position.XZ(), snapPoint.XZ()) < 0.5f) {
					data.transform.position = new Vector3(snapPoint.x, data.transform.position.y, snapPoint.z);
				}
				HandlesX.EndColor();
			}
		}
	}
	public static IEnumerable<Vector3> SnapDirections() {
		var r = 1f/12;
		for(int i = 0; i < 12; i++) {
			yield return Vector2X.WithDegrees(i*r*360).ToVector3XZY();
		}
    }
}