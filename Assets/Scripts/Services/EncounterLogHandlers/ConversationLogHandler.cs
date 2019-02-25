using System;
using System.Linq;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ConversationLogHandler : EncounterLogHandler<ConversationEncounterLogModel>
	{
		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Conversation; } }

		public ConversationLogHandler(EncounterLogHandlerConfiguration configuration) : base(configuration) { }

		protected override void OnHandle(
			ConversationEncounterLogModel logModel,
			Action linearDone,
			Action<string> nonLinearDone
		)
		{
			var result = new ConversationHandlerModel(logModel);
			result.Entries.Value = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToArray();
			result.HaltingDone.Value = linearDone;

			Configuration.Callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}
	}
}