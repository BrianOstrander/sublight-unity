﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class IntegerKeyValueFilterEntryModel : KeyValueFilterEntryModel<int>
	{
		[JsonProperty] IntegerFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<IntegerFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueInteger; } }

		public IntegerKeyValueFilterEntryModel()
		{
			Operation = new ListenerProperty<IntegerFilterOperations>(value => operation = value, () => operation);
		}
	}
}