using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DestructionSystemPresenter : Presenter<IDestructionSystemView>
	{
		GameModel model;

		public DestructionSystemPresenter(GameModel model)
		{
			this.model = model;

			model.DestructionRadius.Changed += OnDestructionRadius;
			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.DestructionRadius.Changed -= OnDestructionRadius;
			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = UniversePosition.Zero;
			View.Radius = model.DestructionRadius;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			ShowView(instant: true);
		}

		#region Events
		void OnDayTimeDelta(DayTimeDelta delta)
		{
			model.DestructionRadius.Value += delta.Delta.TotalDays * model.DestructionSpeed;
		}

		void OnDestructionRadius(float radius)
		{
			if (!View.Visible) return;
			View.Radius = model.DestructionRadius;
		}

		void OnHighlight(bool highlighted)
		{
			App.Log("Highlight", LogTypes.ToDo);
		}

		void OnClick()
		{
			App.Log("Click", LogTypes.ToDo);
		}
		#endregion
	}
}