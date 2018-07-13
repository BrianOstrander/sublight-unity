using Newtonsoft.Json;

using UnityEngine;

using Focuses = LunraGames.SpaceFarm.FocusRequest.Focuses;

namespace LunraGames.SpaceFarm.Models
{
	public class GameModel : SaveModel
	{
		#region Serialized
		[JsonProperty] int seed;
		[JsonProperty] DayTime dayTime;
		[JsonProperty] float speed;
		[JsonProperty] UniverseModel universe;
		[JsonProperty] UniversePosition endSystem;
		[JsonProperty] UniversePosition focusedSector;
		[JsonProperty] ShipModel ship;
		[JsonProperty] float destructionSpeedIncrement;
		[JsonProperty] float destructionSpeed;
		[JsonProperty] float destructionRadius;
		[JsonProperty] TravelRequest travelRequest;
		[JsonProperty] DestructionSpeedDelta[] destructionSpeedDeltas = new DestructionSpeedDelta[0];
		
		[JsonProperty] string[] encountersSeen = new string[0];

		[JsonProperty] GalaxyFocusRequest galaxyFocus;
		[JsonProperty] SystemBodiesFocusRequest systemBodiesFocus;
		[JsonProperty] SystemsFocusRequest systemsFocus;
		[JsonProperty] BodyFocusRequest bodyFocus;
		[JsonProperty] EncounterFocusRequest encounterFocus;

		/// <summary>
		/// The game seed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		/// <summary>
		/// The day time.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> DayTime;
		/// <summary>
		/// The speed of the ship, in universe units per day, whether or not
		/// it's curently in motion.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Speed;
		/// <summary>
		/// The game universe.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniverseModel> Universe;
		/// <summary>
		/// The target system the player is traveling to.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> EndSystem;
		/// <summary>
		/// The sector the camera is looking at.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> FocusedSector;
		/// <summary>
		/// The game ship.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<ShipModel> Ship;
		/// <summary>
		/// The destruction speed increments.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> DestructionSpeedIncrement;
		/// <summary>
		/// The speed at which the destruction expands, in universe units per
		/// day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> DestructionSpeed;
		/// <summary>
		/// The total destruction radius, in universe units.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> DestructionRadius;
		[JsonIgnore]
		public readonly ListenerProperty<TravelRequest> TravelRequest;
		[JsonIgnore]
		public readonly ListenerProperty<DestructionSpeedDelta[]> DestructionSpeedDeltas;

		[JsonIgnore]
		public readonly ListenerProperty<FocusRequest> FocusRequest;

		/// <summary>
		/// The encounters seen, completed or otherwise.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string[]> EncountersSeen;
		#endregion

		#region NonSerialized
		UniversePosition[] focusedSectors = new UniversePosition[0];

		/// <summary>
		/// Positions of all loaded sectors.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition[]> FocusedSectors;
		#endregion

		public GameModel()
		{
			SaveType = SaveTypes.Game;
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			DayTime = new ListenerProperty<DayTime>(value => dayTime = value, () => dayTime);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			Universe = new ListenerProperty<UniverseModel>(value => universe = value, () => universe);
			EndSystem = new ListenerProperty<UniversePosition>(value => endSystem = value, () => endSystem);
			FocusedSector = new ListenerProperty<UniversePosition>(value => focusedSector = value, () => focusedSector);
			FocusedSectors = new ListenerProperty<UniversePosition[]>(value => focusedSectors = value, () => focusedSectors);
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			DestructionSpeedIncrement = new ListenerProperty<float>(value => destructionSpeedIncrement = value, () => destructionSpeedIncrement);
			DestructionSpeed = new ListenerProperty<float>(value => destructionSpeed = value, () => destructionSpeed);
			DestructionRadius = new ListenerProperty<float>(value => destructionRadius = value, () => destructionRadius);
			TravelRequest = new ListenerProperty<TravelRequest>(value => travelRequest = value, () => travelRequest);
			DestructionSpeedDeltas = new ListenerProperty<DestructionSpeedDelta[]>(value => destructionSpeedDeltas = value, () => destructionSpeedDeltas);

			EncountersSeen = new ListenerProperty<string[]>(value => encountersSeen = value, () => encountersSeen);
			
			FocusRequest = new ListenerProperty<FocusRequest>(OnSetFocus, OnGetFocus);
		}

		#region Events
		void OnSetFocus(FocusRequest focus)
		{
			galaxyFocus = null;
			systemBodiesFocus = null;
			systemsFocus = null;
			bodyFocus = null;
			encounterFocus = null;

			switch (focus.Focus)
			{
				case Focuses.Galaxy:
					galaxyFocus = focus as GalaxyFocusRequest;
					break;
				case Focuses.SystemBodies:
					systemBodiesFocus = focus as SystemBodiesFocusRequest;
					break;
				case Focuses.Systems:
					systemsFocus = focus as SystemsFocusRequest;
					break;
				case Focuses.Body:
					bodyFocus = focus as BodyFocusRequest;
					break;
				case Focuses.Encounter:
					encounterFocus = focus as EncounterFocusRequest;
					break;
				default:
					Debug.LogError("Unrecognized Focus: " + focus.Focus);
					break;
			}
		}

		FocusRequest OnGetFocus()
		{
			if (galaxyFocus != null) return galaxyFocus;
			if (systemBodiesFocus != null) return systemBodiesFocus;
			if (systemsFocus != null) return systemsFocus;
			if (bodyFocus != null) return bodyFocus;
			if (encounterFocus != null) return encounterFocus;

			return null;
		}
		#endregion
	}
}