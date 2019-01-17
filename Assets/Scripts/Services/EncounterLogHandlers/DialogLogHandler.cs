using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class DialogLogHandler : EncounterLogHandler<DialogEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Dialog; } }

		public DialogLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			DialogEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{

			Debug.LogError("TODO: Impliment this!");
			//throw new NotImplementedException();
		}
	}
}