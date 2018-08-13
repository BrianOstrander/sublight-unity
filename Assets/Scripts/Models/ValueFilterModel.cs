using System.Linq;
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

			foreach (var filter in newFilters)
			{
				switch (filter.FilterType)
				{
					case ValueFilterTypes.KeyValueBoolean:
						newBooleanKeyValues.Add(filter as BooleanKeyValueFilterEntryModel);
						break;
					default:
						Debug.LogError("Unrecognized FilterType" + filter.FilterType);
						break;
				}
			}
		}

		IValueFilterEntryModel[] OnGetFilters()
		{
			return booleanKeyValues.Cast<IValueFilterEntryModel>().ToArray();
			//return booleanKeyValues.Cast<IValueFilterEntryModel>().Concat(something)
																  //.Concat(somethingElse)
																  //.ToArray();
		}
		#endregion
	}
}