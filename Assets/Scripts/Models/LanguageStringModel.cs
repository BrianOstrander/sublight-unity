using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageStringModel : Model
	{
		[JsonProperty] string key = Guid.NewGuid().ToString();
		[JsonProperty] bool showDetails;

		string value;

		[JsonIgnore]
		public readonly ListenerProperty<string> Key;
		[JsonIgnore]
		public readonly ListenerProperty<string> Value;
		[JsonIgnore]
		public readonly ListenerProperty<bool> ShowDetails;
		[JsonIgnore]
		public bool HasUnsavedValue;

		public LanguageStringModel()
		{
			Key = new ListenerProperty<string>(value => key = value, () => key);
			Value = new ListenerProperty<string>(v => value = v, () => value);
			ShowDetails = new ListenerProperty<bool>(value => showDetails = value, () => showDetails);
		}

		/// <summary>
		/// Converts the LanguageStringModel to a string.
		/// </summary>
		/// <remarks>
		/// Only one way casting is supported.
		/// </remarks>
		/// <returns>The implicit.</returns>
		/// <param name="p">P.</param>
		public static implicit operator string(LanguageStringModel m)
		{
			return m.Value;
		}
	}
}