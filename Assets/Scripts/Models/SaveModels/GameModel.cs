using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
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
		[JsonProperty] EncounterStatus[] encounterStatuses = new EncounterStatus[0];
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonProperty] FinalReportModel[] finalReports = new FinalReportModel[0];
		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonProperty] ToolbarSelections toolbarSelection;

		[JsonProperty] float universeUnitsPerUnityUnit;
		[JsonProperty] ZoomBlock zoom;

		[JsonProperty] UniverseScaleModel scaleSystem = UniverseScaleModel.Create(UniverseScales.System);
		[JsonProperty] UniverseScaleModel scaleLocal = UniverseScaleModel.Create(UniverseScales.Local);
		[JsonProperty] UniverseScaleModel scaleStellar = UniverseScaleModel.Create(UniverseScales.Stellar);
		[JsonProperty] UniverseScaleModel scaleQuadrant = UniverseScaleModel.Create(UniverseScales.Quadrant);
		[JsonProperty] UniverseScaleModel scaleGalactic = UniverseScaleModel.Create(UniverseScales.Galactic);
		[JsonProperty] UniverseScaleModel scaleCluster = UniverseScaleModel.Create(UniverseScales.Cluster);

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

		/// <summary>
		/// The encounters seen, completed or otherwise.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<EncounterStatus[]> EncounterStatuses;

		[JsonIgnore]
		public readonly ListenerProperty<ToolbarSelections> ToolbarSelection;

		[JsonIgnore]
		public readonly ListenerProperty<ZoomBlock> Zoom;
		[JsonIgnore]
		public readonly ListenerProperty<float> UniverseUnitsPerUnityUnit;
		#endregion

		#region NonSerialized
		SaveStateBlock saveState = SaveStateBlock.Savable();
		UniversePosition[] focusedSectors = new UniversePosition[0];

		/// <summary>
		/// Positions of all loaded sectors.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition[]> FocusedSectors;
		[JsonIgnore]
		public readonly ListenerProperty<SaveStateBlock> SaveState;
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
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			DestructionSpeedIncrement = new ListenerProperty<float>(value => destructionSpeedIncrement = value, () => destructionSpeedIncrement);
			DestructionSpeed = new ListenerProperty<float>(value => destructionSpeed = value, () => destructionSpeed);
			DestructionRadius = new ListenerProperty<float>(value => destructionRadius = value, () => destructionRadius);
			TravelRequest = new ListenerProperty<TravelRequest>(value => travelRequest = value, () => travelRequest);
			DestructionSpeedDeltas = new ListenerProperty<DestructionSpeedDelta[]>(value => destructionSpeedDeltas = value, () => destructionSpeedDeltas);
			EncounterStatuses = new ListenerProperty<EncounterStatus[]>(value => encounterStatuses = value, () => encounterStatuses);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
			Zoom = new ListenerProperty<ZoomBlock>(value => zoom = value, () => zoom);

			FocusedSectors = new ListenerProperty<UniversePosition[]>(value => focusedSectors = value, () => focusedSectors);
			SaveState = new ListenerProperty<SaveStateBlock>(value => saveState = value, () => saveState);
			UniverseUnitsPerUnityUnit = new ListenerProperty<float>(value => universeUnitsPerUnityUnit = value, () => universeUnitsPerUnityUnit);
		}

		#region Events

		#endregion

		#region Utility
		public void SetEncounterStatus(EncounterStatus status)
		{
			if (status.Encounter == null)
			{
				Debug.LogError("Cannot update the status of an encounter with a null id, update ignored.");
				return;
			}
			EncounterStatuses.Value = EncounterStatuses.Value.Where(e => e.Encounter != status.Encounter).Append(status).ToArray();
		}

		public EncounterStatus GetEncounterStatus(string encounterId)
		{
			return EncounterStatuses.Value.FirstOrDefault(e => e.Encounter == encounterId);
		}

		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		public void AddFinalReport(FinalReportModel finalReport)
		{
			if (finalReports.FirstOrDefault(r => r.Encounter.Value == finalReport.Encounter.Value) != null)
			{
				Debug.LogError("A final report with EncounterId " + finalReport.Encounter.Value + " already exists.");
				return;
			}
			finalReports = finalReports.Append(finalReport).ToArray();
		}

		public FinalReportModel GetFinalReport(string encounter)
		{
			return finalReports.FirstOrDefault(r => r.Encounter.Value == encounter);
		}

		[JsonIgnore]
		public EncyclopediaListModel Encyclopedia { get { return encyclopedia; } }

		public UniverseScaleModel GetScale(UniverseScales scale)
		{
			switch(scale)
			{
				case UniverseScales.System: return scaleSystem;
				case UniverseScales.Local: return scaleLocal;
				case UniverseScales.Stellar: return scaleStellar;
				case UniverseScales.Quadrant: return scaleQuadrant;
				case UniverseScales.Galactic: return scaleGalactic;
				case UniverseScales.Cluster: return scaleCluster;
				default:
					Debug.LogError("Unrecognized scale: " + scale);
					return null;
			}
		}
		#endregion
	}
}