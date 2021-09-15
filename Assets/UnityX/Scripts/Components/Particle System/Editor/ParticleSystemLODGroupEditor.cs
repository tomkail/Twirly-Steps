using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ParticleSystemLODGroup))]
public class ParticleSystemLODGroupEditor : Editor {
	private int m_SelectedLODSlider = -1;
	private int m_SelectedLOD = -1;
	private int m_NumberOfLODs;

	private ParticleSystemLODGroup m_LODGroup;
	private bool m_IsPrefab;

	private SerializedProperty referenceCamera;
	private SerializedProperty m_CullingDistance;
	private SerializedProperty m_FadeTransitionWidth;
	private SerializedProperty m_LODs;

	void OnEnable() {
		// TODO: support multi-editing?
		referenceCamera = serializedObject.FindProperty("referenceCamera");
		m_CullingDistance = serializedObject.FindProperty("m_CullingDistance");
		m_FadeTransitionWidth = serializedObject.FindProperty("m_FadeTransitionWidth");
		m_LODs = serializedObject.FindProperty("m_LODs");

		EditorApplication.update += Update;

		m_LODGroup = (ParticleSystemLODGroup)target;

		// Calculate if the newly selected LOD group is a prefab... they require special handling
		m_IsPrefab = PrefabUtility.IsPartOfPrefabAsset(m_LODGroup.gameObject);

		Repaint();
	}

	void OnDisable()
	{
		EditorApplication.update -= Update;
	}



	public static bool IsSceneGUIEnabled()
	{
		if (Event.current.type != EventType.Repaint
			|| Camera.current == null
			|| SceneView.lastActiveSceneView != SceneView.currentDrawingSceneView)
		{
			return false;
		}

		return true;
	}

	static Vector3 CalculateWorldReferencePoint (ParticleSystemLODGroup LODGroup) {
		return LODGroup.transform.position;
	}

	float maxDistance => m_CullingDistance.floatValue;
	
	void OnSceneGUI()
	{
		/*
		Camera camera = SceneView.lastActiveSceneView.camera;
		var worldReferencePoint = CalculateWorldReferencePoint(m_LODGroup);

		if (Vector3.Dot(camera.transform.forward,
			(camera.transform.position - worldReferencePoint).normalized) > 0)
			return;

		var info = LODUtility.CalculateVisualizationData(camera, m_LODGroup, -1);
		float size = info.worldSpaceSize;

		// Draw cap around LOD to visualize it's size
		Handles.color = info.activeLODLevel != -1 ? ParticleSystemLODGroupGUI.kLODColors[info.activeLODLevel] : ParticleSystemLODGroupGUI.kCulledLODColor;

		Handles.SelectionFrame(0, worldReferencePoint, camera.transform.rotation, size / 2);

		// Calculate a screen rect for the on scene title
		Vector3 sideways = camera.transform.right * size / 2.0f;
		Vector3 up = camera.transform.up * size / 2.0f;
		var rect = CalculateScreenRect(
			new[]
			{
				worldReferencePoint - sideways + up,
				worldReferencePoint - sideways - up,
				worldReferencePoint + sideways + up,
				worldReferencePoint + sideways - up
			});

		// Place the screen space lable directaly under the
		var midPoint = rect.x + rect.width / 2.0f;
		rect = new Rect(midPoint - ParticleSystemLODGroupGUI.kSceneLabelHalfWidth, rect.yMax, ParticleSystemLODGroupGUI.kSceneLabelHalfWidth * 2, ParticleSystemLODGroupGUI.kSceneLabelHeight);

		if (rect.yMax > Screen.height - ParticleSystemLODGroupGUI.kSceneLabelHeight)
			rect.y = Screen.height - ParticleSystemLODGroupGUI.kSceneLabelHeight - ParticleSystemLODGroupGUI.kSceneHeaderOffset;

		Handles.BeginGUI();
		GUI.Label(rect, GUIContent.none, EditorStyles.notificationBackground);
		EditorGUI.DoDropShadowLabel(rect, GUIContent.Temp(info.activeLODLevel >= 0 ? "LOD " + info.activeLODLevel : "Culled"), ParticleSystemLODGroupGUI.Styles.m_LODLevelNotifyText, 0.3f);
		Handles.EndGUI();
		*/
	}

	private Vector3 m_LastCameraPos = Vector3.zero;
	public void Update()
	{
		if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null)
		{
			return;
		}

		// Update the last camera positon and repaint if the camera has moved
		if (SceneView.lastActiveSceneView.camera.transform.position != m_LastCameraPos)
		{
			m_LastCameraPos = SceneView.lastActiveSceneView.camera.transform.position;
			Repaint();
		}
	}

	private const string kLODDataPath = "m_LODs.Array.data[{0}]";
	private const string kPixelHeightDataPath = "m_LODs.Array.data[{0}].normalizedTransitionDistance";
	private const string kRenderRootPath = "m_LODs.Array.data[{0}].renderers";
	private const string kFadeTransitionWidthDataPath = "m_LODs.Array.data[{0}].fadeTransitionWidth";
	
	private int activeLOD
	{
		get {return m_SelectedLOD; }
	}

	private bool IsLODUsingCrossFadeWidth(int lod)
	{
		// SpeedTree: only last mesh LOD and billboard LOD do crossfade
		if (m_NumberOfLODs > 0 && m_SelectedLOD == m_NumberOfLODs - 1)
			return true;
		if (m_NumberOfLODs > 1 && m_SelectedLOD == m_NumberOfLODs - 2)
		{
			// the second last LOD uses cross-fade if the last LOD is a billboard
			var renderers = serializedObject.FindProperty(string.Format(kRenderRootPath, m_NumberOfLODs - 1));
			if (renderers.arraySize != 1)
				return false;
			// var renderer = renderers.GetArrayElementAtIndex(0).objectReferenceValue;
			// if (renderer is BillboardRenderer || (renderer is MeshRenderer && m_LastLODIsBillboard.boolValue))
			return true;
		}
		return false;
	}

	public override void OnInspectorGUI()
	{
		var initiallyEnabled = GUI.enabled;

		// Grab the latest data from the object
		serializedObject.Update();

		EditorGUILayout.HelpBox("Fades Particle Systems containing the ParticleSystemAlphaController component.\nLODs are based on distance to a reference Camera.", MessageType.Info);

		EditorGUILayout.PropertyField(referenceCamera);
		EditorGUILayout.PropertyField(m_CullingDistance);
		EditorGUILayout.PropertyField(m_FadeTransitionWidth);

		m_NumberOfLODs = m_LODs.arraySize;

		// This could happen when you select a newly inserted LOD level and then undo the insertion.
		// It's valid for m_SelectedLOD to become -1, which means nothing is selected.
		if (m_SelectedLOD >= m_NumberOfLODs)
		{
			m_SelectedLOD = m_NumberOfLODs - 1;
		}

		// Prepass to remove all empty renderers
		if (m_NumberOfLODs > 0 && activeLOD >= 0)
		{
			var renderersProperty = serializedObject.FindProperty(string.Format(kRenderRootPath, activeLOD));
			for (var i = renderersProperty.arraySize - 1; i >= 0; i--)
			{
				var rendererRef = renderersProperty.GetArrayElementAtIndex(i);
				var renderer = rendererRef.objectReferenceValue as ParticleSystemAlphaController;

				if (renderer == null)
					renderersProperty.DeleteArrayElementAtIndex(i);
			}
		}

		// Add some space at the top..
		GUILayout.Space(ParticleSystemLODGroupGUI.kSliderBarTopMargin);

		// Precalculate and cache the slider bar position for this update
		var sliderBarPosition = GUILayoutUtility.GetRect(0, ParticleSystemLODGroupGUI.kSliderBarHeight, GUILayout.ExpandWidth(true));

		// Precalculate the lod info (button locations / ranges ect)
		var lods = ParticleSystemLODGroupGUI.CreateLODInfos(m_NumberOfLODs, sliderBarPosition,
			i => string.Format("LOD {0}", i),
			i => serializedObject.FindProperty(string.Format(kPixelHeightDataPath, i)).floatValue);

		DrawLODLevelSlider(sliderBarPosition, lods);
		GUILayout.Space(ParticleSystemLODGroupGUI.kSliderBarBottomMargin);
		
		// Draw the info for the selected LOD
		if (m_NumberOfLODs > 0 && activeLOD >= 0 && activeLOD < m_NumberOfLODs)
		{
			float contextWidth = (float)typeof(EditorGUIUtility).GetProperty("contextWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null, null);
			DrawRenderersInfo(contextWidth);
		}

		GUILayout.Space(8);

		// Apply the property, handle undo
		serializedObject.ApplyModifiedProperties();

		GUI.enabled = initiallyEnabled;
	}

	// Draw the renderers for the current LOD group
	// Arrange in a grid
	private void DrawRenderersInfo(float availableWidth)
	{
		var horizontalCount = Mathf.FloorToInt(availableWidth / ParticleSystemLODGroupGUI.kRenderersButtonHeight);
		var titleArea = GUILayoutUtility.GetRect(ParticleSystemLODGroupGUI.Styles.m_RendersTitle, ParticleSystemLODGroupGUI.Styles.m_LODSliderTextSelected);
		if (Event.current.type == EventType.Repaint)
			EditorStyles.label.Draw(titleArea, ParticleSystemLODGroupGUI.Styles.m_RendersTitle, false, false, false, false);

		// Draw renderer info
		var renderersProperty = serializedObject.FindProperty(string.Format(kRenderRootPath, activeLOD));

		var numberOfButtons = renderersProperty.arraySize + 1;
		var numberOfRows = Mathf.CeilToInt(numberOfButtons / (float)horizontalCount);

		var drawArea = GUILayoutUtility.GetRect(0, numberOfRows * ParticleSystemLODGroupGUI.kRenderersButtonHeight, GUILayout.ExpandWidth(true));
		var rendererArea = drawArea;
		GUI.Box(drawArea, GUIContent.none);
		rendererArea.width -= 2 * ParticleSystemLODGroupGUI.kRenderAreaForegroundPadding;
		rendererArea.x += ParticleSystemLODGroupGUI.kRenderAreaForegroundPadding;

		var buttonWidth = rendererArea.width / horizontalCount;

		var buttons = new List<Rect>();

		for (int i = 0; i < numberOfRows; i++)
		{
			for (int k = 0; k < horizontalCount && (i * horizontalCount + k) < renderersProperty.arraySize; k++)
			{
				var drawPos = new Rect(
					ParticleSystemLODGroupGUI.kButtonPadding + rendererArea.x + k * buttonWidth,
					ParticleSystemLODGroupGUI.kButtonPadding + rendererArea.y + i * ParticleSystemLODGroupGUI.kRenderersButtonHeight,
					buttonWidth - ParticleSystemLODGroupGUI.kButtonPadding * 2,
					ParticleSystemLODGroupGUI.kRenderersButtonHeight - ParticleSystemLODGroupGUI.kButtonPadding * 2);
				buttons.Add(drawPos);
				DrawRendererButton(drawPos, i * horizontalCount + k);
			}
		}

		if (m_IsPrefab)
			return;

		//+ button
		int horizontalPos = (numberOfButtons - 1) % horizontalCount;
		int verticalPos = numberOfRows - 1;
		HandleAddRenderer(new Rect(
			ParticleSystemLODGroupGUI.kButtonPadding + rendererArea.x + horizontalPos * buttonWidth,
			ParticleSystemLODGroupGUI.kButtonPadding + rendererArea.y + verticalPos * ParticleSystemLODGroupGUI.kRenderersButtonHeight,
			buttonWidth - ParticleSystemLODGroupGUI.kButtonPadding * 2,
			ParticleSystemLODGroupGUI.kRenderersButtonHeight - ParticleSystemLODGroupGUI.kButtonPadding * 2), buttons, drawArea);
	}

	private void HandleAddRenderer(Rect position, IEnumerable<Rect> alreadyDrawn, Rect drawArea)
	{
		Event evt = Event.current;
		switch (evt.type)
		{
			case EventType.Repaint:
			{
				ParticleSystemLODGroupGUI.Styles.m_LODStandardButton.Draw(position, GUIContent.none, false, false, false, false);
				ParticleSystemLODGroupGUI.Styles.m_LODRendererAddButton.Draw(new Rect(position.x - ParticleSystemLODGroupGUI.kButtonPadding, position.y, position.width, position.height), "Add", false, false, false, false);
				break;
			}
			case EventType.DragUpdated:
			case EventType.DragPerform:
			{
				bool dragArea = false;
				if (drawArea.Contains(evt.mousePosition))
				{
					if (alreadyDrawn.All(x => !x.Contains(evt.mousePosition)))
						dragArea = true;
				}

				if (!dragArea)
					break;

				// If we are over a valid range, make sure we have a game object...
				if (DragAndDrop.objectReferences.Length > 0)
				{
					DragAndDrop.visualMode = m_IsPrefab ? DragAndDropVisualMode.None : DragAndDropVisualMode.Copy;

					if (evt.type == EventType.DragPerform)
					{
						// First try gameobjects...
						var selectedGameObjects =
							from go in DragAndDrop.objectReferences
							where go as GameObject != null
							select go as GameObject;

						var renderers = GetRenderers(selectedGameObjects, true);
						AddGameObjectRenderers(renderers, true);
						DragAndDrop.AcceptDrag();

						evt.Use();
						break;
					}
				}
				evt.Use();
				break;
			}
			case EventType.MouseDown:
			{
				if (position.Contains(evt.mousePosition))
				{
					evt.Use();
					// int id = "LODGroupSelector".GetHashCode();
					// ObjectSelector.get.Show(null, typeof(ParticleSystemAlphaController), null, true);
					// ObjectSelector.get.objectSelectorID = id;
					GUIUtility.ExitGUI();
				}
				break;
			}
			case EventType.ExecuteCommand:
			{
				// string commandName = evt.commandName;
				// if (commandName == ObjectSelector.ObjectSelectorClosedCommand && ObjectSelector.get.objectSelectorID == "LODGroupSelector".GetHashCode())
				// {
				// 	var selectedObject = ObjectSelector.GetCurrentObject() as GameObject;
				// 	if (selectedObject != null)
				// 		AddGameObjectRenderers(GetRenderers(new List<GameObject> { selectedObject }, true), true);
				// 	evt.Use();
				// 	GUIUtility.ExitGUI();
				// }
				break;
			}
		}
	}

	private void DrawRendererButton(Rect position, int rendererIndex)
	{
		var renderersProperty = serializedObject.FindProperty(string.Format(kRenderRootPath, activeLOD));
		var rendererRef = renderersProperty.GetArrayElementAtIndex(rendererIndex);
		var renderer = rendererRef.objectReferenceValue as ParticleSystemAlphaController;

		var deleteButton = new Rect(position.xMax - ParticleSystemLODGroupGUI.kDeleteButtonSize, position.yMax - ParticleSystemLODGroupGUI.kDeleteButtonSize, ParticleSystemLODGroupGUI.kDeleteButtonSize, ParticleSystemLODGroupGUI.kDeleteButtonSize);

		Event evt = Event.current;
		switch (evt.type)
		{
			case EventType.Repaint:
			{
				if (renderer != null)
				{
					GUIContent content;

					var filter = renderer.GetComponent<MeshFilter>();
					// if (filter != null && filter.sharedMesh != null)
					// 	content = new GUIContent(AssetPreview.GetAssetPreview(filter.sharedMesh), renderer.gameObject.name);
					// else if (renderer is ParticleSystemAlphaController)
						// content = new GUIContent(AssetPreview.GetAssetPreview((renderer as ParticleSystemAlphaController).particleSystem.GetComponent<ParticleSystemRenderer>()), renderer.gameObject.name);
					// else if (renderer is ParticleSystem)
					// 	content = new GUIContent(AssetPreview.GetAssetPreview((renderer as ParticleSystem).billboard), renderer.gameObject.name);
					// else
						content = new GUIContent(renderer.gameObject.name);

					ParticleSystemLODGroupGUI.Styles.m_LODBlackBox.Draw(position, GUIContent.none, false, false, false, false);

					ParticleSystemLODGroupGUI.Styles.m_LODRendererButton.Draw(
						new Rect(
							position.x + ParticleSystemLODGroupGUI.kButtonPadding,
							position.y + ParticleSystemLODGroupGUI.kButtonPadding,
							position.width - 2 * ParticleSystemLODGroupGUI.kButtonPadding, position.height - 2 * ParticleSystemLODGroupGUI.kButtonPadding),
						content, false, false, false, false);
				}
				else
				{
					ParticleSystemLODGroupGUI.Styles.m_LODBlackBox.Draw(position, GUIContent.none, false, false, false, false);
					ParticleSystemLODGroupGUI.Styles.m_LODRendererButton.Draw(position, "<Empty>", false, false, false, false);
				}

				if (!m_IsPrefab)
				{
					ParticleSystemLODGroupGUI.Styles.m_LODBlackBox.Draw(deleteButton, GUIContent.none, false, false, false, false);
					ParticleSystemLODGroupGUI.Styles.m_LODRendererRemove.Draw(deleteButton, ParticleSystemLODGroupGUI.Styles.m_IconRendererMinus, false, false, false, false);
				}
				break;
			}
			case EventType.MouseDown:
			{
				if (!m_IsPrefab && deleteButton.Contains(evt.mousePosition))
				{
					renderersProperty.DeleteArrayElementAtIndex(rendererIndex);
					evt.Use();
					serializedObject.ApplyModifiedProperties();
					// m_LODGroup.RecalculateBounds();
				}
				else if (position.Contains(evt.mousePosition))
				{
					EditorGUIUtility.PingObject(renderer);
					evt.Use();
				}
				break;
			}
		}
	}


	// Get all the renderers that are attached to this game object
	private IEnumerable<ParticleSystemAlphaController> GetRenderers(IEnumerable<GameObject> selectedGameObjects, bool searchChildren)
	{
		// Only allow renderers that are parented to this LODGroup
		if (EditorUtility.IsPersistent(m_LODGroup))
			return new List<ParticleSystemAlphaController>();

		var validSearchObjects = from go in selectedGameObjects
			where go.transform.IsChildOf(m_LODGroup.transform)
			select go;

		var nonChildObjects = from go in selectedGameObjects
			where !go.transform.IsChildOf(m_LODGroup.transform)
			select go;

		// Handle reparenting
		var validChildren = new List<GameObject>();
		if (nonChildObjects.Count() > 0)
		{
			const string kReparent = "Some objects are not children of the LODGroup GameObject. Do you want to reparent them and add them to the LODGroup?";
			if (EditorUtility.DisplayDialog(
				"Reparent GameObjects",
				kReparent,
				"Yes, Reparent",
				"No, Use Only Existing Children"))
			{
				foreach (var go in nonChildObjects)
				{
					if (EditorUtility.IsPersistent(go))
					{
						var newGo = Instantiate(go) as GameObject;
						if (newGo != null)
						{
							newGo.transform.parent = m_LODGroup.transform;
							newGo.transform.localPosition = Vector3.zero;
							newGo.transform.localRotation = Quaternion.identity;
							validChildren.Add(newGo);
						}
					}
					else
					{
						go.transform.parent = m_LODGroup.transform;
						validChildren.Add(go);
					}
				}
				validSearchObjects = validSearchObjects.Union(validChildren);
			}
		}

		//Get all the renderers
		var renderers = new List<ParticleSystemAlphaController>();
		foreach (var go in validSearchObjects)
		{
			if (searchChildren)
				renderers.AddRange(go.GetComponentsInChildren<ParticleSystemAlphaController>());
			else
				renderers.Add(go.GetComponent<ParticleSystemAlphaController>());
		}

		// Then try renderers
		var selectedRenderers = from go in DragAndDrop.objectReferences
			where go as ParticleSystemAlphaController != null
			select go as ParticleSystemAlphaController;

		renderers.AddRange(selectedRenderers);
		return renderers;
	}

	// Add the given renderers to the current LOD group
	private void AddGameObjectRenderers(IEnumerable<ParticleSystemAlphaController> toAdd, bool add)
	{
		var renderersProperty = serializedObject.FindProperty(string.Format(kRenderRootPath, activeLOD));

		if (!add)
			renderersProperty.ClearArray();

		// On add make a list of the old renderers (to check for dupes)
		var oldRenderers = new List<ParticleSystemAlphaController>();
		for (var i = 0; i < renderersProperty.arraySize; i++)
		{
			var lodRenderRef = renderersProperty.GetArrayElementAtIndex(i);
			var renderer = lodRenderRef.objectReferenceValue as ParticleSystemAlphaController;

			if (renderer == null)
				continue;

			oldRenderers.Add(renderer);
		}

		foreach (var renderer in toAdd)
		{
			// Ensure that we don't add the renderer if it already exists
			if (oldRenderers.Contains(renderer))
				continue;

			renderersProperty.arraySize += 1;
			renderersProperty.
				GetArrayElementAtIndex(renderersProperty.arraySize - 1).objectReferenceValue = renderer;

			// Stop readd
			oldRenderers.Add(renderer);
		}
		serializedObject.ApplyModifiedProperties();
		// m_LODGroup.RecalculateBounds();
	}






	// Callabck action for mouse context clicks on the LOD slider(right click ect)
	private class LODAction
	{
		private readonly float m_normalizedCameraDistance;
		private readonly List<ParticleSystemLODGroupGUI.LODInfo> m_LODs;
		private readonly Vector2 m_ClickedPosition;
		private readonly SerializedObject m_ObjectRef;
		private readonly SerializedProperty m_LODsProperty;

		public delegate void Callback();
		private readonly Callback m_Callback;

		public LODAction(List<ParticleSystemLODGroupGUI.LODInfo> lods, float normalizedCameraDistance, Vector2 clickedPosition, SerializedProperty propLODs, Callback callback)
		{
			m_LODs = lods;
			m_normalizedCameraDistance = normalizedCameraDistance;
			m_ClickedPosition = clickedPosition;
			m_LODsProperty = propLODs;
			m_ObjectRef = propLODs.serializedObject;
			m_Callback = callback;
		}

		public void InsertLOD()
		{
			if (!m_LODsProperty.isArray)
				return;

			// Find where to insert
			int insertIndex = -1;
            for (int i = m_LODs.Count - 1; i >= 0; i--)
			{
                ParticleSystemLODGroupGUI.LODInfo lod = m_LODs[i];
                if (m_normalizedCameraDistance < lod.normalizedCameraDistance)
				{
					insertIndex = lod.LODLevel;
					break;
				}
			}

			// Clicked in the culled area... duplicate last
			if (insertIndex < 0)
			{
				m_LODsProperty.InsertArrayElementAtIndex(m_LODs.Count);
				insertIndex = m_LODs.Count;
			}
			else
			{
				m_LODsProperty.InsertArrayElementAtIndex(insertIndex);
			}

			// Null out the copied renderers (we want the list to be empty)
			var renderers = m_ObjectRef.FindProperty(string.Format(kRenderRootPath, insertIndex));
			renderers.arraySize = 0;

			var newLOD = m_LODsProperty.GetArrayElementAtIndex(insertIndex);
			newLOD.FindPropertyRelative("normalizedTransitionDistance").floatValue = m_normalizedCameraDistance;
			if (m_Callback != null)
				m_Callback();

			m_ObjectRef.ApplyModifiedProperties();
		}

		public void DeleteLOD()
		{
			if (m_LODs.Count <= 0)
				return;

			// Check for range click
			foreach (var lod in m_LODs)
			{
				var numberOfRenderers = m_ObjectRef.FindProperty(string.Format(kRenderRootPath, lod.LODLevel)).arraySize;
				if (lod.m_RangePosition.Contains(m_ClickedPosition) && (numberOfRenderers == 0
																		|| EditorUtility.DisplayDialog("Delete LOD",
																			"Are you sure you wish to delete this LOD?",
																			"Yes",
																			"No")))
				{
					var lodData = m_ObjectRef.FindProperty(string.Format(kLODDataPath, lod.LODLevel));
					lodData.DeleteCommand();

					m_ObjectRef.ApplyModifiedProperties();
					if (m_Callback != null)
						m_Callback();
					break;
				}
			}
		}
	}

	private void DeletedLOD()
	{
		m_SelectedLOD--;
	}






	// Set the camera distance so that the current LOD group covers the desired distance
	private static void UpdateCamera(float distance, ParticleSystemLODGroup group)
	{
		var sceneView = SceneView.lastActiveSceneView;
		var sceneCamera = sceneView.camera;
		// // We need to do inverse of SceneView.cameraDistance:
		// // given the distance, need to figure out "size" to focus the scene view on.
		// float size;
		// if (sceneCamera.orthographic)
		// {
		// 	size = distance;
		// 	if (sceneCamera.aspect < 1.0)
		// 		size *= sceneCamera.aspect;
		// }
		// else
		// {
		// 	var fov = sceneCamera.fieldOfView;
		// 	size = distance * Mathf.Sin(fov * 0.5f * Mathf.Deg2Rad);
		// }
		// .LookAtDirect(worldReferencePoint, , size);

		// SceneView.lastActiveSceneView.pivot = ;
		// SceneView.lastActiveSceneView.
		// sceneView.LookAt(group.transform.position, Quaternion.Euler(90, 0, 0));
		// sceneCamera.transform.position = SceneView.lastActiveSceneView.pivot + sceneCamera.transform.rotation * (Vector3.back * distance);
		
		sceneView.LookAtDirect(group.transform.position, sceneCamera.transform.rotation, distance * Mathf.Sin(sceneView.camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
	}
	
	


	private void UpdateSelectedLODFromCamera(IEnumerable<ParticleSystemLODGroupGUI.LODInfo> lods, float normalizedCameraDistance)
	{
		foreach (var lod in lods)
		{
			if (normalizedCameraDistance > lod.normalizedCameraDistance)
			{
				m_SelectedLOD = lod.LODLevel;
				break;
			}
		}
	}

	private readonly int m_LODSliderId = "LODSliderIDHash".GetHashCode();
	private readonly int m_CameraSliderId = "LODCameraIDHash".GetHashCode();
	private void DrawLODLevelSlider(Rect sliderPosition, List<ParticleSystemLODGroupGUI.LODInfo> lods)
	{
		int sliderId = GUIUtility.GetControlID(m_LODSliderId, FocusType.Passive);
		int camerId = GUIUtility.GetControlID(m_CameraSliderId, FocusType.Passive);
		Event evt = Event.current;

		switch (evt.GetTypeForControl(sliderId))
		{
			case EventType.Repaint:
			{
				ParticleSystemLODGroupGUI.DrawLODSlider(sliderPosition, lods, activeLOD, maxDistance);
				break;
			}
			case EventType.MouseDown:
			{
				// Handle right click first
				if (evt.button == 1 && sliderPosition.Contains(evt.mousePosition))
				{
					var normalizedCameraDistance = ParticleSystemLODGroupGUI.GetNormalizedCameraDistance(evt.mousePosition, sliderPosition);
					var pm = new GenericMenu();
					if (lods.Count >= 8)
					{
						pm.AddDisabledItem(EditorGUIUtility.TrTextContent("Insert Before"));
					}
					else
					{
						pm.AddItem(EditorGUIUtility.TrTextContent("Insert Before"), false,
							new LODAction(lods, normalizedCameraDistance, evt.mousePosition, m_LODs, null).
							InsertLOD);
					}

					// Figure out if we clicked in the culled region
					var disabledRegion = true;
					if (lods.Count > 0 && lods[lods.Count - 1].normalizedCameraDistance > normalizedCameraDistance)
						disabledRegion = false;

					if (disabledRegion)
						pm.AddDisabledItem(EditorGUIUtility.TrTextContent("Delete"));
					else
						pm.AddItem(EditorGUIUtility.TrTextContent("Delete"), false,
							new LODAction(lods, normalizedCameraDistance, evt.mousePosition, m_LODs, DeletedLOD).
							DeleteLOD);
					pm.ShowAsContext();

					// Do selection
					bool selected = false;
					foreach (var lod in lods)
					{
						if (lod.m_RangePosition.Contains(evt.mousePosition))
						{
							m_SelectedLOD = lod.LODLevel;
							selected = true;
							break;
						}
					}

					if (!selected)
						m_SelectedLOD = -1;

					evt.Use();

					break;
				}

				// Slightly grow position on the x because edge buttons overflow by 5 pixels
				var barPosition = sliderPosition;
				barPosition.x -= 5;
				barPosition.width += 10;

				if (barPosition.Contains(evt.mousePosition))
				{
					evt.Use();
					GUIUtility.hotControl = sliderId;

					// Check for button click
					var clickedButton = false;

					// case:464019 have to re-sort the LOD array for these buttons to get the overlaps in the right order...
					var lodsLeft = lods.Where(lod => lod.normalizedCameraDistance <= 0.5f).OrderBy(x => x.LODLevel);
					var lodsRight = lods.Where(lod => lod.normalizedCameraDistance > 0.5f).OrderByDescending(x => x.LODLevel);

					var lodButtonOrder = new List<ParticleSystemLODGroupGUI.LODInfo>();
					lodButtonOrder.AddRange(lodsLeft);
					lodButtonOrder.AddRange(lodsRight);

					foreach (var lod in lodButtonOrder)
					{
						if (lod.m_ButtonPosition.Contains(evt.mousePosition))
						{
							m_SelectedLODSlider = lod.LODLevel;
							clickedButton = true;
							// Bias by 0.1% so that there is no skipping when sliding
							BeginLODDrag(lod.normalizedCameraDistance - 0.001f, m_LODGroup);
							break;
						}
					}

					if (!clickedButton)
					{
						// Check for range click
						foreach (var lod in lodButtonOrder)
						{
							if (lod.m_RangePosition.Contains(evt.mousePosition))
							{
								m_SelectedLODSlider = -1;
								m_SelectedLOD = lod.LODLevel;
								break;
							}
						}
					}
				}
				break;
			}

			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl == sliderId && m_SelectedLODSlider >= 0 && lods[m_SelectedLODSlider] != null)
				{
					evt.Use();

					var normalizedDistance = ParticleSystemLODGroupGUI.GetNormalizedCameraDistance(evt.mousePosition, sliderPosition);
					// Bias by 0.1% so that there is no skipping when sliding
					ParticleSystemLODGroupGUI.SetSelectedLODLevelPercentage(normalizedDistance - 0.001f, m_SelectedLODSlider, lods);
					var percentageProperty = serializedObject.FindProperty(string.Format(kPixelHeightDataPath, lods[m_SelectedLODSlider].LODLevel));
					percentageProperty.floatValue = lods[m_SelectedLODSlider].normalizedCameraDistance;

					UpdateLODDrag(normalizedDistance, m_LODGroup);
				}
				break;
			}

			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl == sliderId)
				{
					GUIUtility.hotControl = 0;
					m_SelectedLODSlider = -1;
					EndLODDrag();
					evt.Use();
				}
				break;
			}

			case EventType.DragUpdated:
			case EventType.DragPerform:
			{
				// -2 = invalid region
				// -1 = culledregion
				// rest = LOD level
				var lodLevel = -2;
				// Is the mouse over a valid LOD level range?
				foreach (var lod in lods)
				{
					if (lod.m_RangePosition.Contains(evt.mousePosition))
					{
						lodLevel = lod.LODLevel;
						break;
					}
				}

				if (lodLevel == -2)
				{
					var culledRange = ParticleSystemLODGroupGUI.GetCulledBox(sliderPosition, lods.Count > 0 ? lods[lods.Count - 1].normalizedCameraDistance : 1.0f);
					if (culledRange.Contains(evt.mousePosition))
					{
						lodLevel = -1;
					}
				}

				if (lodLevel >= -1)
				{
					// Actually set LOD level now
					m_SelectedLOD = lodLevel;

					if (DragAndDrop.objectReferences.Length > 0)
					{
						DragAndDrop.visualMode = m_IsPrefab ? DragAndDropVisualMode.None : DragAndDropVisualMode.Copy;

						if (evt.type == EventType.DragPerform)
						{
							// First try gameobjects...
							var selectedGameObjects = from go in DragAndDrop.objectReferences
								where go as GameObject != null
								select go as GameObject;
							var renderers = GetRenderers(selectedGameObjects, true);

							if (lodLevel == -1)
							{
								m_LODs.arraySize++;
								var pixelHeightNew = serializedObject.FindProperty(string.Format(kPixelHeightDataPath, lods.Count));

								if (lods.Count == 0)
									pixelHeightNew.floatValue = 0.5f;
								else
								{
									var pixelHeightPrevious = serializedObject.FindProperty(string.Format(kPixelHeightDataPath, lods.Count - 1));
									pixelHeightNew.floatValue = pixelHeightPrevious.floatValue / 2.0f;
								}

								m_SelectedLOD = lods.Count;
								AddGameObjectRenderers(renderers, false);
							}
							else
							{
								AddGameObjectRenderers(renderers, true);
							}
							DragAndDrop.AcceptDrag();
						}
					}
					evt.Use();
				}

				break;
			}
			case EventType.DragExited:
			{
				evt.Use();
				break;
			}
		}
		if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null && !m_IsPrefab)
		{
			var camera = SceneView.lastActiveSceneView.camera;

			var currentCameraDistance = Vector3.Distance(m_LODGroup.transform.position, camera.transform.position);
			var currentCameraDistanceFraction = Mathf.Clamp01(currentCameraDistance/maxDistance);

			var cameraRect = ParticleSystemLODGroupGUI.CalcLODButton(sliderPosition, currentCameraDistanceFraction);
			var cameraIconRect = new Rect(cameraRect.center.x - 15, cameraRect.y - 25, 32, 32);
			var cameraLineRect = new Rect(cameraRect.center.x - 1, cameraRect.y, 2, cameraRect.height);
			var normalizedCameraDistanceRect = new Rect(cameraIconRect.center.x - 5, cameraLineRect.yMax, 35, 20);

			switch (evt.GetTypeForControl(camerId))
			{
				case EventType.Repaint:
				{
					// Draw a marker to indicate the current scene camera distance
					var colorCache = GUI.backgroundColor;
					GUI.backgroundColor = new Color(colorCache.r, colorCache.g, colorCache.b, 0.8f);
					ParticleSystemLODGroupGUI.Styles.m_LODCameraLine.Draw(cameraLineRect, false, false, false, false);
					GUI.backgroundColor = colorCache;
					GUI.Label(cameraIconRect, ParticleSystemLODGroupGUI.Styles.m_CameraIcon, GUIStyle.none);
					ParticleSystemLODGroupGUI.Styles.m_LODSliderText.Draw(normalizedCameraDistanceRect, string.Format("{0:0}%", currentCameraDistanceFraction * 100.0f), false, false, false, false);
					break;
				}
				case EventType.MouseDown:
				{
					if (cameraIconRect.Contains(evt.mousePosition))
					{
						evt.Use();
						var normalizedCameraDistance = ParticleSystemLODGroupGUI.GetNormalizedCameraDistance(evt.mousePosition, sliderPosition);

						// Update the selected LOD to be where the camera is if we click the camera
						UpdateSelectedLODFromCamera(lods, normalizedCameraDistance);
						GUIUtility.hotControl = camerId;

						BeginLODDrag(normalizedCameraDistance, m_LODGroup);
					}
					break;
				}
				case EventType.MouseDrag:
				{
					if (GUIUtility.hotControl == camerId)
					{
						evt.Use();
						var normalizedCameraDistance = ParticleSystemLODGroupGUI.GetNormalizedCameraDistance(evt.mousePosition, sliderPosition);

						// Change the active LOD level if the camera moves into a new LOD level
						UpdateSelectedLODFromCamera(lods, normalizedCameraDistance);
						UpdateLODDrag(normalizedCameraDistance, m_LODGroup);
					}
					break;
				}
				case EventType.MouseUp:
				{
					if (GUIUtility.hotControl == camerId)
					{
						EndLODDrag();
						GUIUtility.hotControl = 0;
						evt.Use();
					}
					break;
				}
			}
		}
	}



	private void BeginLODDrag(float desiredNormalizedDistance, ParticleSystemLODGroup group)
	{
		if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null || m_IsPrefab)
			return;

		UpdateCamera(desiredNormalizedDistance * maxDistance, group);
		HierarchyProperty.FilterSingleSceneObject(group.gameObject.GetInstanceID(), false);
		SceneView.RepaintAll();
	}

	private void UpdateLODDrag(float desiredNormalizedDistance, ParticleSystemLODGroup group)
	{
		if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null || m_IsPrefab)
			return;

		UpdateCamera(desiredNormalizedDistance * maxDistance, group);
		SceneView.RepaintAll();
	}

	private void EndLODDrag()
	{
		if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null || m_IsPrefab)
			return;
	}
}