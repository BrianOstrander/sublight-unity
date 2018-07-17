﻿using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class GlobalKeyValuesModel : SaveModel
	{
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();

		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		public GlobalKeyValuesModel()
		{
			SaveType = SaveTypes.GlobalKeyValues;
		}
	}
}