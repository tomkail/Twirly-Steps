// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

internal static class ParticleSystemLODGroupGUI
{
	// Default colors for each LOD group....
	public static readonly Color[] kLODColors =
	{
		new Color(0.4831376f, 0.6211768f, 0.0219608f, 1.0f),
		new Color(0.2792160f, 0.4078432f, 0.5835296f, 1.0f),
		new Color(0.2070592f, 0.5333336f, 0.6556864f, 1.0f),
		new Color(0.5333336f, 0.1600000f, 0.0282352f, 1.0f),
		new Color(0.3827448f, 0.2886272f, 0.5239216f, 1.0f),
		new Color(0.8000000f, 0.4423528f, 0.0000000f, 1.0f),
		new Color(0.4486272f, 0.4078432f, 0.0501960f, 1.0f),
		new Color(0.7749016f, 0.6368624f, 0.0250984f, 1.0f)
	};

	public static readonly Color kCulledLODColor = new Color(.4f, 0f, 0f, 1f);

	public const int kSceneLabelHalfWidth = 100;
	public const int kSceneLabelHeight = 45;
	public const int kSceneHeaderOffset = 40;

	public const int kSliderBarTopMargin = 18;
	public const int kSliderBarHeight = 30;
	public const int kSliderBarBottomMargin = 16;

	public const int kRenderersButtonHeight = 60;
	public const int kButtonPadding = 2;
	public const int kDeleteButtonSize = 20;

	public const int kSelectedLODRangePadding = 3;

	public const int kRenderAreaForegroundPadding = 3;

	public class GUIStyles
	{
		public readonly GUIStyle m_LODSliderBG = "LODSliderBG";
		public readonly GUIStyle m_LODSliderRange = "LODSliderRange";
		public readonly GUIStyle m_LODSliderRangeSelected = "LODSliderRangeSelected";
		public readonly GUIStyle m_LODSliderText = "LODSliderText";
		public readonly GUIStyle m_LODSliderTextSelected = "LODSliderTextSelected";
		public readonly GUIStyle m_LODStandardButton = "Button";
		public readonly GUIStyle m_LODRendererButton = "LODRendererButton";
		public readonly GUIStyle m_LODRendererAddButton = "LODRendererAddButton";
		public readonly GUIStyle m_LODRendererRemove = "LODRendererRemove";
		public readonly GUIStyle m_LODBlackBox = "LODBlackBox";
		public readonly GUIStyle m_LODCameraLine = "LODCameraLine";

		public readonly GUIStyle m_LODSceneText = "LODSceneText";
		public readonly GUIStyle m_LODRenderersText = "LODRenderersText";
		public readonly GUIStyle m_LODLevelNotifyText = "LODLevelNotifyText";

		public readonly GUIContent m_IconRendererPlus                   = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add New Renderers");
		public readonly GUIContent m_IconRendererMinus                  = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove Renderer");
		public readonly GUIContent m_CameraIcon                         = EditorGUIUtility.IconContent("d_Camera Icon");

		public readonly GUIContent m_RendersTitle                       = EditorGUIUtility.TrTextContent("Renderers:");
	}

	private static GUIStyles s_Styles;

	public static GUIStyles Styles
	{
		get
		{
			if (s_Styles == null)
				s_Styles = new GUIStyles();
			return s_Styles;
		}
	}

	public static Rect CalcLODButton(Rect totalRect, float normalizedDistance)
	{
		return new Rect(totalRect.x + (Mathf.Round(totalRect.width * (normalizedDistance))) - 5, totalRect.y, 10, totalRect.height);
	}

	public static Rect GetCulledBox(Rect totalRect, float previousNormalizedDistance)
	{
		var r = CalcLODRange(totalRect, previousNormalizedDistance, 1.0f);
		r.height -= 2;
		r.width -= 1;
		r.center += new Vector2(0f, 1.0f);
		return r;
	}

	public class LODInfo
	{
		public Rect m_ButtonPosition;
		public Rect m_RangePosition;

		public LODInfo(int lodLevel, string name, float normalizedCameraDistance)
		{
			LODLevel = lodLevel;
			LODName = name;
			this.normalizedCameraDistance = normalizedCameraDistance;
		}

		public int LODLevel { get; private set; }
		public string LODName { get; private set; }
		public float normalizedCameraDistance { get; set; }
	}

	public static List<LODInfo> CreateLODInfos(int numLODs, Rect area, Func<int, string> nameGen, Func<int, float> distanceGen)
	{
		var lods = new List<LODInfo>();

		for (int i = 0; i < numLODs; ++i)
		{
			var lodInfo = new LODInfo(i, nameGen(i), distanceGen(i));
			lodInfo.m_ButtonPosition = CalcLODButton(area, lodInfo.normalizedCameraDistance);
			var previousNormalizedCameraDistance = i == 0 ? 0.0f : lods[i - 1].normalizedCameraDistance;
			lodInfo.m_RangePosition = CalcLODRange(area, previousNormalizedCameraDistance, lodInfo.normalizedCameraDistance);
			lods.Add(lodInfo);
		}

		return lods;
	}

	public static float GetNormalizedCameraDistance(Vector2 position, Rect sliderRect)
	{
		return Mathf.Clamp((position.x - sliderRect.x) / sliderRect.width, 0.005f, 1f);
	}

	public static void SetSelectedLODLevelPercentage(float newScreenPercentage, int lod, List<LODInfo> lods)
	{
		// Find the lower detail lod... clamp value to stop overlapping slider
		var minimum = 0.0f;
		var lowerLOD = lods.FirstOrDefault(x => x.LODLevel == lods[lod].LODLevel - 1);
		if (lowerLOD != null)
			minimum = lowerLOD.normalizedCameraDistance;

		// Find the higher detail lod... clamp value to stop overlapping slider
		var maximum = 1.0f;
		var higherLOD = lods.FirstOrDefault(x => x.LODLevel == lods[lod].LODLevel + 1);
		if (higherLOD != null)
			maximum = higherLOD.normalizedCameraDistance;

		maximum = Mathf.Clamp01(maximum);
		minimum = Mathf.Clamp01(minimum);

		// Set that value
		lods[lod].normalizedCameraDistance = Mathf.Clamp(newScreenPercentage, minimum, maximum);
	}

	public static void DrawLODSlider(Rect area, IList<LODInfo> lods, int selectedLevel, float maxDistance)
	{
		Styles.m_LODSliderBG.Draw(area, GUIContent.none, false, false, false, false);
		for (int i = lods.Count - 1; i >= 0; i--)
		{
			var lod = lods[i];
			DrawLODRange(lod, i == 0 ? 0.0f : lods[i - 1].normalizedCameraDistance, i == selectedLevel, maxDistance);
			DrawLODButton(lod);
		}

		// Draw the last range (culled)
		DrawCulledRange(area, lods.Count > 0 ? lods[lods.Count - 1].normalizedCameraDistance : 1.0f, maxDistance);
	}

	public static void DrawMixedValueLODSlider(Rect area)
	{
		Styles.m_LODSliderBG.Draw(area, GUIContent.none, false, false, false, false);
		var r = GetCulledBox(area, 1.0f);
		// Draw the range of a lod level on the slider
		var tempColor = GUI.color;
		GUI.color = kLODColors[1] * 0.6f; // more greyish
		Styles.m_LODSliderRange.Draw(r, GUIContent.none, false, false, false, false);
		GUI.color = tempColor;
		var centeredStyle = new GUIStyle(EditorStyles.whiteLargeLabel)
		{
			alignment = TextAnchor.MiddleCenter
		};
		GUI.Label(area, "---", centeredStyle);
	}

	private static Rect CalcLODRange(Rect totalRect, float startDistanceNormalized, float endDistanceNormalized)
	{
		var startX = Mathf.Round(totalRect.width * startDistanceNormalized);
		var endX = Mathf.Round(totalRect.width * endDistanceNormalized);

		return new Rect(totalRect.x + startX, totalRect.y, endX - startX, totalRect.height);
	}

	private static void DrawLODButton(LODInfo currentLOD)
	{
		// Make the lod button areas a horizonal resizer
		EditorGUIUtility.AddCursorRect(currentLOD.m_ButtonPosition, MouseCursor.ResizeHorizontal);
	}

	private static void DrawLODRange(LODInfo currentLOD, float previousNormalizedDistance, bool isSelected, float maxDistance)
	{
		var tempColor = GUI.backgroundColor;
		var startPercentageString = string.Format("{0}\n{1}m", currentLOD.LODName, previousNormalizedDistance * maxDistance);
		if (isSelected)
		{
			var foreground = currentLOD.m_RangePosition;
			foreground.width -= kSelectedLODRangePadding * 2;
			foreground.height -= kSelectedLODRangePadding * 2;
			foreground.center += new Vector2(kSelectedLODRangePadding, kSelectedLODRangePadding);
			Styles.m_LODSliderRangeSelected.Draw(currentLOD.m_RangePosition, GUIContent.none, false, false, false, false);
			GUI.backgroundColor = kLODColors[currentLOD.LODLevel];
			if (foreground.width > 0)
				Styles.m_LODSliderRange.Draw(foreground, GUIContent.none, false, false, false, false);
			Styles.m_LODSliderText.Draw(currentLOD.m_RangePosition, startPercentageString, false, false, false, false);
		}
		else
		{
			GUI.backgroundColor = kLODColors[currentLOD.LODLevel];
			GUI.backgroundColor *= 0.6f;
			Styles.m_LODSliderRange.Draw(currentLOD.m_RangePosition, GUIContent.none, false, false, false, false);
			Styles.m_LODSliderText.Draw(currentLOD.m_RangePosition, startPercentageString, false, false, false, false);
		}
		GUI.backgroundColor = tempColor;
	}

	private static void DrawCulledRange(Rect totalRect, float previousNormalizedDistance, float maxDistance)
	{
		if (Mathf.Approximately(previousNormalizedDistance, 0.0f)) return;

		var r = GetCulledBox(totalRect, previousNormalizedDistance);
		// Draw the range of a lod level on the slider
		var tempColor = GUI.color;
		GUI.color = kCulledLODColor;
		Styles.m_LODSliderRange.Draw(r, GUIContent.none, false, false, false, false);
		GUI.color = tempColor;

		// Draw some details for the current marker
		var startPercentageString = string.Format("Culled\n{0}m", previousNormalizedDistance * maxDistance);
		Styles.m_LODSliderText.Draw(r, startPercentageString, false, false, false, false);
	}
}