using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	/// <summary>
	/// Preferences modified by ingame actions or encounters.
	/// </summary>
	/// <remarks>
	/// This probably shouldn't be used for stuff like keybindings, visual
	/// settings, etc. Use the PreferencesModel for that.
	/// </remarks>
	public class PreferencesKeyValuesModel : SaveModel
	{
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();

		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		public PreferencesKeyValuesModel()
		{
			SaveType = SaveTypes.PreferencesKeyValues;
		}
	}
}