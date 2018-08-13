using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	// TODO: Unclear if we need this?
	public interface IModel 
	{
		ListenerProperty<string> Id { get; }
	}

	[Serializable]
	public abstract class Model : IModel
	{
		// TODO: Figure out what this is supposed to mean and if it's actually needed...
		[JsonProperty] string id;

		[JsonIgnore]
		readonly ListenerProperty<string> idListener;
		[JsonIgnore]
		public ListenerProperty<string> Id { get { return idListener; } }

		public Model()
		{
			idListener = new ListenerProperty<string>(value => id = value, () => id);
		}
	}
}
