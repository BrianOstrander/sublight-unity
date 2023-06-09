﻿using System;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GalaxyEditorWindow : BaseModelEditorWindow<GalaxyEditorWindow, GalaxyInfoModel>
	{
		public static class PreviewConstants
		{
			public static string[] Names =
			{
				"BodyMap",
				"Details",
				"Full Preview"
			};

			public const int BodyMapIndex = 0;
			public const int DetailsIndex = 1;
			public const int FullPreviewIndex = 2;

			public const int MinimumSize = 128;
			public const int MaximumSize = 2048;

			public static Texture2D GetTexture(GalaxyInfoModel model, int index)
			{
				switch (index)
				{
					case 0:
						return model.BodyMap;
					case 1:
						return model.Details;
					case 2:
						return model.FullPreview;
					default:
						EditorGUILayout.HelpBox("Unrecognized index", MessageType.Error);
						return null;
				}
			}
		}

		[MenuItem("Window/SubLight/Galaxy Editor")]
		static void Initialize() { OnInitialize("Galaxy Editor"); }

		Rect lastPreviewRect = Rect.zero;

		public GalaxyEditorWindow() : base("LG_SL_GalaxyEditor_", "Galaxy")
		{
			RegisterToolbar(new GalaxyGeneralEditorTab(this));
			RegisterToolbar(new GalaxyTargetsEditorTab(this));

			var labelTab = new GalaxyLabelsEditorTab(this);
			
			RegisterToolbar(labelTab);
			RegisterToolbar(new GalaxySpecifiedSectorsEditorTab(this, labelTab));
			RegisterToolbar(new GalaxyGenerationEditorTab(this));
		}

		#region Model Overrides
		protected override void AssignModelName(GalaxyInfoModel model, string name)
		{
			model.Name.Value = name;
		}
		#endregion

		#region Utility
		Vector3 ScreenToNormal(Vector2 screenPosition, Rect preview)
		{
			preview = new Rect(GUIUtility.GUIToScreenPoint(lastPreviewRect.position), preview.size);

			var previewOffset = screenPosition - preview.min;
			return new Vector3(previewOffset.x / preview.width, 0f, 1f - (previewOffset.y / preview.height));
		}

		public Vector2 NormalToWindow(Vector3 normalPosition, Rect preview, out bool inPreview)
		{
			var result = preview.min + new Vector2(preview.width * normalPosition.x, preview.height * (1f - normalPosition.z));
			inPreview = preview.Contains(result);
			return new Vector2(Mathf.Clamp(result.x, preview.xMin, preview.xMax), Mathf.Clamp(result.y, preview.yMin, preview.yMax));
		}

		public Rect CenteredScreen(Vector2 screenPosition, Vector2 size)
		{
			return new Rect(screenPosition - (size * 0.5f), size);
		}

		Rect DrawClickableTexture(
			Texture2D texture,
			int size,
			Action<Vector3> primaryClick,
			Action<Vector3> secondaryClick = null,
			bool isClickable = true
		)
		{
			texture = texture ?? Texture2D.blackTexture;

			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				var previewStyle = new GUIStyle(GUIStyle.none);
				previewStyle.normal.background = texture;
				previewStyle.active.background = texture;
			
				if (isClickable)
				{
					if (GUILayout.Button(GUIContent.none, previewStyle, GUILayout.Width(size), GUILayout.Height(size)))
					{
						var universePosition = ScreenToNormal(
							GUIUtility.GUIToScreenPoint(Event.current.mousePosition), // You may be tempted to fix this by offseting it, but don't...
							lastPreviewRect
						);

						if (secondaryClick != null)
						{
							if (Event.current.button == 1) secondaryClick(universePosition);
							else if (primaryClick != null) primaryClick(universePosition);
						}
						else if (primaryClick != null) primaryClick(universePosition);
					}
				}
				else
				{
					GUILayout.Box(GUIContent.none, previewStyle, GUILayout.Width(size), GUILayout.Height(size));
				}

				if (Event.current.type == EventType.Repaint) lastPreviewRect = GUILayoutUtility.GetLastRect();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			return lastPreviewRect;
		}

		public void DrawPreviews(
			GalaxyInfoModel model,
			EditorPrefsInt previewSelected,
			EditorPrefsInt previewSize,
			EditorPrefsBool previewMinimized,
			bool previewClickable,
			Action<Vector3> primaryClick = null,
			Action<Vector3> secondaryClick = null,
			Action drawToolbarPrefix = null,
			Action drawToolbarSuffix = null,
			Action<Rect> drawOnPreview = null,
			int? previewSelectedOverride = null
		)
		{
			var previewSelectedIndex = previewSelectedOverride ?? 0;

			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			{
				if (previewMinimized.Value)
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label("Expand Preview");
				}
				else
				{
					if (drawToolbarPrefix != null) drawToolbarPrefix();

					if (previewSelectedOverride.HasValue || previewSelected == null) GUILayout.FlexibleSpace();
					else previewSelected.Value = (previewSelectedIndex = GUILayout.Toolbar(Mathf.Min(previewSelected, PreviewConstants.Names.Length - 1), PreviewConstants.Names));

					GUILayout.Label(new GUIContent("Preview Size"), GUILayout.Width(78f));
					previewSize.Value = Mathf.Clamp(EditorGUILayout.IntSlider(previewSize.Value, PreviewConstants.MinimumSize, PreviewConstants.MaximumSize, GUILayout.Width(150f)), PreviewConstants.MinimumSize, PreviewConstants.MaximumSize);

					if (drawToolbarSuffix != null) drawToolbarSuffix();
				}
				var previewText = previewMinimized.Value ? "v" : "^";
				if (GUILayout.Button(previewText, GUILayout.Width(24f))) previewMinimized.Value = !previewMinimized.Value;
			}
			GUILayout.EndHorizontal();

			if (previewMinimized.Value) return;

			var previewTexture = PreviewConstants.GetTexture(model, previewSelectedIndex) ?? Texture2D.blackTexture;

			primaryClick = primaryClick ?? ActionExtensions.GetEmpty<Vector3>();
			secondaryClick = secondaryClick ?? ActionExtensions.GetEmpty<Vector3>();

			var displayArea = DrawClickableTexture(
				previewTexture,
				previewSize,
				primaryClick,
				secondaryClick,
				previewClickable
			);

			drawOnPreview?.Invoke(displayArea);
		}

		public void DrawGalaxyTargets(GalaxyInfoModel model, Rect displayArea, GUIStyle style)
		{
			var galacticOriginInWindow = NormalToWindow(model.GalaxyOriginNormal, displayArea, out var galacticOriginInPreview);
			var playerStartInWindow = NormalToWindow(model.PlayerBeginNormal, displayArea, out var playerStartInPreview);
			var gameEndInWindow = NormalToWindow(model.PlayerEndNormal, displayArea, out var gameEndInPreview);

			EditorGUILayoutExtensions.PushColor(galacticOriginInPreview ? Color.yellow : Color.yellow.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(galacticOriginInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Galactic Origin"), style);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(playerStartInPreview ? Color.green : Color.green.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(playerStartInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Player Start"), style);
			}
			EditorGUILayoutExtensions.PopColor();

			EditorGUILayoutExtensions.PushColor(gameEndInPreview ? Color.red : Color.red.NewA(0.5f));
			{
				GUI.Box(CenteredScreen(gameEndInWindow, new Vector2(16f, 16f)), new GUIContent(string.Empty, "Game End"), style);
			}
			EditorGUILayoutExtensions.PopColor();
		}

		public bool HorizontalPreviewSupported(float ratio = 1.25f)
		{
			return ratio < (position.width / position.height);
		}
		#endregion
	}
}