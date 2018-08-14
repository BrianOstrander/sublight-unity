﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ValueFilterModel : Model
	{
		#region Assigned Values
		[JsonProperty]
		BooleanKeyValueFilterEntryModel[] booleanKeyValues = new BooleanKeyValueFilterEntryModel[0];
		[JsonProperty]
		EncounterInteractionFilterEntryModel[] encounterInteractions = new EncounterInteractionFilterEntryModel[0];
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<IValueFilterEntryModel[]> Filters;
		#endregion

		public ValueFilterModel()
		{
			Filters = new ListenerProperty<IValueFilterEntryModel[]>(OnSetFilters, OnGetFilters);
		}

		#region Events
		void OnSetFilters(IValueFilterEntryModel[] newFilters)
		{
			var newBooleanKeyValues = new List<BooleanKeyValueFilterEntryModel>();
			var newEncounterInteractions = new List<EncounterInteractionFilterEntryModel>();

			foreach (var filter in newFilters)
			{
				switch (filter.FilterType)
				{
					case ValueFilterTypes.KeyValueBoolean:
						newBooleanKeyValues.Add(filter as BooleanKeyValueFilterEntryModel);
						break;
					case ValueFilterTypes.EncounterInteraction:
						newEncounterInteractions.Add(filter as EncounterInteractionFilterEntryModel);
						break;
					default:
						Debug.LogError("Unrecognized FilterType" + filter.FilterType);
						break;
				}
			}

			booleanKeyValues = newBooleanKeyValues.ToArray();
			encounterInteractions = newEncounterInteractions.ToArray();
		}

		IValueFilterEntryModel[] OnGetFilters()
		{
			return booleanKeyValues.Cast<IValueFilterEntryModel>().Concat(encounterInteractions)
																  .ToArray();
			//return booleanKeyValues.Cast<IValueFilterEntryModel>().Concat(something)
																  //.Concat(somethingElse)
																  //.ToArray();
		}
		#endregion
	}
}