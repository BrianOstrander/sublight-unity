using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class FudgedUniverseService : UniverseService
	{
		protected override UniversePosition GetPositionInSector(UniversePosition sectorPosition, int seed, int index, int systemCount)
		{
			switch(index)
			{
				case 0: return sectorPosition.NewLocal(new Vector3(0.1f, 0.05f, 0.0f));
				case 1: return sectorPosition.NewLocal(new Vector3(0.1f, 0.1f, 0.9f));
				case 2: return sectorPosition.NewLocal(new Vector3(0.9f, 0.15f, 0.1f));
				case 3: return sectorPosition.NewLocal(new Vector3(0.9f, 0.05f, 0.9f));
				case 4: return sectorPosition.NewLocal(new Vector3(0.5f, 0.17f, 0.5f));
				default:
					Debug.LogError("Out of bounds for this fudged generator");
					return sectorPosition;
			}
		}

		protected override SystemModel OnCreateSystem(SystemModel systemModel)
		{
			//systemModel.Name.Value = "Sys " + systemModel.Index.Value + " - [ " + systemModel.Position.Value.SectorInteger.x + " , " + systemModel.Position.Value.SectorInteger.z + " ]";
			systemModel.Name.Value = "Unnamed";
			systemModel.PrimaryClassification.Value = SystemClassifications.Stellar;
			systemModel.SecondaryClassification.Value = "Red Dwarf";
			systemModel.IconColor.Value = Color.HSVToRGB(0f, 0.53f, 1f);

			return systemModel;
		}
	}
}