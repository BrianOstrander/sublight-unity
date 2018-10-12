using System;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;
using System.IO;
using IoPath = System.IO.Path;

namespace LunraGames.SubLight.Models
{
	public class SaveModel : Model
	{
		bool supportedVersion;
		string path;

		[JsonProperty] int version;
		[JsonProperty] string meta;
		[JsonProperty] Dictionary<string, string> metaKeyValues = new Dictionary<string, string>();
		[JsonProperty] DateTime created;
		[JsonProperty] DateTime modified;

		/// <summary>
		/// Gets the type of the save.
		/// </summary>
		/// <value>The type of the save.</value>
		[JsonProperty]
		public SaveTypes SaveType { get; protected set; }
		/// <summary>
		/// Is there a directory with the same name as this file next to it where it's saved?
		/// </summary>
		/// <value>True if it should have a sibling directory.</value>
		[JsonProperty]
		public bool HasSiblingDirectory { get; protected set; }

		/// <summary>
		/// Is this loadable, or is the version too old.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> SupportedVersion;
		/// <summary>
		/// The path of this save, depends on the SaveLoadService in use.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Path;

		/// <summary>
		/// The version of the app this was saved under.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Version;
		/// <summary>
		/// Typically used to store the name or some identifying data.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Meta;
		/// <summary>
		/// More identifying data.
		/// </summary>
		/// <remarks>
		/// Editing the dictionary returned won't modify the original one.
		/// </remarks>
		[JsonIgnore]
		public readonly ListenerProperty<Dictionary<string, string>> MetaKeyValues;
		/// <summary>
		/// When this was created and saved.
		/// </summary>
		/// <remarks>
		/// If this is equal to DateTime.MinValue it has never been saved.
		/// </remarks>
		[JsonIgnore]
		public readonly ListenerProperty<DateTime> Created;
		/// <summary>
		/// When this was last modified and saved.
		/// </summary>
		/// <remarks>
		/// If this is equal to DateTime.MinValue it has never been saved.
		/// </remarks>
		[JsonIgnore]
		public readonly ListenerProperty<DateTime> Modified;

		[JsonIgnore]
		public bool IsInternal { get { return Path.Value.StartsWith(Application.dataPath); } }
		[JsonIgnore]
		public string InternalPath
		{
			get
			{
				if (!IsInternal) return null;
				return "Assets" + Path.Value.Substring(Application.dataPath.Length);
			}
		}

		[JsonIgnore]
		public string SiblingDirectory
		{
			get
			{
				if (Path.Value == null || !HasSiblingDirectory) return null;
				return IoPath.Combine(Directory.GetParent(Path.Value).FullName, IoPath.GetFileNameWithoutExtension(Path.Value)+IoPath.DirectorySeparatorChar);
			}
		}
		[JsonIgnore]
		public string InternalSiblingDirectory
		{
			get
			{
				if (!HasSiblingDirectory || !IsInternal) return null;
				return "Assets" + SiblingDirectory.Substring(Application.dataPath.Length);
			}
		}

		public SaveModel()
		{
			SupportedVersion = new ListenerProperty<bool>(value => supportedVersion = value, () => supportedVersion);
			Path = new ListenerProperty<string>(value => path = value, () => path);
			Version = new ListenerProperty<int>(value => version = value, () => version);
			Meta = new ListenerProperty<string>(value => meta = value, () => meta);
			MetaKeyValues = new ListenerProperty<Dictionary<string, string>>(OnSetKeyValues, OnGetMetaKeyValues);
			Created = new ListenerProperty<DateTime>(value => created = value, () => created);
			Modified = new ListenerProperty<DateTime>(value => modified = value, () => modified);
		}

		#region Utility
		/// <summary>
		/// Sets the meta key. If set to null, the value is removed completely.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public string SetMetaKey(string key, string value)
		{
			if (key == null) throw new ArgumentNullException("key");

			string currentValue;
			if (metaKeyValues.TryGetValue(key, out currentValue))
			{
				if (value == null) OnRemoveKeyValue(key);
				else if (currentValue != value) OnSetKeyValue(key, value);
			}
			else if (value != null) OnSetKeyValue(key, value);

			return value;
		}

		/// <summary>
		/// Gets the meta key. If it doesn't exist, null is returned.
		/// </summary>
		/// <returns>The meta key.</returns>
		/// <param name="key">Key.</param>
		public string GetMetaKey(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			string currentValue;
			if (metaKeyValues.TryGetValue(key, out currentValue)) return currentValue;
			return null;
		}
		#endregion

		#region Events
		void OnSetKeyValues(Dictionary<string, string> newMetaKeyValues) { metaKeyValues = new Dictionary<string, string>(newMetaKeyValues); }
		Dictionary<string, string> OnGetMetaKeyValues() { return new Dictionary<string, string>(metaKeyValues); }

		void OnRemoveKeyValue(string key)
		{
			var newMetaKeyValues = MetaKeyValues.Value;
			newMetaKeyValues.Remove(key);
			MetaKeyValues.Value = newMetaKeyValues;
		}

		void OnSetKeyValue(string key, string value)
		{
			var newMetaKeyValues = MetaKeyValues.Value;
			newMetaKeyValues[key] = value;
			MetaKeyValues.Value = newMetaKeyValues;
		}
		#endregion
	}
}