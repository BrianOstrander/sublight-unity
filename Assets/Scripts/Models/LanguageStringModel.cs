using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageStringModel : Model
	{
		[JsonProperty] string key = Guid.NewGuid().ToString();

		string value;

		[JsonIgnore]
		public readonly ListenerProperty<string> Key;
		[JsonIgnore]
		public readonly ListenerProperty<string> Value;
		[JsonIgnore]
		public bool HasUnsavedValue;

		[JsonIgnore]
		public string lol { get { return value; } }

		public LanguageStringModel()
		{
			Key = new ListenerProperty<string>(value => key = value, () => key);
			Value = new ListenerProperty<string>(v => value = v, () => value);
		}
	}
}