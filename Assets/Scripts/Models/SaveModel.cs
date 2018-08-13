using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SaveModel : Model
	{
		bool supportedVersion;
		string path;

		[JsonProperty] int version;
		[JsonProperty] string meta;
		[JsonProperty] DateTime created;
		[JsonProperty] DateTime modified;

		/// <summary>
		/// Gets the type of the save.
		/// </summary>
		/// <value>The type of the save.</value>
		[JsonProperty]
		public SaveTypes SaveType { get; protected set; }

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
		/// The name.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Meta;
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
				return "Assets"+Path.Value.Substring(Application.dataPath.Length);
			}
		}

		public SaveModel()
		{
			SupportedVersion = new ListenerProperty<bool>(value => supportedVersion = value, () => supportedVersion);
			Path = new ListenerProperty<string>(value => path = value, () => path);
			Version = new ListenerProperty<int>(value => version = value, () => version);
			Meta = new ListenerProperty<string>(value => meta = value, () => meta);
			Created = new ListenerProperty<DateTime>(value => created = value, () => created);
			Modified= new ListenerProperty<DateTime>(value => modified = value, () => modified);
		}
	}
}