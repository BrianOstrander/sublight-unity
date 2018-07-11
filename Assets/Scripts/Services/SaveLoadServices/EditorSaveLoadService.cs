using System.Collections.Generic;

namespace LunraGames.SpaceFarm
{
	public class EditorSaveLoadService : DesktopSaveLoadService 
	{
		protected override Dictionary<SaveTypes, bool> CanSave
		{
			get
			{
				var dict = base.CanSave;
				if (dict.ContainsKey(SaveTypes.EncounterInfo)) dict[SaveTypes.EncounterInfo] = true;
				else dict.Add(SaveTypes.EncounterInfo, true);
				return dict;
			}
		}

		public EditorSaveLoadService(bool readableSaves = false) : base(readableSaves) {}
	}
}