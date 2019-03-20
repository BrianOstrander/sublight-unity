using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class FudgedUniverseService : UniverseService
	{
		public static Vector3[] LocalPositions =
		{
			new Vector3(0.1f, 0.05f, 0.0f),
			new Vector3(0.35f, 0.1f, 0.15f),
			new Vector3(0.1f, 0.1f, 0.9f),
			new Vector3(0.15f, 0.05f, 0.35f),
			new Vector3(0.9f, 0.15f, 0.1f),
			new Vector3(0.9f, 0.05f, 0.9f),
			new Vector3(0.5f, 0.17f, 0.5f),
			new Vector3(0.55f, 0.17f, 0.75f),
			new Vector3(0.3f, 0.05f, 0.5f),
			new Vector3(0.2f, 0.15f, 0.6f),
			new Vector3(0.75f, 0.1f, 0.45f),
			new Vector3(0.6f, 0.16f, 0.2f),
			new Vector3(0.25f, 0.05f, 0.85f),
			new Vector3(0.7f, 0.14f, 0.3f),
			new Vector3(0.4f, 0.1f, 0.8f),
			new Vector3(0.8f, 0.03f, 0.7f),
			new Vector3(0.85f, 0.13f, 0.65f)

			/*
			new Vector3(0.1f, 0.05f, 0.0f),
			new Vector3(0.1f, 0.1f, 0.9f),
			new Vector3(0.9f, 0.15f, 0.1f),
			new Vector3(0.9f, 0.05f, 0.9f),
			new Vector3(0.5f, 0.17f, 0.5f),

			new Vector3(0.3f, 0.05f, 0.5f),
			new Vector3(0.2f, 0.15f, 0.6f),
			new Vector3(0.6f, 0.16f, 0.2f),
			new Vector3(0.7f, 0.14f, 0.3f),
			new Vector3(0.4f, 0.1f, 0.8f),
			new Vector3(0.8f, 0.08f, 0.7f),

			new Vector3(0.35f, 0.1f, 0.15f),
			new Vector3(0.15f, 0.05f, 0.35f),
			new Vector3(0.55f, 0.17f, 0.75f),
			new Vector3(0.75f, 0.1f, 0.45f),
			new Vector3(0.25f, 0.05f, 0.85f),
			new Vector3(0.85f, 0.12f, 0.65f)
			*/
		};

		protected override UniversePosition GetPositionInSector(
			UniversePosition sectorPosition,
			int sectorOffset,
			int index,
			int systemCount
		)
		{
			var indexOffset = index + sectorOffset;
			if (LocalPositions.Length <= indexOffset) indexOffset -= LocalPositions.Length;

			return sectorPosition.NewLocal(LocalPositions[indexOffset]);
		}

		protected override SystemModel OnCreateSystem(SystemModel systemModel)
		{
			//systemModel.Name.Value = "Sys " + systemModel.Index.Value + " - [ " + systemModel.Position.Value.SectorInteger.x + " , " + systemModel.Position.Value.SectorInteger.z + " ]";
			systemModel.Name.Value = "Unnamed";

			var hackGenerator = new Demon(DemonUtility.CantorPairs(systemModel.Seed.Value, 42));

			var classification = hackGenerator.NextFloat;

			if (classification < 0.8f)
			{
				systemModel.PrimaryClassification.Value = SystemClassifications.Stellar;

				var classificationSecondary = hackGenerator.NextFloat;
				systemModel.IconScale.Value = classificationSecondary;

				if (classificationSecondary < 0.5f)
				{
					systemModel.SecondaryClassification.Value = "Red Dwarf";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0.53f, 1f);
				}
				else if (classificationSecondary < 0.65f)
				{
					systemModel.SecondaryClassification.Value = "Yellow Dwarf";
					systemModel.IconColor.Value = Color.HSVToRGB(0.161f, 0.53f, 1f);
				}
				else if (classificationSecondary < 0.75f)
				{
					systemModel.SecondaryClassification.Value = "Red Giant";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0.59f, 1f);
				}
				else if (classificationSecondary < 0.83f)
				{
					systemModel.SecondaryClassification.Value = "Blue Giant";
					systemModel.IconColor.Value = Color.HSVToRGB(0.586f, 0.61f, 1f);
				}
				else if (classificationSecondary < 0.9f)
				{
					systemModel.SecondaryClassification.Value = "Red Hypergiant";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0.63f, 1f);
				}
				else if (classificationSecondary < 0.96f)
				{
					systemModel.SecondaryClassification.Value = "Blue Hypergiant";
					systemModel.IconColor.Value = Color.HSVToRGB(0.555f, 0.61f, 1f);
				}
				else
				{
					// TODO: LOL DUPE
					systemModel.SecondaryClassification.Value = "Red Dwarf";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0.53f, 1f);
					//systemModel.SecondaryClassification.Value = "White Dwarf";
					//systemModel.IconColor.Value = Color.HSVToRGB(0f, 0f, 1f);
				}
			}
			else
			{
				systemModel.PrimaryClassification.Value = SystemClassifications.Degenerate;

				var classificationSecondary = hackGenerator.NextFloat;
				systemModel.IconScale.Value = classificationSecondary;

				if (classificationSecondary < 0.2f)
				{
					systemModel.SecondaryClassification.Value = "White Dwarf";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0f, 1f);
				}
				else if (classificationSecondary < 0.45f)
				{
					systemModel.SecondaryClassification.Value = "Pulsar";
					systemModel.IconColor.Value = Color.HSVToRGB(0.583f, 0.17f, 1f);
				}
				else
				{
					systemModel.SecondaryClassification.Value = "Black Hole";
					systemModel.IconColor.Value = Color.HSVToRGB(0f, 0f, 1f);
				}
			}

			if (hackGenerator.NextFloat < 0f) systemModel.KeyValues.Set(KeyDefines.CelestialSystem.PlanetCount, 0);
			else systemModel.KeyValues.Set(KeyDefines.CelestialSystem.PlanetCount, hackGenerator.GetNextInteger(1, GDCHackGlobals.PlanetCountMaximum + 1));

			SetHabitable(hackGenerator, systemModel, KeyDefines.CelestialSystem.HabitableAtmosphere);
			SetHabitable(hackGenerator, systemModel, KeyDefines.CelestialSystem.HabitableGravity);
			SetHabitable(hackGenerator, systemModel, KeyDefines.CelestialSystem.HabitableTemperature);
			SetHabitable(hackGenerator, systemModel, KeyDefines.CelestialSystem.HabitableWater);
			SetHabitable(hackGenerator, systemModel, KeyDefines.CelestialSystem.HabitableResources);

			SetScan(hackGenerator, systemModel, KeyDefines.CelestialSystem.ScanLevelAtmosphere);
			SetScan(hackGenerator, systemModel, KeyDefines.CelestialSystem.ScanLevelGravity);
			SetScan(hackGenerator, systemModel, KeyDefines.CelestialSystem.ScanLevelTemperature);
			SetScan(hackGenerator, systemModel, KeyDefines.CelestialSystem.ScanLevelWater);
			SetScan(hackGenerator, systemModel, KeyDefines.CelestialSystem.ScanLevelResources);

			if (hackGenerator.NextFloat < 0.25f) systemModel.KeyValues.Set(KeyDefines.CelestialSystem.AnomalyIndex, hackGenerator.GetNextInteger(1, GDCHackGlobals.AnomalyMaximum + 1));
			else systemModel.KeyValues.Set(KeyDefines.CelestialSystem.AnomalyIndex, 0);

			systemModel.SpecifiedEncounters.Value = new SpecifiedEncounterEntry[]
			{
				new SpecifiedEncounterEntry
				{
					Trigger = EncounterTriggers.NavigationSelect,
					EncounterId = "2ecf3799-264f-43b7-adec-383b09571ef5"
				}
			};

			var resourceGenerator = new Demon(SystemModel.Seeds.Resources(systemModel.Seed));

			systemModel.KeyValues.Set(KeyDefines.CelestialSystem.Rations.GatherMultiplier, resourceGenerator.NextFloat);
			systemModel.KeyValues.Set(KeyDefines.CelestialSystem.Propellant.GatherMultiplier, resourceGenerator.NextFloat);
			systemModel.KeyValues.Set(KeyDefines.CelestialSystem.Metallics.GatherMultiplier, resourceGenerator.NextFloat);

			return systemModel;
		}

		public void SetScan(
			Demon demon,
			SystemModel systemModel,
			KeyDefinitions.Integer key
		)
		{
			if (demon.NextFloat < 0.9f) systemModel.KeyValues.Set(key, 0);
			else systemModel.KeyValues.Set(key, demon.GetNextInteger(1, GDCHackGlobals.ScanLevelMaximum + 1));
		}

		public void SetHabitable(
			Demon demon,
			SystemModel systemModel,
			KeyDefinitions.Integer key
		)
		{
			systemModel.KeyValues.Set(key, demon.GetNextInteger(0, GDCHackGlobals.HabitableMaximum + 1));
		}
	}
}