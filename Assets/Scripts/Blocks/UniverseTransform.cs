﻿using UnityEngine;
using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct UniverseTransform
	{
		//public static UniverseTransform Default
		//{
		//	get
		//	{
				
		//	}
		//}

		public readonly Vector3 UnityOrigin;
		public readonly UniversePosition UniverseOrigin;

		public readonly Vector3 UnityToUniverse;
		public readonly Vector3 UniverseToUnity;

		public readonly Quaternion Rotation;

		public UniverseTransform(
			Vector3 unityOrigin,
			UniversePosition universeOrigin,
			Vector3 unityToUniverse,
			Vector3 universeToUnity,
			Quaternion rotation
		)
		{
			UnityOrigin = unityOrigin;
			UniverseOrigin = universeOrigin;
			UnityToUniverse = unityToUniverse;
			UniverseToUnity = universeToUnity;
			Rotation = rotation;
		}

		public Vector3 GetUnityPosition(UniversePosition universePosition)
		{
			var universeFromOrigin = (universePosition - UniverseOrigin);
			var sector = universeFromOrigin.Sector;
			var system = universeFromOrigin.System;
			sector.Scale(UniverseToUnity);
			system.Scale(UniverseToUnity);
			return UnityOrigin + (sector + system);
		}

		public UniversePosition GetUniversePosition(Vector3 unityPosition)
		{
			return UniversePosition.Zero; // todo
		}

		// TODO: add where direction of goal is...

	}
}