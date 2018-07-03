using System;

using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class StarSystemView : SystemView
	{
		[Serializable]
		struct StateEntry
		{
			public SystemStates SystemState;
			public GameObject Root;
		}

		[SerializeField]
		StateEntry[] stateEntries;

		public override SystemTypes SystemType { get { return SystemTypes.Celestial; } }
		public override SystemStates SystemState
		{
			set
			{
				foreach (var entry in stateEntries) entry.Root.SetActive(entry.SystemState == value);
			}
		}

	}
}