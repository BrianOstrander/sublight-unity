using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class ValueFilterEntryModel : Model, IValueFilterEntryModel
	{
		[JsonProperty] int index;
		/// <summary>
		/// Used internally to determine how these are displayed.
		/// </summary>
		[JsonIgnore]
		public ListenerProperty<int> Index;

		[JsonProperty] bool ignore;
		[JsonIgnore] public ListenerProperty<bool> Ignore;

		[JsonProperty] string filterId;
		[JsonIgnore] public ListenerProperty<string> FilterId;

		[JsonProperty] ValueFilterGroups group;
		[JsonIgnore] public ListenerProperty<ValueFilterGroups> Group;

		[JsonProperty] bool negate;
		[JsonIgnore] public ListenerProperty<bool> Negate;

		[JsonIgnore]
		public int FilterIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}
		[JsonIgnore]
		public bool FilterIgnore
		{
			get { return Ignore.Value; }
			set { Ignore.Value = value; }
		}
		[JsonIgnore]
		public string FilterIdValue
		{
			get { return FilterId.Value; }
			set { FilterId.Value = value; }
		}
		[JsonIgnore]
		public ValueFilterGroups FilterGroup
		{
			get { return Group.Value; }
			set { Group.Value = value; }
		}
		[JsonIgnore]
		public bool FilterNegate
		{
			get { return Negate.Value; }
			set { Negate.Value = value; }
		}

		[JsonIgnore]
		public abstract ValueFilterTypes FilterType { get; }
		[JsonIgnore]
		public abstract KeyValueTypes FilterValueType { get; }

		public ValueFilterEntryModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			FilterId = new ListenerProperty<string>(value => filterId = value, () => filterId);
			Group = new ListenerProperty<ValueFilterGroups>(value => group = value, () => group);
			Negate = new ListenerProperty<bool>(value => negate = value, () => negate);
		}
	}

	public abstract class ValueFilterEntryModel<T> : ValueFilterEntryModel
	{
		[JsonProperty] T filterValue;
		[JsonIgnore] public ListenerProperty<T> FilterValue;

		public ValueFilterEntryModel()
		{
			FilterValue = new ListenerProperty<T>(value => filterValue = value, () => filterValue);
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
		KeyValueTypes FilterValueType { get; }
	}
}