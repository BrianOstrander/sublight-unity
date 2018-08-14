using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class ValueFilterEntryModel<T> : Model, IValueFilterEntryModel
	{
		[JsonProperty] int index;
		[JsonProperty] bool ignore;
		[JsonProperty] string filterId;
		[JsonProperty] ValueFilterGroups group;
		[JsonProperty] T filterValue;
		[JsonProperty] bool negate;

		/// <summary>
		/// Used internally to determine how these are displayed.
		/// </summary>
		[JsonIgnore]
		public ListenerProperty<int> Index;
		[JsonIgnore]
		public ListenerProperty<bool> Ignore;
		[JsonIgnore]
		public ListenerProperty<string> FilterId;
		[JsonIgnore]
		public ListenerProperty<ValueFilterGroups> Group;
		[JsonIgnore]
		public ListenerProperty<T> FilterValue;
		[JsonIgnore]
		public ListenerProperty<bool> Negate;

		public int FilterIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}
		public bool FilterIgnore
		{
			get { return Ignore.Value; }
			set { Ignore.Value = value; }
		}
		public string FilterIdValue
		{
			get { return FilterId.Value; }
			set { FilterId.Value = value; }
		}
		public ValueFilterGroups FilterGroup
		{
			get { return Group.Value; }
			set { Group.Value = value; }
		}
		public bool FilterNegate
		{
			get { return Negate.Value; }
			set { Negate.Value = value; }
		}
		public abstract ValueFilterTypes FilterType { get; }

		public ValueFilterEntryModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			FilterId = new ListenerProperty<string>(value => filterId = value, () => filterId);
			Group = new ListenerProperty<ValueFilterGroups>(value => group = value, () => group);
			FilterValue = new ListenerProperty<T>(value => filterValue = value, () => filterValue);
			Negate = new ListenerProperty<bool>(value => negate = value, () => negate);
		}
	}

	public interface IValueFilterEntryModel : IModel
	{
		int FilterIndex { get; set; }
		bool FilterIgnore { get; set; }
		string FilterIdValue { get; set; }
		ValueFilterGroups FilterGroup { get; set; }
		bool FilterNegate { get; set; }
		ValueFilterTypes FilterType { get; }
	}
}