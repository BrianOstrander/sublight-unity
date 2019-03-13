using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

using LunraGames.NumberDemon;

using LibNoise;

namespace LunraGames.SubLight
{
	public partial class ProceduralNoiseEditorWindow
	{
		const float PreviewResolution = 512f;

		public enum PreviewModes
		{
			Unknown = 0,
			Absolute = 10,
			Normalized = 20
		}

		public enum ColorModes
		{
			Unknown = 0,
			Grayscale = 10,
			HotAndCold = 20
		}

		EditorPrefsInt generalPreviewSeed;
		EditorPrefsInt generalPreviewResolution;
		EditorPrefsEnum<FilterMode> generalPreviewFiltering;
		EditorPrefsEnum<PreviewModes> generalPreviewMode;
		EditorPrefsEnum<ColorModes> generalPreviewColorMode;
		EditorPrefsBool generalPreviewClampingEnabled;

		struct NoisePreview
		{
			public bool GenerationAttempted;
			public bool GenerationSuccess;
			public int Seed;
			public float MinimumValue;
			public float MaximumValue;
			public IModule Root;
			public Texture2D PreviewTexture;
			public GUIStyle PreviewStyle;
		}

		NoisePreview generalPreview;

		void GeneralConstruct()
		{
			var currPrefix = KeyPrefix + "General";

			generalPreviewSeed = new EditorPrefsInt(currPrefix + "PreviewSeed");
			generalPreviewResolution = new EditorPrefsInt(currPrefix + "PreviewResolution", 16);
			generalPreviewFiltering = new EditorPrefsEnum<FilterMode>(currPrefix + "PreviewFiltering", FilterMode.Point);
			generalPreviewMode = new EditorPrefsEnum<PreviewModes>(currPrefix + "PreviewMode", PreviewModes.Absolute);
			generalPreviewColorMode = new EditorPrefsEnum<ColorModes>(currPrefix + "PreviewColorMode", ColorModes.Grayscale);
			generalPreviewClampingEnabled = new EditorPrefsBool(currPrefix + "PreviewClampingEnabled");

			RegisterToolbar("General", GeneralToolbar);

			BeforeLoadSelection += GeneralOnBeforeLoadSelection;
		}

		void GeneralOnBeforeLoadSelection()
		{
			generalPreview = default(NoisePreview);
		}

		void GeneralToolbar(ProceduralNoiseModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Ignore", GUILayout.ExpandWidth(false));
					model.Ignore.Value = EditorGUILayout.Toggle(model.Ignore.Value, GUILayout.Width(14f));
				}
				GUILayout.EndHorizontal();

				model.ProceduralNoiseId.Value = model.SetMetaKey(
					MetaKeyConstants.ProceduralNoise.ProceduralNoiseId,
					EditorGUILayout.TextField("Procedural Noise Id", model.ProceduralNoiseId.Value)
				);

				model.Name.Value = EditorGUILayout.TextField(new GUIContent("Name", "The internal name for production purposes."), model.Name.Value);
				model.Meta.Value = model.Name;
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			EditorGUILayout.HelpBox("Procedural noise models are currently just stubbed out.", MessageType.Warning);

			if (GUILayout.Button("Regenerate"))
			{
				GeneralOnGeneratePreview(model, generalPreviewSeed.Value);
			}
			else if (!generalPreview.GenerationAttempted)
			{
				GeneralOnGeneratePreview(model, generalPreviewSeed.Value);
			}

			if (!generalPreview.GenerationSuccess)
			{
				EditorGUILayout.HelpBox("A noise entry for this id is not stubbed out.", MessageType.Error);
				return;
			}

			var previewModified = false;
			EditorGUIExtensions.BeginChangeCheck();
			{
				GUILayout.BeginHorizontal();
				{
					generalPreviewSeed.Value = EditorGUILayout.IntField("Seed", generalPreviewSeed.Value);
					if (GUILayout.Button("Randomize", GUILayout.Width(72f))) generalPreviewSeed.Value = DemonUtility.NextInteger;
				}
				GUILayout.EndHorizontal();

				generalPreviewResolution.Value = Mathf.Clamp(
					EditorGUILayout.IntField("Resolution", generalPreviewResolution.Value),
					16,
					200
				);

				generalPreviewFiltering.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
					new GUIContent("Filtering"),
					default(FilterMode).ToString(),
					generalPreviewFiltering.Value
				);

				GUILayout.BeginHorizontal();
				{
					generalPreviewMode.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
						new GUIContent("Preview"),
						"- Select Preview -",
						generalPreviewMode.Value
					);

					generalPreviewColorMode.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
						GUIContent.none,
						"- Select Color Mode -",
						generalPreviewColorMode.Value
					);

					generalPreviewClampingEnabled.Value = EditorGUILayoutExtensions.ToggleButton(
						GUIContent.none,
						generalPreviewClampingEnabled.Value,
						"Clamping",
						"Not Clamping"
					);
				}
				GUILayout.EndHorizontal();

				GUILayout.Label("Range: " + generalPreview.MinimumValue.ToString("N2")+" , "+generalPreview.MaximumValue.ToString("N2"));
				GUILayout.Label("Value at 0, 0, 0: " + generalPreview.Root.GetValue(0.1f, 0.1f, 0f).ToString("N6"));
			}
			EditorGUIExtensions.EndChangeCheck(ref previewModified);

			if (previewModified) GeneralOnGeneratePreview(model, generalPreviewSeed.Value);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Box(GUIContent.none, generalPreview.PreviewStyle);
			}
			GUILayout.EndHorizontal();
		}

		void GeneralOnGeneratePreview(ProceduralNoiseModel model, int seed = 0)
		{
			if (generalPreview.PreviewTexture != null)
			{
				DestroyImmediate(generalPreview.PreviewTexture);
			}

			generalPreview = new NoisePreview();

			generalPreview.Seed = seed;
			generalPreview.GenerationAttempted = true;
			generalPreview.Root = model.CreateInstance(seed);

			if (generalPreview.Root == null) return;
			generalPreview.GenerationSuccess = true;

			generalPreview.PreviewTexture = new Texture2D(
				generalPreviewResolution.Value,
				generalPreviewResolution.Value
			);

			generalPreview.PreviewTexture.filterMode = generalPreviewFiltering.Value;

			var values = new float[generalPreviewResolution.Value, generalPreviewResolution.Value];
			generalPreview.MinimumValue = float.MaxValue;
			generalPreview.MaximumValue = float.MinValue;

			for (var x = 0; x < generalPreviewResolution.Value; x++)
			{
				var xNormal = x / (float)generalPreviewResolution.Value;

				for (var y = 0; y < generalPreviewResolution.Value; y++)
				{
					var yNormal = y / (float)generalPreviewResolution.Value;
					var value = generalPreview.Root.GetValue(xNormal, yNormal, 0f);
					generalPreview.MinimumValue = Mathf.Min(value, generalPreview.MinimumValue);
					generalPreview.MaximumValue = Mathf.Max(value, generalPreview.MaximumValue);
					values[x, y] = value;
				}
			}

			var valueDelta = generalPreview.MaximumValue - generalPreview.MinimumValue;
			var valueDeltaNonZero = !Mathf.Approximately(0f, valueDelta);

			for (var x = 0; x < generalPreviewResolution.Value; x++)
			{
				for (var y = 0; y < generalPreviewResolution.Value; y++)
				{
					var value = values[x, y];

					switch (generalPreviewMode.Value)
					{
						case PreviewModes.Normalized:
							if (valueDeltaNonZero) value = (value - generalPreview.MinimumValue) / valueDelta;
							else value = 0f;
							break;
					}

					Color color;

					var outsideRange = generalPreviewClampingEnabled.Value && (value < -1f || 1f < value);

					switch (generalPreviewColorMode.Value)
					{
						case ColorModes.HotAndCold:
							if (outsideRange) color = Color.black;
							else if (value < 0f)
							{
								if (Mathf.Approximately(0f, generalPreview.MinimumValue)) color = Color.white;
								else color = Color.Lerp(Color.white, Color.blue, Mathf.Abs(value) / Mathf.Abs(generalPreview.MinimumValue));
							}
							else
							{
								if (Mathf.Approximately(0f, generalPreview.MaximumValue)) color = Color.white;
								else color = Color.Lerp(Color.white, Color.red, value / generalPreview.MaximumValue);
							}
							break;
						case ColorModes.Grayscale:
						default:
							if (outsideRange) color = Color.magenta;
							else color = new Color(value, value, value);
							break;
					}
					generalPreview.PreviewTexture.SetPixel(x, y, color);
				}
			}
			generalPreview.PreviewTexture.Apply();

			var style = new GUIStyle(GUIStyle.none);
			style.normal.background = generalPreview.PreviewTexture;
			style.active.background = generalPreview.PreviewTexture;
			style.fixedWidth = PreviewResolution;
			style.fixedHeight = PreviewResolution;

			generalPreview.PreviewStyle = style;
		}
	}
}