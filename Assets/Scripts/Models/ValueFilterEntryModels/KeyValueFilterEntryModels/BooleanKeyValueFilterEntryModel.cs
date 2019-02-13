namespace LunraGames.SubLight.Models
{
	public class BooleanKeyValueFilterEntryModel : KeyValueFilterEntryModel<bool>
	{
		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueBoolean; } }
		public override KeyValueTypes FilterKeyValueType { get { return KeyValueTypes.Boolean; } }
	}
}