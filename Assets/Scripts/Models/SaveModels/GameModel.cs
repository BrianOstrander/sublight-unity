using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight.Models
{
	public class GameModel : SaveModel
	{
		#region Serialized
		[JsonProperty] int seed;
		[JsonProperty] DayTime dayTime;
		[JsonProperty] ShipModel ship;
		[JsonProperty] float destructionSpeedIncrement; // TODO: move to a destruction model
		[JsonProperty] float destructionSpeed; // TODO: move to a destruction model
		[JsonProperty] float destructionRadius; // TODO: move to a destruction model
		[JsonProperty] DestructionSpeedDelta[] destructionSpeedDeltas = new DestructionSpeedDelta[0]; // TODO: move to a destruction model
		[JsonProperty] EncounterStatus[] encounterStatuses = new EncounterStatus[0];
		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonProperty] EncyclopediaListModel encyclopedia = new EncyclopediaListModel();
		[JsonProperty] ToolbarSelections toolbarSelection;

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