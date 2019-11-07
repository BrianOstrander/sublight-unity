using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class EdgeModel : Model
	{
		protected const float DefaultIndent = 32f;

		[JsonProperty] int index;
		[JsonIgnore] public readonly ListenerProperty<int> Index;
		[JsonProperty] bool ignore;
		[JsonIgnore] public readonly ListenerProperty<bool> Ignore;

		public EdgeModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
		}

		[JsonIgnore]
		public virtual string EdgeName { get; }

		[JsonIgnore]
		public int EdgeIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}

		[JsonIgnore]
		public string EdgeId // TODO: Remove this...
		{
			get { return Id.Value; }
			set { Id.Value = value; }
		}

		[JsonIgnore]
		public bool EdgeIgnore
		{
			get { return Ignore.Value; }
			set { Ignore.Value = value; }
		}

		[JsonIgnore]
		public virtual float EdgeIndent { get { return 0f; } }
	}
}
