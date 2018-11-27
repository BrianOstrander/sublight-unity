using UnityEngine;
using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct UniverseTransform
	{
		public readonly Vector3 UnityOrigin;
		public readonly UniversePosition UniverseOrigin;
		public readonly float UnityRadius;

		public readonly Vector3 UnityToUniverse;
		public readonly Vector3 UniverseToUnity;

		public readonly Quaternion Rotation;

		public UniverseTransform(
			Vector3 unityOrigin,
			float unityRadius,
			UniversePosition universeOrigin,
			Vector3 unityToUniverse,
			Vector3 universeToUnity,
			Quaternion rotation
		)
		{
			UnityOrigin = unityOrigin;
			UnityRadius = unityRadius;
			UniverseOrigin = universeOrigin;
			UnityToUniverse = unityToUniverse;
			UniverseToUnity = universeToUnity;
			Rotation = rotation;
		}

		public Vector3 GetUnityScale(UniversePosition universeScale)
		{
			var sector = universeScale.Sector;
			var local = universeScale.Local;
			sector.Scale(UniverseToUnity);
			local.Scale(UniverseToUnity);
			return sector + local;
		}

		public Vector3 GetUnityPosition(UniversePosition universePosition)
		{
			var universeFromOrigin = universePosition - UniverseOrigin;
			var sector = universeFromOrigin.Sector;
			var local = universeFromOrigin.Local;
			sector.Scale(UniverseToUnity);
			local.Scale(UniverseToUnity);
			return UnityOrigin + (sector + local);
		}

		public UniversePosition GetUniverseScale(Vector3 unityScale)
		{
			var newScale = new Vector3(unityScale.x, unityScale.y, unityScale.z);
			newScale.Scale(UnityToUniverse);
			return new UniversePosition(newScale);
		}

		public UniversePosition GetUniversePosition(Vector3 unityPosition)
		{
			var unityFromOrigin = unityPosition - UnityOrigin;
			unityFromOrigin.Scale(UnityToUniverse);
			return UniverseOrigin + new UniversePosition(unityFromOrigin);
		}

		public Vector3 GetGridOffset(float localGridUnitSize)
		{
			// Kinda magic, but basically at some point there are more than 1
			// units per sector, so we have to change how we find the offset.
			var target = localGridUnitSize < UniversePosition.LightYearToUniverseScalar ? UniverseOrigin.Local : UniverseOrigin.Lossy;
			var gridUnitsPerSector = 1f / localGridUnitSize;
			return new Vector3(
				GetGridAxisOffset(target.x, gridUnitsPerSector),
				GetGridAxisOffset(target.y, gridUnitsPerSector),
				GetGridAxisOffset(target.z, gridUnitsPerSector)
			);
		}

		float GetGridAxisOffset(float localValue, float gridUnitsPerSector)
		{
			return (gridUnitsPerSector * localValue) % 1f;
		}

		// TODO: add where direction of goal is...

		public UniverseTransform Duplicate(
			UniversePosition? universeOrigin = null
		)
		{
			return new UniverseTransform(
				UnityOrigin,
				UnityRadius,
				universeOrigin.HasValue ? universeOrigin.Value : UniverseOrigin,
				UnityToUniverse,
				UniverseToUnity,
				Rotation
			);
		}
	}
}