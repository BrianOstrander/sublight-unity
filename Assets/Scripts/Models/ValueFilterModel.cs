﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ValueFilterModel : Model
	{
		public static ValueFilterModel Default(bool defaultValue = true)
		{
			var result = new ValueFilterModel();
			result.FalseByDefault.Value = !defaultValue;
			return result;
		}

		#region Assigned Values
		[JsonProperty] bool showValues;
		[JsonProperty] bool falseByDefault;
		[JsonProperty] BooleanKeyValueFilterEntryModel[] booleanKeyValues = new BooleanKeyValueFilterEntryModel[0];
		[JsonProperty] StringKeyValueFilterEntryModel[] stringKeyValues = new StringKeyValueFilterEntryModel[0];
		[JsonProperty] EncounterInteractionFilterEntryModel[] encounterInteractions = new EncounterInteractionFilterEntryModel[0];

		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<bool> ShowValues;
		[JsonIgnore]
		public readonly ListenerProperty<bool> FalseByDefault;
		[JsonIgnore]
		public readonly ListenerProperty<IValueFilterEntryModel[]> Filters;
		#endregion

		#region Utility
		[JsonIgnore]
		public bool HasFilters { get { return Filters.Value.Any(); } }
		#endregion

		public ValueFilterModel()
		{
			ShowValues = new ListenerProperty<bool>(value => showValues = value, () => showValues);
			FalseByDefault = new ListenerProperty<bool>(value => falseByDefault = value, () => falseByDefault);
			Filters = new ListenerProperty<IValueFilterEntryModel[]>(OnSetFilters, OnGetFilters);
		}

		#region Events
		void OnSetFilters(IValueFilterEntryModel[] newFilters)
		{
			var newBooleanKeyValues = new List<BooleanKeyValueFilterEntryModel>();
			var newStringKeyValues = new List<StringKeyValueFilterEntryModel>();
			var newEncounterInteractions = new List<EncounterInteractionFilterEntryModel>();

			foreach (var filter in newFilters)
			{
				switch (filter.FilterType)
				{
					case ValueFilterTypes.KeyValueBoolean:
						newBooleanKeyValues.Add(filter as BooleanKeyValueFilterEntryModel);
						break;
					case ValueFilterTypes.KeyValueString:
						newStringKeyValues.Add(filter as StringKeyValueFilterEntryModel);
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
			stringKeyValues = newStringKeyValues.ToArray();
			encounterInteractions = newEncounterInteractions.ToArray();
		}

		IValueFilterEntryModel[] OnGetFilters()
		{
			return booleanKeyValues.Cast<IValueFilterEntryModel>().Concat(stringKeyValues)
																  .Concat(encounterInteractions)
																  .ToArray();
		}
		#endregion
	}
}