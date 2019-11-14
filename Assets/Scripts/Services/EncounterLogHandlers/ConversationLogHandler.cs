using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ConversationLogHandler : EncounterLogHandler<ConversationEncounterLogModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Conversation;

		public ConversationLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ConversationEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var request = new ConversationHandlerModel(logModel);
			request.Entries.Value = logModel.Edges.Value.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).ToArray();
			request.HaltingDone.Value = linearDone;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(request));
		}
	}
}