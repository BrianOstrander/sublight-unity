﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class BodyModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] int bodyId;
		[JsonProperty] int parentId;
		[JsonProperty] string name;
		[JsonProperty] string encounter;

		[JsonProperty] ResourceInventoryModel resources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel resourcesAcquired = ResourceInventoryModel.Zero;

		/// <summary>
		/// Gets the type of the body.
		/// </summary>
		/// <value>The type of the body.</value>
		[JsonIgnore]
		public abstract BodyTypes BodyType { get; }

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<int> BodyId;
		[JsonIgnore]
		public readonly ListenerProperty<int> ParentId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Encounter;

		[JsonIgnore]
		public ResourceInventoryModel Resources { get { return resources; } }
		[JsonIgnore]
		public ResourceInventoryModel ResourcesAcquired { get { return resourcesAcquired; } }
		[JsonIgnore]
		public ResourceInventoryModel ResourcesCurrent { get { return Resources.Duplicate.Subtract(ResourcesAcquired); } }

		public BodyModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			BodyId = new ListenerProperty<int>(value => bodyId = value, () => bodyId);
			ParentId = new ListenerProperty<int>(value => parentId = value, () => parentId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Encounter = new ListenerProperty<string>(value => encounter = value, () => encounter);
		}

		#region Utility
		[JsonIgnore]
		public bool HasEncounter { get { return !string.IsNullOrEmpty(Encounter); } }
		#endregion
	}
}