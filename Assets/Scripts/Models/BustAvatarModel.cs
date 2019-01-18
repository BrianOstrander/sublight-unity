using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustAvatarModel : Model
	{
		public enum AvatarTypes
		{
			Unknown = 0,
			None = 10,
			StaticImage = 20
		}

		[JsonProperty] AvatarTypes avatarType;

		[JsonIgnore]
		public readonly ListenerProperty<AvatarTypes> AvatarType;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return KeyValues; } }

		public BustAvatarModel()
		{
			AvatarType = new ListenerProperty<AvatarTypes>(value => avatarType = value, () => avatarType);
		}
	}
}