﻿namespace LunraGames.SubLight.Models
{
	public class BooleanKeyValueFilterEntryModel : KeyValueFilterEntryModel<bool>
	{
		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueBoolean; } }
		public override KeyValueTypes FilterValueType { get { return KeyValueTypes.Boolean; } }
	}
}