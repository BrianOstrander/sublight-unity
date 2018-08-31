using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class LanguageStringModel : Model
	{
		[JsonProperty] string key = Guid.NewGuid().ToString();

		string value;
		string cachedValue;
		RequestStatus valueStatus;

		[JsonIgnore]
		readonly ListenerProperty<string> cachedValueListener;

		[JsonIgnore]
		public Action<string, string, Action<string, RequestStatus>> ValueChange;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;
		/// <summary>
		/// The value, this will update the database entry.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Value;
		/// <summary>
		/// The value as it is in the database.
		/// </summary>
		[JsonIgnore]
		public readonly ReadonlyProperty<string> CachedValue;
		// Unknown: Query in progress
		// Cancel: Qued for Query
		// Success: Loaded
		// Failure: Failed to load
		[JsonIgnore]
		public readonly ListenerProperty<RequestStatus> ValueStatus;

		public LanguageStringModel()
		{
			Key = new ListenerProperty<string>(value => key = value, () => key);
			CachedValue = new ReadonlyProperty<string>(
				value => cachedValue = value,
				() => cachedValue,
				out cachedValueListener
			);
			Value = new ListenerProperty<string>(OnSetValue, OnGetValue);
			ValueStatus = new ListenerProperty<RequestStatus>(value => valueStatus = value, () => valueStatus);
		}

		#region Events
		void OnSetValue(string newValue)
		{
			value = newValue;
			if (ValueChange == null) OnSetValueDone(newValue, newValue, RequestStatus.Success);
			else
			{
				ValueStatus.Value = RequestStatus.Unknown;
				ValueChange(Key.Value, newValue, (databaseValue, status) => OnSetValueDone(databaseValue, newValue, status));
			}
		}

		void OnSetValueDone(string databaseValue, string updatedValue, RequestStatus status)
		{
			ValueStatus.Value = status;
			if (status != RequestStatus.Success)
			{
				Debug.LogError("Updated language value with key " + Key.Value + " returned with status " + status + "\n\tDatabaseValue: " + databaseValue + "\n\tUpdatedValue: " + updatedValue);
				return;
			}
			cachedValueListener.Value = databaseValue;
		}

		string OnGetValue() { return value; }
		#endregion
	}
}