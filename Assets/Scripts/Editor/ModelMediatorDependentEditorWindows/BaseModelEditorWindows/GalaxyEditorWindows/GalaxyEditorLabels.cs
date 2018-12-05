using System;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GalaxyEditorWindow
	{
		static class LabelsConstants
		{
			public const int SelectedLabelCurveSampling = 10;
			public const int AllLabelsCurveSamplingQuadrant = 5;
			public const int AllLabelsCurveSamplingGalactic = 15;
		}

		enum LabelStates
		{
			Unknown = 0,
			Idle = 10,
			SelectingBegin = 20,
			SelectingEnd = 30,
			UpdatingBegin = 40,
			UpdatingEnd = 50
		}

		enum LabelColorCodes
		{
			Unknown = 0,
			Label = 10,
			Group = 20
		}

		EditorPrefsInt labelsPreviewSize;
		EditorPrefsBool labelsPreviewMinimized;
		EditorPrefsString labelsSelectedLabelId;
		EditorPrefsInt labelsSelectedUniverseScale;
		EditorPrefsFloat labelsListScroll;
		EditorPrefsFloat labelsDetailsScroll;
		EditorPrefsEnum<LabelColorCodes> labelsColorCodeBy;

		LabelStates labelsLabelState = LabelStates.Idle;
		GalaxyLabelModel labelsLastSelectedLabel;
		GalaxyLabelModel labelsSelectedLabel;
		bool labelsIsOverAnAllLabel;

		void LabelsConstruct()
		{
			var currPrefix = KeyPrefix + "Labels";

			labelsPreviewSize = new EditorPrefsInt(currPrefix + "PreviewSize");
			labelsPreviewMinimized = new EditorPrefsBool(currPrefix + "PreviewMinimized");
			labelsSelectedLabelId = new EditorPrefsString(currPrefix + "SelectedLabelId");
			labelsSelectedUniverseScale = new EditorPrefsInt(currPrefix + "SelectedUniverseScale");
			labelsListScroll = new EditorPrefsFloat(currPrefix + "ListScroll");
			labelsDetailsScroll = new EditorPrefsFloat(currPrefix + "DetailsScroll");
			labelsColorCodeBy = new EditorPrefsEnum<LabelColorCodes>(currPrefix + "ColorCodeBy");

			RegisterToolbar("Labels", LabelsToolbar);

			BeforeLoadSelection += LabelsBeforeLoadSelection;
		}

		void LabelsBeforeLoadSelection()
		{
			SelectLabel(null);
		}

		void LabelsToolbar(GalaxyInfoModel model)
		{
			var selectedScale = UniverseScales.Quadrant;

			if (HorizontalPreviewSupported())
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical();
					{
						selectedScale = LabelsToolbarPrimary(model, true);
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
					{
						LabelsToolbarSecondary(model, selectedScale);
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				selectedScale = LabelsToolbarPrimary(model, false);
				GUILayout.FlexibleSpace();
				LabelsToolbarSecondary(model, selectedScale);
			}
		}

		UniverseScales LabelsToolbarPrimary(GalaxyInfoModel model, bool vertical)
		{
			if (string.IsNullOrEmpty(labelsSelectedLabelId.Value))
			{
				if (labelsSelectedLabel != null)
				{
					SelectLabel(null);
				}
			}
			else if (labelsSelectedLabel == null)
			{
				SelectLabel(model.GetLabel(labelsSelectedLabelId.Value));
			}

			var selectedScale = UniverseScales.Quadrant;

			if (vertical)
			{
				selectedScale = LabelsToolbarPrimaryList(model, 200f);
				LabelsToolbarPrimaryDetails(model);
			}
			else
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(EditorStyles.helpBox);
					{
						selectedScale = LabelsToolbarPrimaryList(model);
					}
					GUILayout.EndVertical();
					
					LabelsToolbarPrimaryDetails(model);
				}
				GUILayout.EndHorizontal();
			}


			return selectedScale;
		}

		UniverseScales LabelsToolbarPrimaryList(GalaxyInfoModel model, float? height = null)
		{
			UniverseScales[] scales =
			{
				UniverseScales.Quadrant,
				UniverseScales.Galactic
			};

			var scaleNames = scales.Select(s => s.ToString()).ToArray();
			var selectedScale = scales[labelsSelectedUniverseScale.Value];

			if (labelsSelectedLabel != null)
			{
				if (labelsSelectedLabel.Scale.Value != selectedScale)
				{
					SelectLabel(null);
				}
			}

			labelsSelectedUniverseScale.Value = GUILayout.Toolbar(Mathf.Min(labelsSelectedUniverseScale, scaleNames.Length - 1), scaleNames);

			EditorGUILayoutExtensions.PushEnabled(labelsSelectedLabel == null);
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Color Code By");
					labelsColorCodeBy.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Color Coding -", labelsColorCodeBy.Value);
					if (labelsColorCodeBy.Value == LabelColorCodes.Unknown) labelsColorCodeBy.Value = LabelColorCodes.Group;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayoutExtensions.PopEnabled();

			selectedScale = scales[labelsSelectedUniverseScale.Value];

			var scrollOptions = height.HasValue ? new GUILayoutOption[] { GUILayout.Height(height.Value) } : new GUILayoutOption[] { GUILayout.ExpandHeight(true) };

			labelsListScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, labelsListScroll.Value), false, true, scrollOptions).y;
			{
				var labelIndex = -1;
				foreach (var label in model.GetLabels(selectedScale))
				{
					labelIndex++;
					
					var isSelectedLabel = label.LabelId.Value == labelsSelectedLabelId.Value;
					if (isSelectedLabel) EditorGUILayoutExtensions.PushColor(Color.blue.NewS(0.7f));
					GUILayout.BeginVertical(EditorStyles.helpBox);
					if (isSelectedLabel) EditorGUILayoutExtensions.PopColor();
					{
						GUILayout.BeginHorizontal();
						{
							EditorGUILayoutExtensions.PushColor(EditorUtilityExtensions.ColorFromIndex(labelIndex));
							GUILayout.BeginHorizontal(EditorStyles.helpBox);
							EditorGUILayoutExtensions.PopColor();
							{
								GUILayout.Label(label.Name.Value, EditorStyles.boldLabel);
							}
							GUILayout.EndHorizontal();
							
							EditorGUILayoutExtensions.PushEnabled(!isSelectedLabel);
							if (GUILayout.Button("Edit Label", GUILayout.Width(100f)))
							{
								SelectLabel(label);
							}
							EditorGUILayoutExtensions.PopEnabled();
							
							if (EditorGUILayoutExtensions.XButton())
							{
								if (labelsSelectedLabel == label)
								{
									SelectLabel(null);
								}
								model.RemoveLabel(label);
								ModelSelectionModified = true;
							}
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Group Id");
							EditorGUILayoutExtensions.PushBackgroundColor(EditorUtilityExtensions.ColorFromString(label.GroupId.Value));
							var groupValue = label.GroupId.Value ?? "< No Group Id >";
							if (16 < groupValue.Length) groupValue = groupValue.Substring(0, 16) + "...";
							//if (GUILayout.Button(groupValue))
							if (GUILayout.Button(new GUIContent(groupValue, "Copy group id"), GUILayout.Width(124f)))
							{
								EditorGUIUtility.systemCopyBuffer = label.GroupId.Value;
								ShowNotification(new GUIContent("Copied Group Id to Clipboard"));
							}
							EditorGUILayoutExtensions.PopBackgroundColor();
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				
			}
			GUILayout.EndScrollView();
			
			GUILayout.BeginHorizontal();
			{
				switch (labelsLabelState)
				{
					case LabelStates.Idle:
						EditorGUILayout.HelpBox("Click on preview to begin defining a new label.", MessageType.Info);
						break;
					case LabelStates.SelectingBegin:
						EditorGUILayout.HelpBox("Click on preview to select where to begin.", MessageType.Info);
						break;
					case LabelStates.SelectingEnd:
						EditorGUILayout.HelpBox("Click on preview to select where to end.", MessageType.Info);
						break;
					case LabelStates.UpdatingBegin:
						EditorGUILayout.HelpBox("Click on preview to select a new begin.", MessageType.Info);
						break;
					case LabelStates.UpdatingEnd:
						EditorGUILayout.HelpBox("Click on preview to select a new end.", MessageType.Info);
						break;
				}
				
				GUILayout.BeginVertical(GUILayout.Width(72f));
				{
					EditorGUILayoutExtensions.PushEnabled(labelsLabelState == LabelStates.Idle && labelsSelectedLabel != null);
					if (GUILayout.Button("Deselect"))
					{
						SelectLabel(null);
					}
					EditorGUILayoutExtensions.PopEnabled();
					
					EditorGUILayoutExtensions.PushEnabled(labelsLabelState != LabelStates.Idle);
					EditorGUILayoutExtensions.PushColor(Color.red);
					if (GUILayout.Button("Cancel"))
					{
						switch (labelsLabelState)
						{
							case LabelStates.SelectingBegin:
							case LabelStates.SelectingEnd:
								SelectLabel(labelsLastSelectedLabel);
								break;
							case LabelStates.UpdatingBegin:
							case LabelStates.UpdatingEnd:
								SelectLabel(labelsSelectedLabel);
								break;
						}
						labelsLabelState = LabelStates.Idle;
					}
					EditorGUILayoutExtensions.PopColor();
					EditorGUILayoutExtensions.PopEnabled();

				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			return selectedScale;
		}

		void LabelsToolbarPrimaryDetails(GalaxyInfoModel model)
		{
			labelsDetailsScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, labelsDetailsScroll.Value), false, true, GUILayout.ExpandHeight(true)).y;
			{
				if (labelsSelectedLabel == null)
				{
					EditorGUILayout.HelpBox("Select a label to edit it.", MessageType.Info);
					GUILayout.FlexibleSpace();
				}
				else
				{
					EditorGUILayoutExtensions.PushEnabled(labelsLabelState == LabelStates.Idle);
					{
						EditorGUIExtensions.BeginChangeCheck();
						{
							GUILayout.BeginHorizontal();
							{
								labelsSelectedLabel.Name.Value = EditorGUILayout.TextField("Name", labelsSelectedLabel.Name.Value);
								labelsSelectedLabel.LabelId.Value = EditorGUILayout.TextField(labelsSelectedLabel.LabelId.Value, GUILayout.Width(128f));
								labelsSelectedLabel.Source.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select a Source -", labelsSelectedLabel.Source.Value, GUILayout.Width(90f));
							}
							GUILayout.EndHorizontal();

							labelsSelectedLabel.GroupId.Value = EditorGUILayout.TextField("Group Id", labelsSelectedLabel.GroupId.Value);

							switch (labelsSelectedLabel.Source.Value)
							{
								case GalaxyLabelSources.Static:
									labelsSelectedLabel.StaticText.Key.Value = EditorGUILayout.TextField("Static Key", labelsSelectedLabel.StaticText.Key.Value);
									EditorGUILayoutExtensions.PushEnabled(false);
									{
										EditorGUILayout.TextField(new GUIContent("Value", "Edit the name until proper language support is added"), labelsSelectedLabel.StaticText.Value.Value);
									}
									EditorGUILayoutExtensions.PopEnabled();
									break;
								case GalaxyLabelSources.GameKeyValue:
									labelsSelectedLabel.SourceKey.Value = EditorGUILayout.TextField("Source Key", labelsSelectedLabel.SourceKey.Value);
									break;
								default:
									EditorGUILayout.HelpBox("Unrecognized source " + labelsSelectedLabel.Source.Value, MessageType.Error);
									break;
							}

							EditorGUILayoutValueFilter.Field("Filtering", labelsSelectedLabel.Filtering);

							var labelCurve = labelsSelectedLabel.CurveInfo.Value;

							labelCurve.LabelStyle = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select a Style -", labelCurve.LabelStyle);
							labelCurve.FontSize = EditorGUILayout.FloatField("Font Size", labelCurve.FontSize);
							labelCurve.Curve = EditorGUILayoutAnimationCurve.Field("Curve", labelCurve.Curve);
							labelCurve.CurveMaximum = EditorGUILayout.FloatField("Curve Maximum", labelCurve.CurveMaximum);
							labelCurve.FlipAnchors = EditorGUILayout.Toggle("Flip Anchors", labelCurve.FlipAnchors);
							labelCurve.FlipCurve = EditorGUILayout.Toggle("Flip Curve", labelCurve.FlipCurve);

							labelsSelectedLabel.CurveInfo.Value = labelCurve;

							GUILayout.BeginHorizontal();
							{
								EditorGUILayout.PrefixLabel("Slice Layer");

								var sliceColors = new Color[] { Color.black, Color.red, Color.yellow, Color.white };

								for (var t = 0; t < 4; t++)
								{
									var isCurrSlice = t == labelsSelectedLabel.SliceLayer.Value;
									EditorGUILayoutExtensions.PushBackgroundColor(sliceColors[t]);
									if (GUILayout.Button(new GUIContent(isCurrSlice ? "Selected" : string.Empty), GUILayout.MaxWidth(100f))) labelsSelectedLabel.SliceLayer.Value = t;
									EditorGUILayoutExtensions.PopBackgroundColor();
								}
							}
							GUILayout.EndHorizontal();
							//selectedLabel.SliceLayer.Value = Mathf.Max(0, EditorGUILayout.IntField("Slice Layer", selectedLabel.SliceLayer.Value));

							GUILayout.BeginHorizontal(EditorStyles.helpBox);
							{
								GUILayout.BeginVertical();
								{
									labelsSelectedLabel.BeginAnchorNormal.Value = EditorGUILayout.Vector3Field("Begin Anchor", labelsSelectedLabel.BeginAnchorNormal);
								}
								GUILayout.EndVertical();
								EditorGUILayoutExtensions.PushBackgroundColor(Color.cyan);
								if (GUILayout.Button("Update Begin", GUILayout.Width(100f), GUILayout.Height(51f)))
								{
									labelsLabelState = LabelStates.UpdatingBegin;
								}
								EditorGUILayoutExtensions.PopBackgroundColor();
							}
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal(EditorStyles.helpBox);
							{
								GUILayout.BeginVertical();
								{
									labelsSelectedLabel.EndAnchorNormal.Value = EditorGUILayout.Vector3Field("EndAnchor", labelsSelectedLabel.EndAnchorNormal);
								}
								GUILayout.EndVertical();
								EditorGUILayoutExtensions.PushBackgroundColor(Color.magenta);
								if (GUILayout.Button("Update End", GUILayout.Width(100f), GUILayout.Height(51f)))
								{
									labelsLabelState = LabelStates.UpdatingEnd;
								}
								EditorGUILayoutExtensions.PopBackgroundColor();
							}
							GUILayout.EndHorizontal();
						}
						EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
			}
			GUILayout.EndScrollView();
		}

		void LabelsToolbarSecondary(GalaxyInfoModel model, UniverseScales selectedScale)
		{
			var selectedPreviewIndex = 0;

			switch (selectedScale)
			{
				case UniverseScales.Quadrant:
					selectedPreviewIndex = PreviewConstants.DetailsIndex;
					break;
				case UniverseScales.Galactic:
					selectedPreviewIndex = PreviewConstants.FullPreviewIndex;
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized scale " + selectedScale, MessageType.Error);
					break;
			}

			DrawPreviews(
				model,
				null,
				labelsPreviewSize,
				labelsPreviewMinimized,
				!labelsIsOverAnAllLabel,
				clickPosition => LabelsPrimaryClickPreview(model, clickPosition, selectedScale),
				clickPosition => LabelsSecondaryClickPreview(model, clickPosition, selectedScale),
				drawOnPreview: displayArea => LabelsDrawOnPreview(model, selectedScale, displayArea),
				previewSelectedOverride: selectedPreviewIndex
			);
		}

		void LabelsDrawOnPreview(GalaxyInfoModel model, UniverseScales selectedScale, Rect displayArea)
		{
			labelsIsOverAnAllLabel = false;
			if (labelsSelectedLabel == null)
			{
				LabelsShowAllLabels(model, selectedScale, displayArea);
				return;
			}

			var beginInPreview = true;
			var endInPreview = true;

			var beginAnchorInWindow = NormalToWindow(labelsSelectedLabel.BeginAnchorNormal.Value, displayArea, out beginInPreview);
			var endAnchorInWindow = NormalToWindow(labelsSelectedLabel.EndAnchorNormal.Value, displayArea, out endInPreview);

			var beginColor = labelsLabelState == LabelStates.UpdatingBegin ? Color.cyan.NewS(0.25f) : Color.cyan;

			EditorGUILayoutExtensions.PushColor(beginInPreview ? beginColor : beginColor.NewA(beginColor.a * 0.5f));
			{
				GUI.Box(CenteredScreen(beginAnchorInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Anchor Begin"), SubLightEditorConfig.Instance.LabelAnchorStyle);
			}
			EditorGUILayoutExtensions.PopColor();

			switch (labelsLabelState)
			{
				case LabelStates.Idle:
				case LabelStates.UpdatingBegin:
				case LabelStates.UpdatingEnd:
					var endColor = labelsLabelState == LabelStates.UpdatingEnd ? Color.magenta.NewS(0.25f) : Color.magenta;
					EditorGUILayoutExtensions.PushColor(endInPreview ? endColor : endColor.NewA(endColor.a * 0.5f));
					{
						GUI.Box(CenteredScreen(endAnchorInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Anchor End"), SubLightEditorConfig.Instance.LabelAnchorStyle);
					}
					EditorGUILayoutExtensions.PopColor();
					break;
			}

			if (labelsLabelState != LabelStates.Idle) return;

			var previewCurveInfo = labelsSelectedLabel.CurveInfo.Value; // We modify this to make changes for rendering to the screen...
			previewCurveInfo.FlipCurve = !previewCurveInfo.FlipCurve;

			var currBegin = new Vector3(beginAnchorInWindow.x, 0f, beginAnchorInWindow.y);
			var currEnd = new Vector3(endAnchorInWindow.x, 0f, endAnchorInWindow.y);

			for (var i = 0; i < LabelsConstants.SelectedLabelCurveSampling; i++)
			{
				var curvePos = previewCurveInfo.Evaluate(currBegin, currEnd, i / (LabelsConstants.SelectedLabelCurveSampling - 1f), false);
				var curvePosInWindow = new Vector2(curvePos.x, curvePos.z);

				EditorGUILayoutExtensions.PushColor(Color.yellow);
				{
					GUI.Box(CenteredScreen(curvePosInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Position on Curve"), SubLightEditorConfig.Instance.LabelCurvePointStyle);
				}
				EditorGUILayoutExtensions.PopColor();
			}
		}

		void LabelsShowAllLabels(
			GalaxyInfoModel model,
			UniverseScales scale,
			Rect displayArea
		)
		{
			var labels = model.GetLabels(scale);
			var sampling = scale == UniverseScales.Quadrant ? LabelsConstants.AllLabelsCurveSamplingQuadrant : LabelsConstants.AllLabelsCurveSamplingGalactic;
			var labelIndex = -1;

			foreach (var label in labels)
			{
				labelIndex++;

				var color = labelsColorCodeBy.Value == LabelColorCodes.Label ? EditorUtilityExtensions.ColorFromIndex(labelIndex) : EditorUtilityExtensions.ColorFromString(label.GroupId.Value);

				EditorGUILayoutExtensions.PushColor(color);

				var beginInPreview = true;
				var endInPreview = true;

				var beginAnchorInWindow = NormalToWindow(label.BeginAnchorNormal.Value, displayArea, out beginInPreview);
				var endAnchorInWindow = NormalToWindow(label.EndAnchorNormal.Value, displayArea, out endInPreview);

				var previewCurveInfo = label.CurveInfo.Value; // We modify this to make changes for rendering to the screen...
				previewCurveInfo.FlipCurve = !previewCurveInfo.FlipCurve;

				var currBegin = new Vector3(beginAnchorInWindow.x, 0f, beginAnchorInWindow.y);
				var currEnd = new Vector3(endAnchorInWindow.x, 0f, endAnchorInWindow.y);

				for (var i = 0; i < sampling; i++)
				{
					var progress = i / (sampling - 1f);
					var curvePos = previewCurveInfo.Evaluate(currBegin, currEnd, progress, false);
					var curvePosInWindow = new Vector2(curvePos.x, curvePos.z);

					GUI.Box(CenteredScreen(curvePosInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Position on Curve"), SubLightEditorConfig.Instance.LabelCurvePointStyle);
				}

				var centerPos = previewCurveInfo.Evaluate(currBegin, currEnd, 0.5f, false);
				var centerPosInWindow = new Vector2(centerPos.x, centerPos.z);
				var selectCurrentArea = CenteredScreen(centerPosInWindow, new Vector2(16f, 16f));

				if (GUI.Button(selectCurrentArea, new GUIContent(string.Empty, label.Name.Value), SubLightEditorConfig.Instance.LabelCurveCenterStyle))
				{
					SelectLabel(label);
				}

				labelsIsOverAnAllLabel = labelsIsOverAnAllLabel || selectCurrentArea.Contains(Event.current.mousePosition);

				EditorGUILayoutExtensions.PopColor();
			}
		}

		void LabelsPrimaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition, UniverseScales scale)
		{
			switch (labelsLabelState)
			{
				case LabelStates.Idle:
					labelsLastSelectedLabel = labelsSelectedLabel;
					labelsSelectedLabel = LabelsCreateNewLabel(scale);
					labelsSelectedLabelId.Value = labelsSelectedLabel.LabelId.Value;
					labelsSelectedLabel.BeginAnchorNormal.Value = clickPosition;

					labelsLabelState = LabelStates.SelectingBegin;

					TextDialogPopup.Show(
						"New Label",
						value =>
						{
							labelsLabelState = LabelStates.SelectingEnd;
							labelsSelectedLabel.Name.Value = value;
						},
						() =>
						{
							SelectLabel(labelsLastSelectedLabel);
						}
					);
					break;
				case LabelStates.SelectingBegin:
					labelsSelectedLabel.BeginAnchorNormal.Value = clickPosition;
					labelsLabelState = LabelStates.SelectingEnd;
					break;
				case LabelStates.SelectingEnd:
					labelsSelectedLabel.EndAnchorNormal.Value = clickPosition;
					model.AddLabel(labelsSelectedLabel);
					ModelSelectionModified = true;
					labelsLabelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingBegin:
					labelsSelectedLabel.BeginAnchorNormal.Value = clickPosition;
					ModelSelectionModified = true;
					labelsLabelState = LabelStates.Idle;
					break;
				case LabelStates.UpdatingEnd:
					labelsSelectedLabel.EndAnchorNormal.Value = clickPosition;
					ModelSelectionModified = true;
					labelsLabelState = LabelStates.Idle;
					break;
				default:
					Debug.LogError("Unrecognized state " + labelsLabelState);
					break;
			}
		}

		void LabelsSecondaryClickPreview(GalaxyInfoModel model, Vector3 clickPosition, UniverseScales scale)
		{
			switch (labelsLabelState)
			{
				case LabelStates.Idle:
					SelectLabel(null);
					return;
				case LabelStates.SelectingBegin:
				case LabelStates.SelectingEnd:
					SelectLabel(labelsLastSelectedLabel);
					break;
				case LabelStates.UpdatingBegin:
				case LabelStates.UpdatingEnd:
					SelectLabel(labelsSelectedLabel);
					break;
			}
			labelsLabelState = LabelStates.Idle;
		}

		GalaxyLabelModel LabelsCreateNewLabel(UniverseScales scale, string groupId = null)
		{
			var result = new GalaxyLabelModel();
			result.LabelId.Value = Guid.NewGuid().ToString();
			result.GroupId.Value = string.IsNullOrEmpty(groupId) ? Guid.NewGuid().ToString() : groupId;
			result.Scale.Value = scale;

			return result;
		}
	}
}