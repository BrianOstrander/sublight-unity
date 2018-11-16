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
		[JsonProperty] bool playerStartSelected;
		[JsonProperty] ShipModel ship;
		[JsonProperty] float destructionSpeedIncrement;
		[JsonProperty] float destructionSpeed;
		[JsonProperty] float destructionRadius;
		[JsonProperty] DestructionSpeedDelta[] destructionSpeedDeltas = new DestructionSpeedDelta[0];
		[JsonProperty] EncounterStatus[] encounterStatuses = new EncounterStatus[0];
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonProperty] FinalReportModel[] finalReports = new FinalReportModel[0];
		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonProperty] ToolbarSelections toolbarSelection;

		[JsonProperty] float universeUnitsPerUnityUnit;
		[JsonProperty] FocusTransform focusTransform;

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
		/// Has the player start been selected yet? If not the ship position will still be zero.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> PlayerStartSelected;
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
		public readonly ListenerProperty<DestructionSpeedDelta[]> DestructionSpeedDeltas;

		/// <summary>
		/// The encounters seen, completed or otherwise.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<EncounterStatus[]> EncounterStatuses;

		[JsonIgnore]
		public readonly ListenerProperty<ToolbarSelections> ToolbarSelection;

		[JsonIgnore]
		public readonly ListenerProperty<FocusTransform> FocusTransform;
		[JsonIgnore]
		public readonly ListenerProperty<float> UniverseUnitsPerUnityUnit;

		[JsonProperty] string galaxyId;
		[JsonProperty] string galaxyTargetId;
		[JsonProperty] UniverseModel universe;

		[JsonIgnore]
		public UniverseModel Universe
		{
			get { return universe; }
			set { universe = value; }
		}

		[JsonIgnore]
		public string GalaxyId
		{
			get { return galaxyId; }
			set { galaxyId = value; }
		}

		[JsonIgnore]
		public string GalaxyTargetId
		{
			get { return galaxyTargetId; }
			set { galaxyTargetId = value; }
		}

		[JsonIgnore]
		public GalaxyInfoModel Galaxy { get; set; }
		[JsonIgnore]
		public GalaxyInfoModel GalaxyTarget { get; set; }
		#endregion

		#region NonSerialized
		SaveStateBlock saveState = SaveStateBlock.Savable();
		CameraTransformRequest cameraTransform = CameraTransformRequest.Default;
		CelestialSystemStateBlock celestialSystemState = CelestialSystemStateBlock.Default;

		[JsonIgnore]
		public readonly ListenerProperty<SaveStateBlock> SaveState;
		[JsonIgnore]
		public readonly ListenerProperty<CameraTransformRequest> CameraTransform;
		[JsonIgnore]
		public readonly ListenerProperty<CelestialSystemStateBlock> CelestialSystemState;

		CelestialSystemStateBlock celestialSystemStateLastSelected = CelestialSystemStateBlock.Default;

		[JsonIgnore]
		public CelestialSystemStateBlock CelestialSystemStateLastSelected { get { return celestialSystemStateLastSelected; } }
		#endregion

		public GameModel()
		{
			SaveType = SaveTypes.Game;
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			DayTime = new ListenerProperty<DayTime>(value => dayTime = value, () => dayTime);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			PlayerStartSelected = new ListenerProperty<bool>(value => playerStartSelected = value, () => playerStartSelected);
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			DestructionSpeedIncrement = new ListenerProperty<float>(value => destructionSpeedIncrement = value, () => destructionSpeedIncrement);
			DestructionSpeed = new ListenerProperty<float>(value => destructionSpeed = value, () => destructionSpeed);
			DestructionRadius = new ListenerProperty<float>(value => destructionRadius = value, () => destructionRadius);
			DestructionSpeedDeltas = new ListenerProperty<DestructionSpeedDelta[]>(value => destructionSpeedDeltas = value, () => destructionSpeedDeltas);
			EncounterStatuses = new ListenerProperty<EncounterStatus[]>(value => encounterStatuses = value, () => encounterStatuses);
			ToolbarSelection = new ListenerProperty<ToolbarSelections>(value => toolbarSelection = value, () => toolbarSelection);
			FocusTransform = new ListenerProperty<FocusTransform>(value => focusTransform = value, () => focusTransform);

			SaveState = new ListenerProperty<SaveStateBlock>(value => saveState = value, () => saveState);
			CameraTransform = new ListenerProperty<CameraTransformRequest>(value => cameraTransform = value, () => cameraTransform);
			CelestialSystemState = new ListenerProperty<CelestialSystemStateBlock>(value => celestialSystemState = value, () => celestialSystemState, OnCelestialSystemState);
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
			switch (scale)
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

		[JsonIgnore]
		public UniverseScaleModel ActiveScale
		{
			get
			{
				foreach (var scaleEnum in EnumExtensions.GetValues(UniverseScales.Unknown))
				{
					var curr = GetScale(scaleEnum);
					if (curr.IsActive) return curr;
				}
				return null;
			}
		}

		#endregion

		#region Events
		void OnCelestialSystemState(CelestialSystemStateBlock block)
		{
			switch (block.State)
			{
				case CelestialSystemStateBlock.States.UnSelected:
				case CelestialSystemStateBlock.States.Selected:
					celestialSystemStateLastSelected = block;
					break;
			}
		}
		#endregion
	}
}