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
		const int ResolutionPerSector = 16;
		const float PreviewResolution = 512f;

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
			RegisterToolbar("General", GeneralToolbar);
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
				GeneralOnGeneratePreview(model, DemonUtility.NextInteger);
			}
			else if (!generalPreview.GenerationAttempted)
			{
				GeneralOnGeneratePreview(model);
			}

			if (!generalPreview.GenerationSuccess)
			{
				EditorGUILayout.HelpBox("A noise entry for this id is not stubbed out.", MessageType.Error);
				return;
			}

			GUILayout.Label("Seed: " + generalPreview.Seed);
			GUILayout.Label("Range: " + generalPreview.MinimumValue.ToString("N2")+" , "+generalPreview.MaximumValue.ToString("N2"));
			GUILayout.Box(GUIContent.none, generalPreview.PreviewStyle);
		}

		void GeneralOnGeneratePreview(ProceduralNoiseModel model, int seed = 0)
		{
			generalPreview = new NoisePreview();

			generalPreview.Seed = seed;
			generalPreview.GenerationAttempted = true;
			generalPreview.Root = model.CreateInstance(seed);

			if (generalPreview.Root == null) return;
			generalPreview.GenerationSuccess = true;

			generalPreview.PreviewTexture = new Texture2D(
				ResolutionPerSector,
				ResolutionPerSector
			);

			generalPreview.PreviewTexture.filterMode = FilterMode.Point;

			var values = new float[ResolutionPerSector, ResolutionPerSector];
			generalPreview.MinimumValue = float.MaxValue;
			generalPreview.MaximumValue = float.MinValue;

			for (var x = 0; x < ResolutionPerSector; x++)
			{
				var xNormal = x / (float)ResolutionPerSector;

				for (var y = 0; y < ResolutionPerSector; y++)
				{
					var yNormal = y / (float)ResolutionPerSector;
					var value = generalPreview.Root.GetValue(xNormal, yNormal, 0f);
					generalPreview.MinimumValue = Mathf.Min(value, generalPreview.MinimumValue);
					generalPreview.MaximumValue = Mathf.Max(value, generalPreview.MaximumValue);
					values[x, y] = value;
				}
			}

			var valueDelta = generalPreview.MaximumValue - generalPreview.MinimumValue;
			var valueDeltaNonZero = !Mathf.Approximately(0f, valueDelta);

			for (var x = 0; x < ResolutionPerSector; x++)
			{
				for (var y = 0; y < ResolutionPerSector; y++)
				{
					var value = values[x, y];
					if (valueDeltaNonZero) value = (value - generalPreview.MinimumValue) / valueDelta;
					else value = 0f;

					generalPreview.PreviewTexture.SetPixel(x, y, new Color(value, value, value));
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