using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEdgeModel : IModel
	{
		string EdgeName { get; }
		int EdgeIndex { get; set; }
		string EdgeId { get; set; }
	}

	public abstract class EdgeModel : Model, IEdgeModel
	{
		[JsonProperty] int index;
		[JsonProperty] bool ignore;

		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;

		public EdgeModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
		}

		[JsonIgnore]
		public abstract EdgeEntryModel RawEntry { get; }
		[JsonIgnore]
		public abstract string EdgeName { get; }

		[JsonIgnore]
		public int EdgeIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}

		[JsonIgnore]
		public string EdgeId
		{
			get { return RawEntry == null ? null : RawEntry.EntryId.Value; }
			set
			{
				if (RawEntry == null)
				{
					Debug.LogError("Unable to set the EdgeId of a null entry");
					return;
				}
				RawEntry.EntryId.Value = value;
			}
		}
	}
}
