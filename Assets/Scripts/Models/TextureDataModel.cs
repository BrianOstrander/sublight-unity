using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TextureDataModel : Model
	{
		public enum Channels
		{
			Unknown = 0,
			Red = 10,
			Green = 20,
			Blue = 30,
			Alpha = 40,
			Hue = 50,
			Saturation = 60,
			Value = 70
		}

		public enum Wrapping
		{
			Unknown = 0,
			Clamped = 10,
			Repeat = 20
		}

		[Serializable]
		public class Definition
		{
			public string DefinitionId;
			public string Key;
			public Channels Channel;
			public Wrapping Wrapping;
		}

		public class DefinitionInstance
		{
			public Texture2D Texture;
			public Definition DefinitionEntry;

			public float GetValue(
				Vector2 normalPosition,
				float fallback = 0f
			)
			{
				var x = Mathf.FloorToInt(Texture.width * normalPosition.x);
				var y = Mathf.FloorToInt(Texture.height * normalPosition.y);

				switch (DefinitionEntry.Wrapping)
				{
					case Wrapping.Clamped:
						x = Mathf.Clamp(x, 0, Texture.width);
						y = Mathf.Clamp(y, 0, Texture.height);
						break;
					case Wrapping.Repeat:
						x = x % Texture.width;
						y = y % Texture.height;
						break;
					default:
						Debug.LogError("Unrecognized wrapping mode");
						break;
				}

				var color = Texture.GetPixel(x, y);

				switch (DefinitionEntry.Channel)
				{
					case Channels.Red: return color.r;
					case Channels.Green: return color.g;
					case Channels.Blue: return color.b;
					case Channels.Alpha: return color.a;
					case Channels.Hue: return color.GetH();
					case Channels.Saturation: return color.GetS();
					case Channels.Value: return color.GetV();
					default:
						Debug.LogError("Unrecognized channel: " + DefinitionEntry.Channel);
						return fallback;
				}
			}
		}

		/// <summary>
		/// Keys are not case sensitive, so we normalize them here, along with
		/// any other operations.
		/// </summary>
		/// <returns>The key.</returns>
		/// <param name="key">Key.</param>
		string NormalizeKey(string key) { return string.IsNullOrEmpty(key) ? key : key.ToLower(); }

		[JsonProperty] string texturePath;
		[JsonIgnore] public readonly ListenerProperty<string> TexturePath;

		[JsonProperty] Definition[] definitions = new Definition[0];
		[JsonIgnore] public readonly ListenerProperty<Definition[]> Definitions;

		public TextureDataModel()
		{
			TexturePath = new ListenerProperty<string>(value => texturePath = value, () => texturePath);
			Definitions = new ListenerProperty<Definition[]>(value => definitions = value, () => definitions);
		}

		public DefinitionInstance CreateInstance(
			string key,
			Texture2D texture
		)
		{
			if (string.IsNullOrEmpty(key) || texture == null) return null;

			key = NormalizeKey(key);

			var definition = definitions.FirstOrDefault(d => d.Key == key);

			if (definition == null) return null;

			return new DefinitionInstance
			{
				Texture = texture,
				DefinitionEntry = definition
			};
		}
	}
}