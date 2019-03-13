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
		EditorPrefsFloat generationBarScroll;

		void GenerationConstruct()
		{
			var currPrefix = "Generation";
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

			generationBarScroll.Value = GUILayout.BeginScrollView(new Vector2(0f, generationBarScroll), false, true).y;
			{
				var allTextureData = model.TextureData.Value;
				var deletedTextureDataId = string.Empty;

				var textureDataKeys = new List<string>();
				var textureDataKeyDuplicates = new List<string>();

				foreach (var textureData in allTextureData)
				{
					if (textureDataKeyDuplicates.Contains(textureData.Key.Value)) continue;

					if (textureDataKeys.Contains(textureData.Key.Value)) textureDataKeyDuplicates.Add(textureData.Key.Value);
					else textureDataKeys.Add(textureData.Key.Value);
				}

				EditorGUIExtensions.BeginChangeCheck();
				{
					foreach (var textureData in allTextureData)
					{
						GUILayout.BeginVertical(EditorStyles.helpBox);
						{
							GenerationTextureData(
								model,
								textureData,
								textureDataKeyDuplicates.Contains(textureData.TexturePath.Value),
								ref deletedTextureDataId
							);
						}
						GUILayout.EndVertical();
					}
				}
				EditorGUIExtensions.EndChangeCheck(ref ModelSelectionModified);

				if (!string.IsNullOrEmpty(deletedTextureDataId))
				{
					model.TextureData.Value = model.TextureData.Value.Where(d => d.TextureDataId.Value != deletedTextureDataId).ToArray();
				}
			}
			GUILayout.EndScrollView();
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

		void GenerationTextureData(
			GalaxyInfoModel model,
			TextureDataModel textureData,
			bool isDuplicateKey,
			ref string deletedTextureDataId
		)
		{
			var pathName = string.IsNullOrEmpty(textureData.TexturePath.Value) ? "< no path >" : textureData.TexturePath.Value;
			var keyName = string.IsNullOrEmpty(textureData.Key.Value) ? "< no key >" : textureData.Key.Value;

			var headerError = string.IsNullOrEmpty(pathName) || string.IsNullOrEmpty(keyName);

			GUILayout.BeginHorizontal();
			{
				if (headerError) EditorGUILayoutExtensions.PushColor(Color.red);
				GUILayout.Label(pathName + "." + keyName);
				if (headerError) EditorGUILayoutExtensions.PopColor();
				if (EditorGUILayoutExtensions.XButton(true)) deletedTextureDataId = textureData.TextureDataId.Value;
			}
			GUILayout.EndHorizontal();

			var pathColor = Color.red;
			var keyColor = Color.red;

			var wasPathError = string.IsNullOrEmpty(textureData.TexturePath.Value);
			var wasKeyError = string.IsNullOrEmpty(textureData.Key.Value) || isDuplicateKey;

			var pathTooltip = "The path of the texture within the sibling folder.";
			if (wasPathError) pathTooltip += "\n * A path must be specified.";
			else if (!model.Textures.ContainsKey(textureData.TexturePath.Value))
			{
				wasPathError = true;
				pathColor = Color.yellow;
				pathTooltip += "\n * No texture with this path could be found.";
			}

			var keyTooltip = "The unique key used to retrieve values from the target texture.";
			if (wasKeyError)
			{
				if (string.IsNullOrEmpty(textureData.Key.Value)) keyTooltip += "\n * A key must be specified.";
				if (isDuplicateKey) keyTooltip += "\n * This key is specified multiple times, this may cause unpredictable behaviour.";
			}

			const float DropDownWidths = 150f;

			GUILayout.BeginHorizontal();
			{

				if (wasPathError) EditorGUILayoutExtensions.PushBackgroundColor(pathColor);
				{
					textureData.TexturePath.Value = EditorGUILayout.TextField(
						new GUIContent("Texture Path", pathTooltip),
						textureData.TexturePath.Value
					);
				}
				if (wasPathError) EditorGUILayoutExtensions.PopBackgroundColor();

				textureData.Wrapping.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Wrap Type -",
					textureData.Wrapping.Value,
					Color.red,
					GUILayout.Width(DropDownWidths)
				);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if (wasKeyError) EditorGUILayoutExtensions.PushBackgroundColor(keyColor);
				{
					textureData.Key.Value = EditorGUILayout.TextField(
						new GUIContent("Key", keyTooltip),
						textureData.Key.Value
					);
				}
				if (wasKeyError) EditorGUILayoutExtensions.PopBackgroundColor();

				textureData.Channel.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Channel -",
					textureData.Channel.Value,
					Color.red,
					GUILayout.Width(DropDownWidths)
				);
			}
			GUILayout.EndHorizontal();
		}
	}
}