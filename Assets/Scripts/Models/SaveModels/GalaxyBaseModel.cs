﻿using System;
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
		[JsonProperty] UniversePosition playerStart;
		[JsonProperty] UniversePosition gameEnd;
		 
		[JsonProperty] int minimumSectorSystemCount;
		[JsonProperty] int maximumSectorSystemCount;
		[JsonProperty] AnimationCurve sectorSystemChance = AnimationCurveExtensions.Constant(1f);

		[JsonProperty] string encyclopediaEntryId;

		[JsonProperty] GalaxyLabelModel[] labels = new GalaxyLabelModel[0];

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
		public readonly ListenerProperty<UniversePosition> ClusterOrigin;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> GalaxyOrigin;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> PlayerStart;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> GameEnd;

		[JsonIgnore]
		public readonly ListenerProperty<int> MinimumSectorSystemCount;
		[JsonIgnore]
		public readonly ListenerProperty<int> MaximumSectorSystemCount;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> SectorSystemChance;

		[JsonIgnore]
		public readonly ListenerProperty<string> EncyclopediaEntryId;

		public GalaxyBaseModel()
		{
			IsPlayable = new ListenerProperty<bool>(value => isPlayable = value, () => isPlayable);
			GalaxyId = new ListenerProperty<string>(value => galaxyId = value, () => galaxyId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);

			UniverseNormal = new ListenerProperty<Vector3>(value => universeNormal = value, () => universeNormal);
			AlertHeightMultiplier = new ListenerProperty<float>(value => alertHeightMultiplier = value, () => alertHeightMultiplier);

			ClusterOrigin = new ListenerProperty<UniversePosition>(value => clusterOrigin = value, () => clusterOrigin);
			GalaxyOrigin = new ListenerProperty<UniversePosition>(value => galaxyOrigin = value, () => galaxyOrigin);
			PlayerStart = new ListenerProperty<UniversePosition>(value => playerStart = value, () => playerStart);
			GameEnd = new ListenerProperty<UniversePosition>(value => gameEnd = value, () => gameEnd);

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

		#region Utility
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

		public int SectorBodyCount(float value)
		{
			var delta = (MaximumSectorSystemCount.Value - MinimumSectorSystemCount.Value);
			return MinimumSectorSystemCount.Value + Mathf.FloorToInt((SectorSystemChance.Value.Evaluate(value) * delta));
		}
		#endregion
	}
}