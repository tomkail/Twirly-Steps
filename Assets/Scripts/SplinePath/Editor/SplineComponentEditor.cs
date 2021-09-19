using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SplineSystem;

[InitializeOnLoad]
[CustomEditor(typeof(SplineComponent)), CanEditMultipleObjects]
public class SplineComponentEditor : Editor {
    SplineComponent data;
	List<SplineComponent> datas;

	SplineEditor splineEditor;

    static SplineComponentEditor () {
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    static void DuringSceneGUI (SceneView sceneView) {
        var splines = Object.FindObjectsOfType<SplineComponent>();
        foreach(var spline in splines) {
            if(spline.handleSettings.showWhenNotSelected && !Selection.Contains(spline.gameObject)) 
                DrawSplineInSceneGUI(spline);
        }
    }

    static void DrawSplineInSceneGUI (SplineComponent spline) {
        
        var splineEditor = new SplineEditor(spline.transform);
        splineEditor.pointsPerMeter = 100;
        splineEditor.editable = false;
        
        if(splineEditor.OnSceneGUI(spline.spline)) {
            Undo.RecordObject(spline, "Modified Spline");
            spline.spline.RefreshCurveData();
            EditorUtility.SetDirty(spline);
            spline.OnValidate();
        }

        splineEditor.Destroy();
    }

    void OnEnable() {
        SetData();
		Undo.undoRedoPerformed += HandleUndoRedoCallback;
        splineEditor = new SplineEditor(data.transform);
        splineEditor.pointsPerMeter = 100;
    }

	void OnDisable() {
		Undo.undoRedoPerformed -= HandleUndoRedoCallback;
        splineEditor.Destroy();
		if(data == null) return;
	}

    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if(GUI.changed && target != null) {
			EditorUtility.SetDirty(target);
		}
		if(EditorGUI.EndChangeCheck()) {
            data.spline.RefreshCurveData();
            data.OnValidate();
        }
	}
	
	void SetData () {
		// If an object has been deleted under our feet we need to handle it gracefully
		// (Previously it would assert)
		// This can happen if an editor script deletes an object that you previously had selected.
		if( target == null ) {
			data = null;
		} else {
			Debug.Assert(target as SplineComponent != null, "Cannot cast "+target + " to "+typeof(SplineComponent));
			data = (SplineComponent) target;
		}

		// datas = new List<SplineComponent>();
		// foreach(Object t in targets) {
		// 	if( t == null ) continue;
		// 	Debug.Assert(t as SplineComponent != null, "Cannot cast "+t + " to "+typeof(SplineComponent));
		// 	datas.Add((SplineComponent) t); 
		// }
	}

    void OnSceneGUI () {
        Undo.RecordObject(target, "Modified Spline");
        if(splineEditor.OnSceneGUI(data.spline)) {
            data.spline.RefreshCurveData();
            EditorUtility.SetDirty(target);
            data.OnValidate();
        }
    }

	void HandleUndoRedoCallback () {
        data.spline.RefreshCurveData();
        data.OnValidate();
	}
}