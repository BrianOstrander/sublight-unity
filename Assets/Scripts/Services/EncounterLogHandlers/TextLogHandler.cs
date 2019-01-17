using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class TextLogHandler : EncounterLogHandler<TextEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Text; } }

		public TextLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			TextEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var result = new TextHandlerModel();
			result.Log.Value = logModel;
			result.Message.Value = logModel.Message;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));

			linearDone();
		}
	}
}