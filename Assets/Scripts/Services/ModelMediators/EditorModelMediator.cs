using System.Collections.Generic;

namespace LunraGames.SpaceFarm
{
	public class EditorModelMediator : DesktopModelMediator 
	{
		Dictionary<SaveTypes, bool> CanSaveOverrides
		{
			get
			{
				return new Dictionary<SaveTypes, bool>
				{
					{ SaveTypes.EncounterInfo, true },
					// -- Inventory References
					{ SaveTypes.ModuleReference, true },
					{ SaveTypes.OrbitalCrewReference, true }
					// --
				};
			}
		}

		protected override Dictionary<SaveTypes, bool> CanSave
		{
			get
			{
				var dict = base.CanSave;
				var overrideDict = CanSaveOverrides;

				foreach (var kv in overrideDict)
				{
					if (dict.ContainsKey(kv.Key)) dict[kv.Key] = kv.Value;
					else dict.Add(kv.Key, kv.Value);
				}

				return dict;
			}
		}

		public EditorModelMediator(bool readableSaves = false) : base(readableSaves) {}
	}
}