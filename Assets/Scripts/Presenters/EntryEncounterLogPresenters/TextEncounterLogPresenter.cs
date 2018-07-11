using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class TextEncounterLogPresenter : EntryEncounterLogPresenter<TextEncounterLogModel, ITextEncounterLogView>
	{
		public TextEncounterLogPresenter(GameModel model, TextEncounterLogModel logModel) : base(model, logModel) {}

		protected override void OnShow()
		{
			View.Header = LogModel.Header;
			View.Message = LogModel.Message;
		}
	}
}