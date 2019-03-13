using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GalaxyEditorWindow
	{
		EditorPrefsInt generationAssetsIndex;
		EditorPrefsFloat generationBarScroll;

		void GenerationConstruct()
		{
			var currPrefix = "Generation";

			generationAssetsIndex = new EditorPrefsInt(currPrefix + "AssetsIndex");
			generationBarScroll = new EditorPrefsFloat(currPrefix + "BarScroll");

			RegisterToolbar("Generation", GenerationToolbar);
		}

		void GenerationToolbar(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				if (model.MaximumSectorSystemCount < model.MinimumSectorSystemCount) EditorGUILayout.HelpBox("Maximum Sector System Count must be higher than the minimum", MessageType.Error);
				model.MinimumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Minimum Sector System Count", "The minimum bodies ever spawned in a sector."), model.MinimumSectorSystemCount));
				model.MaximumSectorSystemCount.Value = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Maximum Sector System Count", "The maximum bodies ever spawned in a sector."), model.MaximumSectorSystemCount));

				model.SectorSystemChance.Value = EditorGUILayoutAnimationCurve.Field(new GUIContent("Sector System Chance", "The bodymap is a linear gradient that is evaluated along a curve, then remapped between the minimum and maximum sector body count."), model.SectorSystemChance.Value);
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			var generationAssetsIndexPrevious = generationAssetsIndex.Value;

			generationAssetsIndex.Value = GUILayout.Toolbar(
				generationAssetsIndex.Value,
				new string[] {
					"Noise Data",
					"Texture Data"
				}
			);

			if (generationAssetsIndexPrevious != generationAssetsIndex.Value) generationBarScroll.Value = 0f;

			switch (generationAssetsIndex.Value)
			{
				case 0:
					GenerationProceduralNoiseData(model);
					break;
				case 1:
					GenerationTextureData(model);
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized Asset Index: " + generationAssetsIndex.Value, MessageType.Error);
					break;
			}
		}

		void GenerationProceduralNoiseData(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				if (GUILayout.Button("Add Noise Data Definition")) 
				{
					GenerationCreateProceduralNoiseData(
						model
					);
				}
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			var all = model.NoiseData.Value;
			var deletedId = string.Empty;

			var keys = new List<string>();
			var keyDuplicates = new List<string>();

			foreach (var current in all)
			{
				if (keyDuplicates.Contains(current.Key.Value)) continue;

				if (keys.Contains(current.Key.Value)) keyDuplicates.Add(current.Key.Value);
				else keys.Add(current.Key.Value);
			}

			generationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, generationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					foreach (var noiseData in model.NoiseData.Value)
					{
						GUILayout.BeginVertical(EditorStyles.helpBox);
						{
							GenerationProceduralNoiseDataEntry(
								model,
								noiseData,
								keyDuplicates.Contains(noiseData.Key.Value),
								ref deletedId
							);
						}
						GUILayout.EndVertical();
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
			}
			GUILayout.EndScrollView();

			if (!string.IsNullOrEmpty(deletedId))
			{
				model.NoiseData.Value = model.NoiseData.Value.Where(d => d.NoiseDataId.Value != deletedId).ToArray();
			}
		}

		void GenerationCreateProceduralNoiseData(
			GalaxyInfoModel model
		)
		{
			var result = new ProceduralNoiseDataModel();
			result.NoiseDataId.Value = Guid.NewGuid().ToString();

			model.NoiseData.Value = model.NoiseData.Value.Append(result).ToArray();
		}

		void GenerationProceduralNoiseDataEntry(
			GalaxyInfoModel model,
			ProceduralNoiseDataModel entry,
			bool isDuplicateKey,
			ref string deletedId
		)
		{
			var keyName = string.IsNullOrEmpty(entry.Key.Value) ? "< no key >" : entry.Key.Value;
			var assetId = string.IsNullOrEmpty(entry.NoiseAssetId.Value) ? "< no asset id >" : entry.NoiseAssetId.Value;

			var headerError = string.IsNullOrEmpty(assetId) || string.IsNullOrEmpty(keyName);

			GUILayout.BeginHorizontal();
			{
				if (headerError) EditorGUILayoutExtensions.PushColor(Color.red);
				GUILayout.Label(keyName + "." + assetId);
				if (headerError) EditorGUILayoutExtensions.PopColor();
				if (EditorGUILayoutExtensions.XButton(true)) deletedId = entry.NoiseDataId.Value;
			}
			GUILayout.EndHorizontal();

			var keyColor = Color.red;
			var assetColor = Color.red;

			var wasKeyError = string.IsNullOrEmpty(entry.Key.Value) || isDuplicateKey;
			var wasAssetError = string.IsNullOrEmpty(entry.NoiseAssetId.Value);

			var keyTooltip = "The unique key used to retrieve this noise asset id.";
			if (wasKeyError)
			{
				if (string.IsNullOrEmpty(entry.Key.Value)) keyTooltip += "\n * A key must be specified.";
				if (isDuplicateKey) keyTooltip += "\n * This key is specified multiple times, this may cause unpredictable behaviour.";
			}

			var assetTooltip = "The id of the procedural noise asset.";
			if (wasAssetError) assetTooltip += "\n * A path must be specified.";

			if (wasKeyError) EditorGUILayoutExtensions.PushBackgroundColor(keyColor);
			{
				entry.Key.Value = EditorGUILayout.TextField(
					new GUIContent("Key", keyTooltip),
					entry.Key.Value
				);
			}
			if (wasKeyError) EditorGUILayoutExtensions.PopBackgroundColor();

			if (wasAssetError) EditorGUILayoutExtensions.PushBackgroundColor(assetColor);
			{
				entry.NoiseAssetId.Value = EditorGUILayout.TextField(
					new GUIContent("Noise Asset Id", assetTooltip),
					entry.NoiseAssetId.Value
				);
			}
			if (wasAssetError) EditorGUILayoutExtensions.PopBackgroundColor();
		}

		void GenerationTextureData(GalaxyInfoModel model)
		{
			EditorGUIExtensions.BeginChangeCheck();
			{
				var textureOptions = model.Textures.Keys.Where(k => !GalaxyBaseModel.TextureNamesAll.Contains(k) && model.TextureData.Value.None(d => d.TexturePath.Value == k)).ToList();
				
				textureOptions.Insert(0, "< Blank >");
				textureOptions.Insert(0, "- Select a Texture -");
				
				var selection = EditorGUILayout.Popup("Define Texture", 0, textureOptions.ToArray());
				
				switch (selection)
				{
					case 0: break;
					case 1:
						GenerationCreateTextureData(model, null);
						break;
					default:
						GenerationCreateTextureData(model, textureOptions[selection]);
						break;
				}
			}
			EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

			var all = model.TextureData.Value;
			var deletedId = string.Empty;
			
			var keys = new List<string>();
			var keyDuplicates = new List<string>();
			
			foreach (var current in all)
			{
				if (keyDuplicates.Contains(current.Key.Value)) continue;
				
				if (keys.Contains(current.Key.Value)) keyDuplicates.Add(current.Key.Value);
				else keys.Add(current.Key.Value);
			}
			
			generationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, generationBarScroll), false, true).y;
			{
				EditorGUIExtensions.BeginChangeCheck();
				{
					foreach (var textureData in all)
					{
						GUILayout.BeginVertical(EditorStyles.helpBox);
						{
							GenerationTextureDataEntry(
								model,
								textureData,
								keyDuplicates.Contains(textureData.Key.Value),
								ref deletedId
							);
						}
						GUILayout.EndVertical();
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);
			}
			GUILayout.EndScrollView();

			if (!string.IsNullOrEmpty(deletedId))
			{
				model.TextureData.Value = model.TextureData.Value.Where(d => d.TextureDataId.Value != deletedId).ToArray();
			}
		}

		void GenerationCreateTextureData(
			GalaxyInfoModel model,
			string textureName
		)
		{
			var result = new TextureDataModel();
			result.TextureDataId.Value = Guid.NewGuid().ToString();
			result.TexturePath.Value = textureName;
			result.Channel.Value = TextureDataModel.Channels.Red;
			result.Wrapping.Value = TextureDataModel.WrappingTypes.Clamped;

			model.TextureData.Value = model.TextureData.Value.Append(result).ToArray();
		}

		void GenerationTextureDataEntry(
			GalaxyInfoModel model,
			TextureDataModel entry,
			bool isDuplicateKey,
			ref string deletedId
		)
		{
			var keyName = string.IsNullOrEmpty(entry.Key.Value) ? "< no key >" : entry.Key.Value;
			var pathName = string.IsNullOrEmpty(entry.TexturePath.Value) ? "< no path >" : entry.TexturePath.Value;

			var headerError = string.IsNullOrEmpty(pathName) || string.IsNullOrEmpty(keyName);

			GUILayout.BeginHorizontal();
			{
				if (headerError) EditorGUILayoutExtensions.PushColor(Color.red);
				GUILayout.Label(keyName + "." + pathName);
				if (headerError) EditorGUILayoutExtensions.PopColor();
				if (EditorGUILayoutExtensions.XButton(true)) deletedId = entry.TextureDataId.Value;
			}
			GUILayout.EndHorizontal();

			var keyColor = Color.red;
			var pathColor = Color.red;

			var wasKeyError = string.IsNullOrEmpty(entry.Key.Value) || isDuplicateKey;
			var wasPathError = string.IsNullOrEmpty(entry.TexturePath.Value);

			var keyTooltip = "The unique key used to retrieve values from the target texture.";
			if (wasKeyError)
			{
				if (string.IsNullOrEmpty(entry.Key.Value)) keyTooltip += "\n * A key must be specified.";
				if (isDuplicateKey) keyTooltip += "\n * This key is specified multiple times, this may cause unpredictable behaviour.";
			}

			var pathTooltip = "The path of the texture within the sibling folder.";
			if (wasPathError) pathTooltip += "\n * A path must be specified.";
			else if (!model.Textures.ContainsKey(entry.TexturePath.Value))
			{
				wasPathError = true;
				pathColor = Color.yellow;
				pathTooltip += "\n * No texture with this path could be found.";
			}

			const float DropDownWidths = 150f;

			GUILayout.BeginHorizontal();
			{
				if (wasKeyError) EditorGUILayoutExtensions.PushBackgroundColor(keyColor);
				{
					entry.Key.Value = EditorGUILayout.TextField(
						new GUIContent("Key", keyTooltip),
						entry.Key.Value
					);
				}
				if (wasKeyError) EditorGUILayoutExtensions.PopBackgroundColor();

				entry.Channel.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Channel -",
					entry.Channel.Value,
					Color.red,
					GUILayout.Width(DropDownWidths)
				);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{

				if (wasPathError) EditorGUILayoutExtensions.PushBackgroundColor(pathColor);
				{
					entry.TexturePath.Value = EditorGUILayout.TextField(
						new GUIContent("Texture Path", pathTooltip),
						entry.TexturePath.Value
					);
				}
				if (wasPathError) EditorGUILayoutExtensions.PopBackgroundColor();

				entry.Wrapping.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Wrap Type -",
					entry.Wrapping.Value,
					Color.red,
					GUILayout.Width(DropDownWidths)
				);
			}
			GUILayout.EndHorizontal();
		}
	}
}