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
		[JsonProperty] UniversePosition playerStart;
		[JsonProperty] UniversePosition gameEnd;

		[JsonProperty] AnimationCurve bodyAdjustment = AnimationCurveExtensions.Constant(1f);
		[JsonProperty] int minimumSectorBodies;
		[JsonProperty] int maximumSectorBodies;
		[JsonProperty] AnimationCurve sectorBodyChance;

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
		public readonly ListenerProperty<AnimationCurve> BodyAdjustment;
		[JsonIgnore]
		public readonly ListenerProperty<int> MinimumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<int> MaximumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> SectorBodyChance;

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

			BodyAdjustment = new ListenerProperty<AnimationCurve>(value => bodyAdjustment = value, () => bodyAdjustment);
			MinimumSectorBodies = new ListenerProperty<int>(value => minimumSectorBodies = value, () => minimumSectorBodies);
			MaximumSectorBodies = new ListenerProperty<int>(value => maximumSectorBodies = value, () => maximumSectorBodies);
			SectorBodyChance = new ListenerProperty<AnimationCurve>(value => sectorBodyChance = value, () => sectorBodyChance);
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

		public GalaxyLabelModel[] GetLabels(UniverseScales scale)
		{
			return labels.Where(l => l.Scale == scale).ToArray();
		}
		#endregion
	}
}