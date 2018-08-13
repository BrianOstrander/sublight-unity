using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class ValueFilterEntryModel<T> : Model, IValueFilterEntryModel
	{
		[JsonProperty] ValueFilterGroups group;
		[JsonProperty] T filterValue;
		[JsonProperty] bool negate;

		[JsonIgnore]
		public ListenerProperty<ValueFilterGroups> Group;
		[JsonIgnore]
		public ListenerProperty<T> FilterValue;
		[JsonIgnore]
		public ListenerProperty<bool> Negate;

		public ValueFilterGroups FilterGroup { get { return Group.Value; } }
		public bool FilterNegate { get { return Negate.Value; } }
		public abstract ValueFilterTypes FilterType { get; }

		public ValueFilterEntryModel()
		{
			Group = new ListenerProperty<ValueFilterGroups>(value => group = value, () => group);
			FilterValue = new ListenerProperty<T>(value => filterValue = value, () => filterValue);
			Negate = new ListenerProperty<bool>(value => negate = value, () => negate);
		}
	}

	public interface IValueFilterEntryModel : IModel
	{
		ValueFilterGroups FilterGroup { get; }
		bool FilterNegate { get; }
		ValueFilterTypes FilterType { get; }
	}
}