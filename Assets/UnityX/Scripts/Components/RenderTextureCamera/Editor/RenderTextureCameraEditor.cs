using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RenderTextureCamera))]
public class RenderTextureCameraEditor : Editor {
    SerializedProperty _renderTextureProperty;
	void OnEnable() {
		_renderTextureProperty = serializedObject.FindProperty("rt");
	}

	public override bool RequiresConstantRepaint() {
		return true;
	}

	public override bool HasPreviewGUI() {return true;}

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
		if(Event.current.type == EventType.Repaint && _renderTextureProperty.objectReferenceValue != null) {
			EditorGUI.DrawPreviewTexture(r, _renderTextureProperty.objectReferenceValue as RenderTexture, null, ScaleMode.ScaleToFit);
		}
    }
}