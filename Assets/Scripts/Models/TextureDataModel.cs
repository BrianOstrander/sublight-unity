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

		public enum WrappingTypes
		{
			Unknown = 0,
			Clamped = 10,
			Repeat = 20
		}

		public class DefinitionInstance
		{
			public Texture2D Texture;
			public TextureDataModel Model;

			public float GetValue(
				Vector2 normalPosition,
				float fallback = 0f
			)
			{
				var x = Mathf.FloorToInt(Texture.width * normalPosition.x);
				var y = Mathf.FloorToInt(Texture.height * normalPosition.y);

				switch (Model.Wrapping.Value)
				{
					case WrappingTypes.Clamped:
						x = Mathf.Clamp(x, 0, Texture.width);
						y = Mathf.Clamp(y, 0, Texture.height);
						break;
					case WrappingTypes.Repeat:
						x = x % Texture.width;
						y = y % Texture.height;
						break;
					default:
						Debug.LogError("Unrecognized Wrapping Type: "+Model.Wrapping.Value);
						break;
				}

				var color = Texture.GetPixel(x, y);

				switch (Model.Channel.Value)
				{
					case Channels.Red: return color.r;
					case Channels.Green: return color.g;
					case Channels.Blue: return color.b;
					case Channels.Alpha: return color.a;
					case Channels.Hue: return color.GetH();
					case Channels.Saturation: return color.GetS();
					case Channels.Value: return color.GetV();
					default:
						Debug.LogError("Unrecognized Channel: " + Model.Channel.Value);
						return fallback;
				}
			}
		}

		[JsonProperty] string textureDataId;
		[JsonIgnore] public readonly ListenerProperty<string> TextureDataId;

		[JsonProperty] string texturePath;
		[JsonIgnore] public readonly ListenerProperty<string> TexturePath;

		[JsonProperty] string key;
		[JsonIgnore] public readonly ListenerProperty<string> Key;

		[JsonProperty] Channels channel;
		[JsonIgnore] public readonly ListenerProperty<Channels> Channel;

		[JsonProperty] WrappingTypes wrapping;
		[JsonIgnore] public readonly ListenerProperty<WrappingTypes> Wrapping;

		public TextureDataModel()
		{
			TextureDataId = new ListenerProperty<string>(value => textureDataId = value, () => textureDataId);
			TexturePath = new ListenerProperty<string>(value => texturePath = value, () => texturePath);
			Key = new ListenerProperty<string>(value => key = value, () => key);
			Channel = new ListenerProperty<Channels>(value => channel = value, () => channel);
			Wrapping = new ListenerProperty<WrappingTypes>(value => wrapping = value, () => wrapping);
		}

		public DefinitionInstance CreateInstance(
			Texture2D texture
		)
		{
			if (texture == null) throw new ArgumentNullException("texture");

			return new DefinitionInstance
			{
				Texture = texture,
				Model = this
			};
		}
	}
}