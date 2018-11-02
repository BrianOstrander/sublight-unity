using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageStringModel : Model
	{
		/// <summary>
		/// Used until an actual language loading system is built.
		/// </summary>
		/// <returns>The override.</returns>
		/// <param name="value">Value.</param>
		public static LanguageStringModel Override(string value)
		{
			var result = new LanguageStringModel();
			result.Value.Value = value;
			return result;
		}

		public static LanguageStringModel Empty
		{
			get
			{
				var result = new LanguageStringModel();
				result.Value.Value = String.Empty;
				return result;
			}
		}

		[JsonProperty] string key = Guid.NewGuid().ToString();

		string value;

		[JsonIgnore]
		public readonly ListenerProperty<string> Key;
		[JsonIgnore]
		public readonly ListenerProperty<string> Value;

		public LanguageStringModel()
		{
			Key = new ListenerProperty<string>(value => key = value, () => key);
			Value = new ListenerProperty<string>(v => value = v, () => value);
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
