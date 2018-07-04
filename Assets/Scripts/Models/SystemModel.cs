using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class SystemModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;
		[JsonProperty] string name;
		[JsonProperty] float rations;
		[JsonProperty] float fuel;
		[JsonProperty] float rationsDetection;
		[JsonProperty] float fuelDetection;

		[JsonProperty] StarBodyModel[] stars = new StarBodyModel[0];
		[JsonProperty] TerrestrialBodyModel[] terrestrials = new TerrestrialBodyModel[0];

		/// <summary>
		/// Gets the type of the system.
		/// </summary>
		/// <value>The type of the system.</value>
		[JsonIgnore]
		public abstract SystemTypes SystemType { get; }

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		[JsonIgnore]
		public readonly ListenerProperty<float> Fuel;
		[JsonIgnore]
		public readonly ListenerProperty<float> RationsDetection;
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelDetection;

		#region Derived
		[JsonIgnore]
		public readonly ListenerProperty<BodyModel[]> Bodies;
		#endregion

		public SystemModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel);
			RationsDetection = new ListenerProperty<float>(value => rationsDetection = value, () => rationsDetection);
			FuelDetection = new ListenerProperty<float>(value => fuelDetection = value, () => fuelDetection);

			Bodies = new ListenerProperty<BodyModel[]>(OnSetBodies, OnGetBodies);
		}

		#region Utility
		public BodyModel GetBody(int id)
		{
			return Bodies.Value.FirstOrDefault(b => b.BodyId.Value == id);
		}
		#endregion

		#region Events
		void OnSetBodies(BodyModel[] newBodies)
		{
			var starList = new List<StarBodyModel>();
			var terrestrialList = new List<TerrestrialBodyModel>();

			foreach (var body in newBodies)
			{
				switch (body.BodyType)
				{
					case BodyTypes.Star:
						starList.Add(body as StarBodyModel);
						break;
					case BodyTypes.Terrestrial:
						terrestrialList.Add(body as TerrestrialBodyModel);
						break;
					default:
						Debug.LogError("Unrecognized BodyType: " + body.BodyType);
						break;
				}
			}

			stars = starList.ToArray();
			terrestrials = terrestrialList.ToArray();
		}

		BodyModel[] OnGetBodies()
		{
			return stars.Cast<BodyModel>().Concat(terrestrials)
										  .ToArray();
		}
		#endregion
	}
}