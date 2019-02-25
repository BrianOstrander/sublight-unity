using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

using LunraGames;

namespace LunraGames.SubLight.Models
{
	public abstract class GalaxyBaseModel : SaveModel
	{
		public static class TextureNames
		{
			public const string BodyMap = "bodymap";
			public const string Preview = "preview";
			public const string FullPreview = "fullpreview";
			public const string Details = "details";
		}

		[JsonProperty] bool isPlayable;
		[JsonProperty] string galaxyId;
		[JsonProperty] string name;
		[JsonProperty] string description;

		[JsonProperty] Vector3 universeNormal;
		[JsonProperty] float alertHeightMultiplier;

		[JsonProperty] UniversePosition clusterOrigin;
		[JsonProperty] UniversePosition galaxyOrigin;

		[JsonProperty] int minimumSectorSystemCount;
		[JsonProperty] int maximumSectorSystemCount;
		[JsonProperty] AnimationCurve sectorSystemChance = AnimationCurveExtensions.Constant(1f);

		[JsonProperty] string encyclopediaEntryId;

		[JsonProperty] GalaxyLabelModel[] labels = new GalaxyLabelModel[0];
		[JsonProperty] SectorModel[] specifiedSectors = new SectorModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<bool> IsPlayable;
		[JsonIgnore]
		public readonly ListenerProperty<string> GalaxyId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;

		[JsonIgnore]
		public readonly ListenerProperty<Vector3> UniverseNormal;
		[JsonIgnore]
		public readonly ListenerProperty<float> AlertHeightMultiplier;

		[JsonIgnore]
		public readonly ListenerProperty<int> MinimumSectorSystemCount;
		[JsonIgnore]
		public readonly ListenerProperty<int> MaximumSectorSystemCount;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> SectorSystemChance;

		[JsonIgnore]
		public readonly ListenerProperty<string> EncyclopediaEntryId;

		[JsonProperty] int galaxyRadius;
		[JsonProperty] UniversePosition galaxySize;

		/// <summary>
		/// Gets or sets the galaxy radius in sectors.
		/// </summary>
		/// <value>The galaxy radius.</value>
		[JsonIgnore]
		public int GalaxyRadius
		{
			get { return galaxyRadius; }
			set
			{
				galaxyRadius = value;
				var galaxySectorDiameter = value * 2;
				galaxySize = new UniversePosition(new Vector3Int(galaxySectorDiameter, galaxySectorDiameter, galaxySectorDiameter));
			}
		}

		/// <summary>
		/// Gets the size of the galaxy in sectors.
		/// </summary>
		/// <value>The size of the galaxy.</value>
		[JsonIgnore]
		public UniversePosition GalaxySize { get { return galaxySize; } }

		[JsonIgnore]
		public Vector3 ClusterOriginNormal
		{
			get { return UniversePosition.NormalizedSector(clusterOrigin, galaxySize); }
			set { clusterOrigin = UniversePositionFromNormal(value); }
		}

		[JsonIgnore]
		public Vector3 GalaxyOriginNormal
		{
			get { return UniversePosition.NormalizedSector(galaxyOrigin, galaxySize); }
			set { galaxyOrigin = UniversePositionFromNormal(value); }
		}

		[JsonIgnore]
		public Vector3 PlayerBeginNormal
		{
			get { return UniversePosition.NormalizedSector(GetPlayerBegin(), galaxySize); }
			set { SetPlayerBegin(UniversePositionFromNormal(value)); }
		}

		[JsonIgnore]
		public Vector3 PlayerEndNormal
		{
			get { return UniversePosition.NormalizedSector(GetPlayerEnd(), galaxySize); }
			set { SetPlayerEnd(UniversePositionFromNormal(value)); }
		}

		[JsonIgnore]
		public UniversePosition ClusterOrigin
		{
			get { return clusterOrigin; }
			set { clusterOrigin = value; }
		}

		[JsonIgnore]
		public UniversePosition GalaxyOrigin
		{
			get { return galaxyOrigin; }
			set { galaxyOrigin = value; }
		}

		public GalaxyBaseModel()
		{
			IsPlayable = new ListenerProperty<bool>(value => isPlayable = value, () => isPlayable);
			GalaxyId = new ListenerProperty<string>(value => galaxyId = value, () => galaxyId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);

			UniverseNormal = new ListenerProperty<Vector3>(value => universeNormal = value, () => universeNormal);
			AlertHeightMultiplier = new ListenerProperty<float>(value => alertHeightMultiplier = value, () => alertHeightMultiplier);

			MinimumSectorSystemCount = new ListenerProperty<int>(value => minimumSectorSystemCount = value, () => minimumSectorSystemCount);
			MaximumSectorSystemCount = new ListenerProperty<int>(value => maximumSectorSystemCount = value, () => maximumSectorSystemCount);
			SectorSystemChance = new ListenerProperty<AnimationCurve>(value => sectorSystemChance = value, () => sectorSystemChance);

			EncyclopediaEntryId = new ListenerProperty<string>(value => encyclopediaEntryId = value, () => encyclopediaEntryId);
		}

		protected override void OnPrepareTexture(string name, Texture2D texture)
		{
			// TODO: This should probably be exposed by some interface, oh well...
			switch (name)
			{
				case TextureNames.Preview:
					texture.anisoLevel = 2;
					break;
				case TextureNames.Details:
				case TextureNames.FullPreview:
					texture.anisoLevel = 4;
					break;
			}
		}

		#region Private Utility
		UniversePosition UniversePositionFromNormal(Vector3 normal)
		{
			var maxDiameter = (GalaxyRadius * 2) - 1;
			return new UniversePosition(new Vector3(normal.x * maxDiameter, normal.y * maxDiameter, normal.z * maxDiameter));
		}
		#endregion

		#region Utility
		public UniversePosition GetPlayerBegin()
		{
			var found = false;
			return GetPlayerBegin(out found);
		}

		public UniversePosition GetPlayerEnd()
		{
			var found = false;
			return GetPlayerEnd(out found);
		}

		public UniversePosition GetPlayerBegin(out bool found)
		{
			SectorModel sector = null;
			SystemModel system = null;
			return GetPlayerBegin(out found, out sector, out system);
		}

		public UniversePosition GetPlayerEnd(out bool found)
		{
			SectorModel sector = null;
			SystemModel system = null;
			return GetPlayerEnd(out found, out sector, out system);
		}

		public UniversePosition GetPlayerBegin(out bool found, out SectorModel sector, out SystemModel system)
		{
			found = false;
			sector = null;
			system = null;

			foreach (var currSector in specifiedSectors)
			{
				foreach (var currSystem in currSector.Systems.Value)
				{
					found = currSystem.PlayerBegin.Value;
					if (found)
					{
						sector = currSector;
						system = currSystem;
						return system.Position.Value;
					}
				}
			}

			return UniversePosition.Zero;
		}

		public UniversePosition GetPlayerEnd(out bool found, out SectorModel sector, out SystemModel system)
		{
			found = false;
			sector = null;
			system = null;

			foreach (var currSector in specifiedSectors)
			{
				foreach (var currSystem in currSector.Systems.Value)
				{
					found = currSystem.PlayerEnd.Value;
					if (found)
					{
						sector = currSector;
						system = currSystem;
						return system.Position.Value;
					}
				}
			}

			return UniversePosition.Zero;
		}

		public void SetPlayerBegin(UniversePosition position)
		{
			var found = false;
			SectorModel sector = null;
			SystemModel system = null;
			GetPlayerBegin(out found, out sector, out system);
			if (found) sector.Position.Value = position.LocalZero;
		}

		public void SetPlayerEnd(UniversePosition position)
		{
			var found = false;
			SectorModel sector = null;
			SystemModel system = null;
			GetPlayerEnd(out found, out sector, out system);
			if (found) sector.Position.Value = position.LocalZero;
		}

		public void AddLabel(GalaxyLabelModel label)
		{
			if (label == null) throw new ArgumentNullException("label");
			if (labels.Contains(label))
			{
				Debug.LogError("An identical label already exists");
				return;
			}
			var identicalLabel = GetLabel(label.LabelId.Value);
			if (identicalLabel != null)
			{
				Debug.LogError("A label with id \"" + label.LabelId.Value + "\" already exists");
				return;
			}
			labels = labels.Append(label).ToArray();
		}

		public void RemoveLabel(GalaxyLabelModel label)
		{
			if (label == null) throw new ArgumentNullException("label");
			if (!labels.Contains(label))
			{
				Debug.LogError("No label found to remove");
				return;
			}
			labels = labels.ExceptOne(label).ToArray();
		}

		public void RemoveLabel(string labelId)
		{
			var toRemove = labels.Where(l => l.LabelId.Value == labelId);
			if (toRemove.None())
			{
				Debug.LogError("No label with id \"" + labelId + "\" found to remove");
				return;
			}
			labels = labels.Except(toRemove).ToArray();
		}

		public GalaxyLabelModel GetLabel(string labelId)
		{
			return labels.FirstOrDefault(l => l.LabelId.Value == labelId);
		}

		public GalaxyLabelModel[] GetLabels() { return labels.ToArray(); }

		public GalaxyLabelModel[] GetLabels(params UniverseScales[] scales)
		{
			return labels.Where(l => scales.Contains(l.Scale)).ToArray();
		}

		public void AddSpecifiedSector(SectorModel sector)
		{
			if (sector == null) throw new ArgumentNullException("sector");
			if (specifiedSectors.Contains(sector))
			{
				Debug.LogError("An identical sector already exists");
				return;
			}
			var identicalSector = GetSpecifiedSector(sector.Name);
			if (identicalSector != null)
			{
				Debug.LogError("A sector with name \"" + sector.Name.Value + "\" already exists");
				return;
			}
			specifiedSectors = specifiedSectors.Append(sector).ToArray();
		}

		public void RemoveSpecifiedSector(SectorModel sector)
		{
			if (sector == null) throw new ArgumentNullException("sector");
			if (!specifiedSectors.Contains(sector))
			{
				Debug.LogError("No sector found to remove");
				return;
			}
			specifiedSectors = specifiedSectors.ExceptOne(sector).ToArray();
		}

		public void RemoveSpecifiedSector(string name)
		{
			var toRemove = specifiedSectors.Where(s => s.Name.Value == name);
			if (toRemove.None())
			{
				Debug.LogError("No sector with name \"" + name + "\" found to remove");
				return;
			}
			specifiedSectors = specifiedSectors.Except(toRemove).ToArray();
		}

		public SectorModel GetSpecifiedSector(string name)
		{
			return specifiedSectors.FirstOrDefault(s => s.Name.Value == name);
		}

		public SectorModel[] GetSpecifiedSectors() { return specifiedSectors.ToArray(); }

		public int SectorBodyCount(float value)
		{
			var delta = (MaximumSectorSystemCount.Value - MinimumSectorSystemCount.Value);
			return MinimumSectorSystemCount.Value + Mathf.FloorToInt((SectorSystemChance.Value.Evaluate(value) * delta));
		}
		#endregion
	}
}