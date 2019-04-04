using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GamemodeInfoModel : SaveModel
	{
		public static class TextureNames
		{
			public const string Icon = "icon";
		}

		[JsonProperty] bool isInDevelopment;
		[JsonIgnore] public readonly ListenerProperty<bool> IsInDevelopment;

		[JsonProperty] string gamemodeId;
		[JsonIgnore] public readonly ListenerProperty<string> GamemodeId;

		[JsonProperty] string name;
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] int orderWeight;
		[JsonIgnore] public readonly ListenerProperty<int> OrderWeight;

		[JsonProperty] string title;
		[JsonIgnore] public readonly ListenerProperty<string> Title;

		[JsonProperty] string subTitle;
		[JsonIgnore] public readonly ListenerProperty<string> SubTitle;

		[JsonProperty] string description;
		[JsonIgnore] public readonly ListenerProperty<string> Description;

		[JsonProperty] string gamemodeKey;
		[JsonIgnore] public readonly ListenerProperty<string> GamemodeKey;

		[JsonIgnore] public Texture2D Icon { get { return GetTexture(TextureNames.Icon); } }

		public GamemodeInfoModel()
		{
			SaveType = SaveTypes.GamemodeInfo;

			SiblingBehaviour = SiblingBehaviours.All;

			IsInDevelopment = new ListenerProperty<bool>(value => isInDevelopment = value, () => isInDevelopment);
			GamemodeId = new ListenerProperty<string>(value => gamemodeId = value, () => gamemodeId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			OrderWeight = new ListenerProperty<int>(value => orderWeight = value, () => orderWeight);
			Title = new ListenerProperty<string>(value => title = value, () => title);
			SubTitle = new ListenerProperty<string>(value => subTitle = value, () => subTitle);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			GamemodeKey = new ListenerProperty<string>(value => gamemodeKey = value, () => gamemodeKey);
		}

		protected override void OnPrepareTexture(string name, Texture2D texture)
		{
			// TODO: This should probably be exposed by some interface, oh well...
			switch (name)
			{
				case TextureNames.Icon:
					texture.anisoLevel = 4;
					texture.wrapMode = TextureWrapMode.Clamp;
					break;
			}
		}
	}
}