using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExtrudedPolygonRenderer), true)]
[CanEditMultipleObjects]
public class ExtrudedPolygonRendererEditor : BaseEditor<ExtrudedPolygonRenderer> {
	PolygonEditorHandles polygonEditor;

	public override void OnEnable() {
		base.OnEnable();
		polygonEditor = new PolygonEditorHandles(data.transform);
		polygonEditor = new PolygonEditorHandles(data.transform, data.input.offsetMatrix);
	}		

	void OnDisable() {
        polygonEditor.Destroy();
		if(data == null) return;
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(EditorGUI.EndChangeCheck()) {
			foreach(var data in datas) {
				data.RebuildMesh();
			}
		}
	}
	protected override void OnMultiEditSceneGUI () {
        foreach(var data in datas) {
			if(!data.editable) return;
			Undo.RecordObject(data, "Edit polygon");
			polygonEditor.offsetMatrix = data.input.offsetMatrix;
			if(polygonEditor.OnSceneGUI(data.input.polygon)) {
				data.RebuildMesh();
			}
		}
	}
}